using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rubik : MonoBehaviour
{
    public float _rotationSpeed = 0.5f;

    int _columns = 3;
    int _numberOfCubesFace;
    float _scale = 1.0f;
    float _center = 0.0f;

    Vector3 _initialMovemment;

    GameObject[] _cubes;

    bool _anyCubeClicked = false;
    Vector3 _cubeClicked = Vector3.zero;

    GameObject[] _face = new GameObject[0];
    GameObject _pivot;

    Vector3 _rotatingVector = Vector3.zero;
    float _angleLeft;

    bool _preview = false;

    GUISkin _gameSkin;
    public Texture2D[] _icons;

    float _time = 0;
    int _menuEnabled = 0;
    bool _build = false;
    int _finished = 0;
    Locale _locale;
    AdsManager _adsManager;
    LeaderboardManager.LEADERBOARD _currentLeaderboard = LeaderboardManager.LEADERBOARD.NONE; 

    private void Start()
    {
        _preview = !SceneManager.GetActiveScene().name.Equals("GameScene");
        if (!_preview)
        {
            _build = false;
            _adsManager = AdsManager.instance();
            _locale = Locale.instance(Application.systemLanguage);
            _gameSkin = Resources.Load<GUISkin>("Styles/gameSkin");
            _columns = PlayerPrefs.GetInt(Constants.SHARED_PREFERENCES.RUBIK_SIZE.ToString(), _columns);
            if (_columns == 2) _currentLeaderboard = LeaderboardManager.LEADERBOARD.TWO;
            else if (_columns == 3) _currentLeaderboard = LeaderboardManager.LEADERBOARD.THREE;
            else if (_columns == 4) _currentLeaderboard = LeaderboardManager.LEADERBOARD.FOUR;
            else if (_columns == 5) _currentLeaderboard = LeaderboardManager.LEADERBOARD.FIVE;

            _numberOfCubesFace = Mathf.RoundToInt(Mathf.Pow(_columns, 2));
            _center = (_columns * _scale - _scale) / 2;

            generateRubikCube(_numberOfCubesFace);

            Camera.main.transform.position = transform.position - Camera.main.transform.forward * _columns;
            Camera.main.orthographicSize = _columns + 2;

            sample();
            _build = true;
        }
    }

    void restart()
    {
        _build = false;
        sample();
        _menuEnabled = 0;
        _finished = 0;
        _time = 0;
        _build = true;
    }

    public void generateRubikPreview(int columns, float height)
    {
        _columns = columns;

        _numberOfCubesFace = Mathf.RoundToInt(Mathf.Pow(_columns, 2));
        _center = (_columns * _scale - _scale) / 2;

        generateRubikCube(_numberOfCubesFace);
        
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, height, 5));
        transform.position = worldPoint;
        transform.localScale /= columns;
        transform.rotation = Quaternion.Euler(-39, 62, 40);
    }

    void generateRubikCube(int numberOfCubesFace)
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        GameObject c = Resources.Load("Prefabs/Cube", typeof(GameObject)) as GameObject;

        int numberOfCubes = (numberOfCubesFace * _columns);

        _cubes = new GameObject[numberOfCubes];
        int index = 0;
        List<int> faces = new List<int>();
        
        gameObject.transform.localScale = new Vector3(_columns * _scale, _columns * _scale, _columns * _scale);
        _initialMovemment = new Vector3(
            gameObject.transform.position.x - _center,
            gameObject.transform.position.y - _center,
            gameObject.transform.position.z - _center);
        
        for (int z = 0; z < _columns; z++)
        {
            for (int y = 0; y < _columns; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    faces.Clear();
                    if (x == 0)
                    {
                        faces.Add(Constants.FACE.LEFT);
                    }
                    if (y == 0)
                    {
                        faces.Add(Constants.FACE.UP);
                    }
                    if (z == 0)
                    {
                        faces.Add(Constants.FACE.FRONT);
                    }
                    if (x == (_columns - 1))
                    {
                        faces.Add(Constants.FACE.RIGHT);
                    }
                    if (y == (_columns - 1))
                    {
                        faces.Add(Constants.FACE.DOWN);
                    }
                    if (z == (_columns - 1))
                    {
                        faces.Add(Constants.FACE.BACK);
                    }
                    _cubes[index] = Instantiate(c, new Vector3(x, y, z), Quaternion.identity);
                    _cubes[index].GetComponent<Cube>().setData(_scale, new Vector3(x, y, z), _initialMovemment, Quaternion.identity, transform, faces.ToArray(), this);

                    index++;
                }
            }
        }
    }

    void sample()
    {
        int iterations = UnityEngine.Random.Range(Constants.MIN_SAMPLE_ITERATIONS, Constants.LIMIT_SAMPLE_ITERATIONS);
        GameObject[] auxCubes = new GameObject[_numberOfCubesFace * 2 + ((_columns * (_columns - 2)) * 2) + (((_columns - 2) * (_columns - 2)) * 2)];

        int index = 0;
        foreach (GameObject c in _cubes)
        {
            if (c.GetComponent<Cube>().hasFaces())
            {
                auxCubes[index] = c;
                index++;
            }
        }

        while (iterations > 0)
        {
            _cubeClicked = auxCubes[UnityEngine.Random.Range(0, auxCubes.Length - 1)].transform.position;
            _rotatingVector = Vector3.zero;
            switch (UnityEngine.Random.Range(0, 5))
            {
                case 0: _rotatingVector = Vector3.right; break;
                case 1: _rotatingVector = Vector3.left; break;
                case 2: _rotatingVector = Vector3.up; break;
                case 3: _rotatingVector = Vector3.down; break;
                case 4: _rotatingVector = Vector3.forward; break;
                case 5: _rotatingVector = Vector3.back; break;
            }
            createRotateRow();
            if (_pivot != null && _face != null && _face.Length == _numberOfCubesFace)
            {
                _pivot.transform.Rotate(_rotatingVector, Constants.ROTATION_ANGLE, Space.World);
                removePivot();
                iterations--;
            }
        }
    }

    public void setCubeClicked(Vector3 cubeClicked)
    {
        if (!_anyCubeClicked)
        {
            _anyCubeClicked = true;
            _cubeClicked = cubeClicked;
        }
    }
 
    void createRotateRow()
    {
        if (_pivot == null && (_face == null || _face.Length == 0))
        {
            Vector3 currentCubePivot = Vector3.Scale(_cubeClicked, _rotatingVector);

            int index = 0;
            
            int rowEmpty = _numberOfCubesFace;

            float middle = _scale * _columns / 2;

            _pivot = new GameObject("Row");
            _pivot.transform.position = currentCubePivot * middle;
            _pivot.transform.parent = gameObject.transform;

            _face = new GameObject[_numberOfCubesFace];
            foreach (GameObject c in _cubes)
            {
                if (Vector3.Scale(c.transform.position, _rotatingVector) == currentCubePivot)
                {
                    if (index < _face.Length)
                    {
                        c.transform.parent = _pivot.transform;
                        _face[index] = c;
                        index++;
                    }
                    rowEmpty--;
                }
            }
            if (rowEmpty == 0)
            {
                _angleLeft = Constants.ROTATION_ANGLE;
            }
            else
            {
                if(_pivot != null)
                {
                    removePivot();
                }
            }
        }
    }

    void rotate()
    {
        if (_angleLeft > 0)
        {
            if (_pivot != null)
            {
                _pivot.transform.Rotate(_rotatingVector, Constants.ROTATION_SPEED, Space.World);
            }
            else if (_rotatingVector != Vector3.zero)
            {
                transform.Rotate(_rotatingVector, Constants.ROTATION_SPEED, Space.World);
            }

            _angleLeft -= Constants.ROTATION_SPEED;
            if (_angleLeft <= 0)
            {
                _angleLeft = 0;
                removePivot();
                
                if (isCompleted())
                {
                    LeaderboardManager.instance().setPuntuation(Mathf.RoundToInt(_time * 1000), _currentLeaderboard);
                    _finished = 1;
                }
            }
        }
    }

    bool isCompleted()
    {
        if (!_build)
        {
            return false;
        }
        Quaternion rotation = _cubes[0].transform.rotation;
        rotation.x = (float) Math.Round(rotation.x, 1);
        rotation.y = (float)Math.Round(rotation.y, 1);
        rotation.z = (float)Math.Round(rotation.z, 1);
        rotation.w = (float)Math.Round(rotation.w, 1);

        foreach (GameObject c in _cubes)
        {
            Quaternion q = c.transform.rotation;
            q.x = (float) Math.Round(q.x, 1);
            q.y = (float)Math.Round(q.y, 1);
            q.z = (float)Math.Round(q.z, 1);
            q.w = (float)Math.Round(q.w, 1);
            if (!(rotation.x == q.x
            && rotation.y == q.y
            && rotation.z == q.z
            && rotation.w == q.w))
            {
                return false;
            }
        }
        return true;
    }

    void Update()
    {
        if (!_preview)
        {
            if (_menuEnabled == 0 && _finished == 0)
            {
                if (_time < Constants.TIME_LIMIT)
                {
                    _time += Time.deltaTime;
                }
                else
                {
                    _finished = -1;
                }
                if (_angleLeft <= 0)
                {
                    _rotatingVector = Swipe.getSwipe();
                    if (_rotatingVector != Vector3.zero)
                    {
                        if (_anyCubeClicked)
                        {
                            createRotateRow();
                        }
                        else
                        {
                            if (_rotatingVector != Vector3.zero)
                            {
                                _angleLeft = Constants.ROTATION_ANGLE;
                            }
                        }
                    }
                }
                rotate();
            }else if (_menuEnabled == 1)
            {
                _menuEnabled--;
            }
        }
    }

    private void OnGUI()
    {
        if (!_preview)
        {
            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(50, 50, Screen.width / 8, Screen.width / 8), "", _gameSkin.GetStyle("settings")))
            {
                _menuEnabled = 2;
            }
            GUI.Label(new Rect(60 + Screen.width / 8, 50, Screen.width - (60 + Screen.width / 8), Screen.width / 8), Mathf.RoundToInt(_time).ToString(), _gameSkin.GetStyle("time"));

            if (_menuEnabled == 2)
            {
                GUI.backgroundColor = Color.blue;
                GUI.Window(0, new Rect(Screen.width / 6, Screen.height / 5, 2 * Screen.width / 3, 2 * Screen.height / 5), ShowMenu, "", _gameSkin.window);
            }
            else if (_finished != 0)
            {
                GUI.backgroundColor = Color.blue;
                GUI.Window(0, new Rect(Screen.width / 8, Screen.height / 5, 3 * Screen.width / 4, 2 * Screen.height / 5), ShowFinished, "", _gameSkin.window);
            }

        }
    }
    void ShowMenu(int windowID)
    {
        float width = 2 * Screen.width / 3;
        float height = Screen.height / 2;
        GUI.Label(new Rect(40, 40, width - 80, (height - 100) / 2), _locale.getWord("menu_text"), _gameSkin.GetStyle("menuText"));

        float y = -60 + (height - 100) / 2;

        GUI.backgroundColor = Color.green;
        if (GUI.Button(new Rect(40, y, (width - 100) / 3, (height - 100) / 2), _icons[0], _gameSkin.button))
        {
            _menuEnabled = 1;
        }

        GUI.backgroundColor = Color.yellow;
        if (GUI.Button(new Rect(50 + (width - 100) / 3, y, (width - 100) / 3, (height - 100) / 2), _icons[1], _gameSkin.button))
        {
            restart();
            _adsManager.showInterstical();
        }

        GUI.backgroundColor = Color.red;
        if (GUI.Button(new Rect(60 + (2 * (width - 100) / 3), y, (width - 100) / 3, (height - 100) / 2), _icons[2], _gameSkin.button))
        {
            SceneManager.LoadScene("MenuScene");
            _adsManager.showInterstical();
        }
    }

    void ShowFinished(int windowID)
    {
        float width = 3 * Screen.width / 4;
        float height = Screen.height / 2;
        GUI.Label(new Rect(40, 40, width - 80, (height - 100) / 2), _locale.getWord(_finished > 0 ? "rubik_finish" : "time_limit"), _gameSkin.GetStyle("menuText"));

        float y = -60 + (height - 100) / 2;

        GUI.backgroundColor = Color.yellow;
        if (GUI.Button(new Rect(40 , y, (width - 100) / 3, (height - 100) / 2), _icons[1], _gameSkin.button))
        {
            restart();
            _adsManager.showInterstical();
        }

        GUI.backgroundColor = Color.green;
        if (GUI.Button(new Rect(40 + (2 * (width - 100) / 3), y, (width - 100) / 3, (height - 100) / 2), _icons[3], _gameSkin.button))
        {
            SceneManager.LoadScene("MenuScene");
            _adsManager.showInterstical();
        }
    }

    void removePivot()
    {
        // Desatach elements of _face array from pivot
        if(_face != null)
        {
            foreach (GameObject f in _face)
            {
                if (f != null)
                {
                    f.transform.parent = transform;
                }
            }
        }

        // Remove pivot
        if (_pivot != null)
        {
            Destroy(_pivot);
            _pivot = null;
        }

        _face = new GameObject[0];

        _rotatingVector = Vector3.zero;
        _cubeClicked = Vector3.zero;
        _anyCubeClicked = false;
    }
}
