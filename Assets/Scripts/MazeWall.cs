using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeWall 
{

    public Vector3 Position { get; private set; }
    public bool Rotated { get; private set;}


    public MazeWall(Vector3 position, bool isRotated)
    {
        Position = position;

        //Does the wall need to be rotated 90 degrees when created?
        Rotated = isRotated;
    }

}
