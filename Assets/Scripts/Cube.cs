using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube: MonoBehaviour
{
    int[] _showFaces;
    Rubik _parent;

    public void setData(float scale, Vector3 position, Vector3 movemment, Quaternion rotation, Transform transform, int[] showFaces, Rubik parent)
    {
        this._showFaces = showFaces;
        this._parent = parent;
        this.setColor();
        this.setScale(scale);
        this.setPosition(scale * position + movemment, rotation);
        this.gameObject.transform.parent = transform;
    }

    public void setScale(float scale)
    {
        gameObject.GetComponent<Transform>().localScale = new Vector3(scale, scale, scale);

    }

    public void setPosition(Vector3 position, Quaternion rotation)
    {
        gameObject.transform.SetPositionAndRotation(position, rotation); 
    }

    public void setColor()
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
                case 0: color = containsFace(5) ? Color.red : Color.black; break;
                case 4: color = containsFace(4) ? Color.white : Color.black; break;
                case 6: color = containsFace(2) ? Color.green : Color.black; break;
                case 8: color = containsFace(4) ? Color.white : Color.black; break;
                case 10: color = containsFace(2) ? Color.green : Color.black; break;
                case 12: color = containsFace(1) ? Color.blue : Color.black; break;
                case 16: color = containsFace(0) ? Color.yellow : Color.black; break;
                case 20: color = containsFace(3) ? Color.magenta : Color.black; break;
            }

            colors[i] = color;
            k++;

        }
        mesh.SetColors(colors);
    }

    bool containsFace(int face)
    {
     //   return true;
        foreach(int f in this._showFaces)
        {
            if(face == f)
            {
                return true;
            }
        }
        return false;
    }

    public void rotate(Vector3 direction)
    {
        gameObject.transform.RotateAround(this._parent.gameObject.transform.position, direction, -5);
    }
}
