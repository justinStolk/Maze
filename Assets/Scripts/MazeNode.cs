using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeNode
{
    public bool Visited { get; set; }
    public MazeNode Parent { get; set; }
    public Vector2Int Position { get; set; }

    public List<MazeWall> walls = new(); 

}
