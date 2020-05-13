using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{   
    int[] _showFaces;
    Rubik _parent;

    public void setData(float scale, Vector3 position, Vector3 movemment, Quaternion rotation, Transform transform, int[] showFaces, Rubik parent)
    {
        _showFaces = showFaces;
        _parent = parent;
        setColor();
        setScale(scale);
        setPosition((scale * position) + movemment, rotation);
        gameObject.transform.parent = transform;
    }

    public bool hasFaces()
    {
        return _showFaces != null && _showFaces.Length > 0;
    }

    public void setScale(float scale)
    {
        gameObject.GetComponent<Transform>().localScale = new Vector3(scale, scale, scale);

    }

    public void setPosition(Vector3 position, Quaternion rotation)
    {
        gameObject.transform.SetPositionAndRotation(position, rotation); 
    }

    void setColor()
    {
        Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        
        // create new colors array where the colors will be created.
        Color[] colors = new Color[vertices.Length];

        int k = 0;
        Color color = Color.black;
        for (int i = 0; i < vertices.Length; i++)
        {
            switch (k)
            {
                case 0: color = containsFace(Constants.FACE.BACK) ? Color.red : Color.black; break;
                case 4: color = containsFace(Constants.FACE.DOWN) ? Color.white : Color.black; break;
                case 6: color = containsFace(Constants.FACE.FRONT) ? Color.green : Color.black; break;
                case 8: color = containsFace(Constants.FACE.DOWN) ? Color.white : Color.black; break;
                case 10: color = containsFace(Constants.FACE.FRONT) ? Color.green : Color.black; break;
                case 12: color = containsFace(Constants.FACE.UP) ? Color.yellow : Color.black; break;
                case 16: color = containsFace(Constants.FACE.LEFT) ? Color.blue : Color.black; break;
                case 20: color = containsFace(Constants.FACE.RIGHT) ? Color.magenta : Color.black; break;
            }

            colors[i] = color;
            k++;

        }
        mesh.SetColors(colors);
    }
        
    bool containsFace(int face)
    {
        foreach(int f in _showFaces)
        {
            if(face == f)
            {
                return true;
            }
        }
        return false;
    }
}
