using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swipe
{
    static Vector2 firstPressPos;

    public static Vector3 getSwipe()
    {
        if (Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                firstPressPos = new Vector2(t.position.x, t.position.y);
            }
            if (t.phase == TouchPhase.Ended)
            {
                Vector3 secondPressPos = new Vector2(t.position.x, t.position.y);
                Vector3 currentSwipe = new Vector3(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);
                
                currentSwipe.Normalize();
                               
                return checkSwipe(currentSwipe);
            }
        }
        return Vector2.zero;
    }

    static Vector3 checkSwipe(Vector3 currentSwipe)
    {
        Vector3 rotationVector = Vector3.zero;

        if (currentSwipe != Vector3.zero)
        {
            if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
                if (Screen.width / 2 < firstPressPos.x)
                {
                    rotationVector = Vector3.forward;
                }
                else if (Screen.width / 2 > firstPressPos.x)
                {
                    rotationVector = Vector3.right;
                }
            }
            else if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
                if (Screen.width / 2 < firstPressPos.x)
                {
                    rotationVector = Vector3.back;
                }
                else if (Screen.width / 2 > firstPressPos.x)
                {
                    rotationVector = Vector3.left;
                }
            }
            else if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            {
                rotationVector = Vector3.up;               
            }
            else if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            {
                rotationVector = Vector3.down;
            }
        }
        return rotationVector;
    }
}
