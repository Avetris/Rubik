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

    private void Start()
    {
        _preview = !SceneManager.GetActiveScene().name.Equals("GameScene");
        if (!_preview) { 
            _columns = PlayerPrefs.GetInt(Constants.SHARED_PREFERENCES.RUBIK_SIZE.ToString(), _columns);

            _numberOfCubesFace = Mathf.RoundToInt(Mathf.Pow(_columns, 2));
            _center = (_columns * _scale - _scale) / 2;

            generateRubikCube(_numberOfCubesFace);

            Camera.main.transform.position = transform.position - Camera.main.transform.forward * _columns;
            Camera.main.orthographicSize = _columns + 2;

            sample();
        }
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
            Vector3 v = c.transform.position;
            if(v.x == _initialMovemment.x || v.y == _initialMovemment.y || v.z == _initialMovemment.z ||
                v.x == -_initialMovemment.x || v.y == -_initialMovemment.y || v.z == -_initialMovemment.z)
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
                    Debug.Log("COMPLETED");
                }
                else
                {
                    Debug.Log("NOT COMPLETED");

                }
            }
        }
    }

    bool isCompleted()
    {
        int index = 0;
        Vector3 nullVector = new Vector3(999, 999, 999);
        Vector3[] rotation = new Vector3[6] {
            nullVector,
            nullVector,
            nullVector,
            nullVector,
            nullVector,
            nullVector
        };

        foreach (GameObject c in _cubes)
        {
            if (c.transform.position.x == -_initialMovemment.x) index = 0;
            else if (c.transform.position.x == _initialMovemment.x) index = 1;
            else if (c.transform.position.y == -_initialMovemment.y) index = 2;
            else if (c.transform.position.y == _initialMovemment.y) index = 3;
            else if (c.transform.position.z == -_initialMovemment.z) index = 4;
            else if (c.transform.position.z == _initialMovemment.z) index = 5;

            if (!checkEqualRotation(nullVector, rotation, index, c.transform.rotation)) return false;
        }
        return true;
    }

    bool checkEqualRotation(Vector3 nullVector, Vector3[] generalRotation, int index, Quaternion currentRotation)
    {
        if (generalRotation[index] == nullVector)
        {
            generalRotation[index].x = currentRotation.x;
            generalRotation[index].y = currentRotation.y;
            generalRotation[index].z = currentRotation.z;
        }
        return generalRotation[index].x == currentRotation.x && generalRotation[index].y == currentRotation.y && generalRotation[index].z == currentRotation.z;
    }

    void Update()
    {
        if (!_preview)
        {
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

    private void OnGUI()
    {
        if (!_preview)
        {
            if (GUI.Button(new Rect(20, 40, 200, 200), "Space"))
            {
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
}
