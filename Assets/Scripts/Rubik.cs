using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rubik : MonoBehaviour
{

    public float _rotationSpeed = 0.5f;

    int _numberOfCubesFace = 16;
    int _columns;
    float _scale = 1.0f;
    float _center = 0.0f;
    Vector3 _initialMovemment;

    GameObject[] _cubes;


    private GameObject[] face;
    private GameObject rightPivot;
    private int i;

    private void Start()
    {

        this._columns = Mathf.RoundToInt(Mathf.Sqrt(this._numberOfCubesFace));
        this._center = (_columns * _scale - _scale) / 2;

        Vector3 _initialMovemment = new Vector3(
            this.gameObject.transform.position.x - _center,
            this.gameObject.transform.position.y - _center,
            this.gameObject.transform.position.z - _center);

        Debug.Log(_initialMovemment);
        generateRubikCube(_numberOfCubesFace);
        face = new GameObject[_numberOfCubesFace];
        createRotateRow(1, Vector3.forward);
    }

    void generateRubikCube(int numberOfCubesFace)
    {

        GameObject c = Resources.Load("Prefabs/Cube", typeof(GameObject)) as GameObject;

        int numberOfCubes = (numberOfCubesFace * _columns);

        _cubes = new GameObject[numberOfCubes];
        int index = 0;
        List<int> faces = new List<int>();


        this.gameObject.transform.localScale = new Vector3(_columns * _scale, _columns * _scale, _columns * _scale);
               
        for (int z = 0; z < _columns; z++)
        {
            for (int y = 0; y < _columns; y++)
            {
                for (int x = 0; x < _columns; x++)
                {
                    faces.Clear();
                    if (x == 0)
                    {
                        faces.Add(0);
                    }
                    if (y == 0)
                    {
                        faces.Add(1);
                    }
                    if (z == 0)
                    {
                        faces.Add(2);
                    }
                    if (x == (_columns - 1))
                    {
                        faces.Add(3);
                    }
                    if (y == (_columns - 1))
                    {
                        faces.Add(4);
                    }
                    if (z == (_columns - 1))
                    {
                        faces.Add(5);
                    }
                    _cubes[index] = GameObject.Instantiate(c, new Vector3(x, y, z), Quaternion.identity);
                    _cubes[index].GetComponent<Cube>().setData(_scale, new Vector3(x, y, z), _initialMovemment, Quaternion.identity, this.transform, faces.ToArray(), this);

                    index++;
                }
            }
        }
    }

    
    void createRotateRow(int column, Vector3 direction)
    {
        // right --> vertical
        // up --> horizontal
        // front --> lateral
        i = 0;
        rightPivot = new GameObject("rightPivot");
        Vector3 v = Vector3.one;
        if (direction.x == 1)
        {
            v.x = 0;
        } else if (direction.y == 1)
        {
            v.y = 0;
        }else if (direction.z == 1)
        {
            v.z = 0;
        }
        rightPivot.transform.position = direction * (column * _scale) + (v * _center);
        rightPivot.transform.parent = gameObject.transform;
        foreach (GameObject cube in _cubes)
        {
            if (Vector3.Scale(cube.transform.position, direction) == column * direction)
            {
                cube.transform.parent = rightPivot.transform;
                face[i] = cube;
                i++;
            }
        }
    }

    float speed = 15.0f; 
    bool mouseClicked = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseClicked = true;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            mouseClicked = false;
        }
        if (mouseClicked)
        {
            transform.Rotate(-new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * speed);
        }
    }
}
