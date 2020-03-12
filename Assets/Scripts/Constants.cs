using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    public static class FACE
    {
        public const int RIGHT = 3;
        public const int LEFT = 0;
        public const int UP = 1;
        public const int DOWN = 4;
        public const int FRONT = 2;
        public const int BACK = 5;
    };

    public const int ROTATION_ANGLE = 90;
    public const int ROTATION_SPEED = 15;

    public const int MIN_SAMPLE_ITERATIONS = 200;
    public const int LIMIT_SAMPLE_ITERATIONS = 1000;

    public const int MAX_RUBIK_SIZE = 5;
    public const int MIN_RUBIK_SIZE = 2;

    public enum SHARED_PREFERENCES
    {
        RUBIK_SIZE
    }
}
