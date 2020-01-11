using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rubik : MonoBehaviour
{

    public float _rotationSpeed = 0.5f;

    int _columns = 3;
    int _numberOfCubesFace;
    float _scale = 1.0f;
    float _center = 0.0f;

    Vector3 _initialMovemment;

    GameObject[] _cubes;

    bool _cubeClicked = false;
    
    GameObject[] _face;
    GameObject _pivot;

    const float _angleSpeed = 15;
    const float _initialAngle = 90;

    Vector3 _rotatingVector = Vector3.zero;
    float _angleLeft;

    private void Start()
    {

        this._numberOfCubesFace = Mathf.RoundToInt(Mathf.Pow(this._columns, 2));
        this._center = (_columns * _scale - _scale) / 2;
    
        generateRubikCube(_numberOfCubesFace);
        _face = new GameObject[_numberOfCubesFace];

        transform.position = Camera.main.transform.position + Camera.main.transform.forward * _columns;
        Camera.main.orthographicSize = _columns + 2;
    }

    public void setCubeClicked(bool clicked)
    {
        this._cubeClicked = clicked;
    }

    void generateRubikCube(int numberOfCubesFace)
    {

        GameObject c = Resources.Load("Prefabs/Cube", typeof(GameObject)) as GameObject;

        int numberOfCubes = (numberOfCubesFace * _columns);

        _cubes = new GameObject[numberOfCubes];
        int index = 0;
        List<int> faces = new List<int>();


        this.gameObject.transform.localScale = new Vector3(_columns * _scale, _columns * _scale, _columns * _scale);
        _initialMovemment = new Vector3(
            this.gameObject.transform.position.x - _center,
            this.gameObject.transform.position.y - _center,
            this.gameObject.transform.position.z - _center);

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
                    _cubes[index].GetComponent<Cube>().setData(_scale, new Vector3(x, y, z), _initialMovemment, Quaternion.identity, this.transform, faces.ToArray(), this);

                    index++;
                }
            }
        }
    }

    
    public void createRotateRow(Vector3 cube, Vector3 direction)
    {
        Vector3 currentCubePivot = Vector3.Scale(cube, direction);         
        
        int index = 0;

        float minX = _columns * _scale, minY = minX, minZ = minX;
        float maxX = -minX, maxY = -minY, maxZ = -minZ;
        bool rowEmpty = true;

        foreach (GameObject c in _cubes)
        {
            if (Vector3.Scale(c.transform.position, direction) == currentCubePivot)
            {
                _face[index] = c;
                index++;

                if (c.transform.position.x > maxX) maxX = c.transform.position.x;
                else if (c.transform.position.x < minX) minX = c.transform.position.x;

                if (c.transform.position.y > maxY) maxY = c.transform.position.y;
                else if (c.transform.position.y < minY) minY = c.transform.position.y;

                if (c.transform.position.z > maxZ) maxZ = c.transform.position.z;
                else if (c.transform.position.z < minZ) minZ = c.transform.position.z;

                rowEmpty = false;
            }
        }
        if (!rowEmpty)
        {
            _pivot = new GameObject("Row");
            _pivot.transform.position = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
            _pivot.transform.parent = gameObject.transform;

            // Attach elements of _face array to pivot
            foreach (GameObject c in _face)
            {
                c.transform.parent = _pivot.transform;
            }
            _rotatingVector = direction;
            _angleLeft = _initialAngle;
        }
    }
    
    void Update()
    {
        if (!this._cubeClicked)
        {
            Vector3 currentSwipe = Swipe.getSwipe();

            if (currentSwipe != Vector3.zero)
            {
                _rotatingVector = currentSwipe;
                _angleLeft = _initialAngle;
            }
        }
        if (_angleLeft > 0)
        {
            if (_pivot != null)
            {
                _pivot.transform.Rotate(_rotatingVector, _angleSpeed, Space.World);
            }
            else if (_rotatingVector != Vector3.zero)
            {
                transform.Rotate(_rotatingVector, _angleSpeed, Space.World);
            }

            if (_angleLeft - _angleSpeed <= 0)
            {
                _angleLeft = 0;
                _rotatingVector = Vector3.zero;
                removePivot();
            }
            else
            {
                _angleLeft -= _angleSpeed;
            }
        }
    }

    void removePivot()
    {
        // Desatach elements of _face array from pivot
        foreach (GameObject f in _face)
        {
            if (f != null)
            {
                f.transform.parent = transform;
            }
        }

        // Remove pivot
        if (_pivot != null)
        {
            Destroy(_pivot);
        }

    }
}
