using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rubik : MonoBehaviour
{
    public GameObject _pausePanel, _endPanel;
    public GameObject _timeText;

    bool _showingMenu = false;

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

    float _time = 0;
    bool _build = false;
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
        _showingMenu = false;
        _time = 0;
        _build = true;
        _pausePanel.SetActive(false);
        _endPanel.SetActive(false);
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
            Vector3 currentCubePivot = Vector3.Scale(_cubeClicked, _rotatingVector).normalized;
            currentCubePivot.x = (float)Math.Round(currentCubePivot.x, 2);
            currentCubePivot.y = (float)Math.Round(currentCubePivot.y, 2);
            currentCubePivot.z = (float)Math.Round(currentCubePivot.z, 2);

            int index = 0;
            
            int rowEmpty = _numberOfCubesFace;

            float middle = _scale * _columns / 2;

            _pivot = new GameObject("Row");
            _pivot.transform.position = currentCubePivot * middle;
            _pivot.transform.parent = gameObject.transform;

            _face = new GameObject[_numberOfCubesFace];
            foreach (GameObject c in _cubes)
            {
                Vector3 scale = Vector3.Scale(c.transform.position, _rotatingVector).normalized;
                scale.x = (float)Math.Round(scale.x, 2);
                scale.y = (float)Math.Round(scale.y, 2);
                scale.z = (float)Math.Round(scale.z, 2);

                if (scale == currentCubePivot)
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
                    LeaderboardManager.instance().setPuntuation(Mathf.RoundToInt(_time), _currentLeaderboard);
                    showMenu(1);
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
        Vector3 v1 = _cubes[0].transform.rotation.eulerAngles;
        v1.x = (float) Math.Round(v1.x, 1);
        v1.y = (float)Math.Round(v1.y, 1);
        v1.z = (float)Math.Round(v1.z, 1);

        foreach (GameObject c in _cubes)
        {
            Vector3 v2 = c.transform.rotation.eulerAngles;
            v2.x = (float)Math.Round(v2.x, 2);
            v2.y = (float)Math.Round(v2.y, 2);
            v2.z = (float)Math.Round(v2.z, 2);

            if (v1 != v2)
            {
                return false;
            }
        }
        return true;
    }

    private void Update()
    {
        int i = 0;
        while (i < Input.touchCount)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
                
                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, 10.0f))
                {
                    //RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position), -Vector2.up);

                    setCubeClicked(hit.collider.gameObject.transform.position);

                }
            }
            ++i;
        }
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                showMenu(0);
            }
        }
    }

    void FixedUpdate()
    {
        if (!_preview)
        {
            if (!_showingMenu)
            {
                if (_time < Constants.TIME_LIMIT)
                {
                    _time += Time.deltaTime;
                    _timeText.GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(_time).ToString();
                }
                else
                {
                    showMenu(2);
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
            }
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

    public void action(int action)
    {
        _showingMenu = false;
        if (action == 0)
        {
            _pausePanel.SetActive(false);
            _endPanel.SetActive(false);
        }
        else
        {
            _adsManager.showInterstical();
            switch (action)
            {
                case 1: restart(); break;
                case 2: SceneManager.LoadScene("MenuScene"); break;
            }
        }
    }

    public void showMenu(int type)
    {
        if (!_showingMenu)
        {
            _showingMenu = true;
            if (type == 0)
            {
                _pausePanel.SetActive(true);
            }
            else
            {
                if (type == 1)
                {
                    _endPanel.transform.GetComponentInChildren<TextMeshProUGUI>().text = _locale.getWord("rubik_finish");
                }
                else
                {
                    _endPanel.transform.GetComponentInChildren<TextMeshProUGUI>().text = _locale.getWord("time_limit");
                }
                _endPanel.SetActive(true);
            }
        }
    }
}
