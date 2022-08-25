using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeGenerator : MonoBehaviour
{
    public int MazeWidth { get; private set; }
    public int MazeHeight { get; private set;}

    [SerializeField] private InputField widthInputField;
    [SerializeField] private InputField heighInputField;

    private List<MazeNode> unevaluatedNodes;

    private Dictionary<Vector2Int, MazeNode> mazeFloors = new();
    private Dictionary<Vector3, GameObject> mazeWalls = new();

    private GameObject mazeParent;

    // Start is called before the first frame update
    void Start()
    {
        MazeWidth = 10;
        MazeHeight = 10;
    }

    public void GenerateMaze()
    {
        if(mazeParent != null)
        {
            Destroy(mazeParent);
            mazeFloors.Clear();
            mazeWalls.Clear();
        }
        BuildNodes();
        CreateMaze();
    }

    public void SetHeight(string fieldInput)
    {
        int heightValue = Mathf.Clamp(int.Parse(fieldInput), 10, 250);
        heighInputField.text = heightValue.ToString();
        MazeHeight = heightValue;
    }

    public void SetWidth(string fieldInput)
    {
        int widthvalue = Mathf.Clamp(int.Parse(fieldInput), 10, 250);
        widthInputField.text = widthvalue.ToString();
        MazeWidth = widthvalue;
    }

    private void BuildNodes()
    {
        mazeParent = new GameObject("Maze");
            
        unevaluatedNodes = new();

        GameObject floor = Resources.Load("MazeFloor") as GameObject;

        GameObject floorParent = new GameObject("Floors");
        floorParent.transform.SetParent(mazeParent.transform);

        for (int x = 0; x < MazeWidth; x++)
        {
            for (int y = 0; y < MazeHeight; y++)
            {
                Instantiate(floor, new Vector3(x, 0, y), Quaternion.identity, floorParent.transform);

                MazeNode newFloorNode = new MazeNode();
                newFloorNode.Position = new Vector2Int(x, y);
                mazeFloors.Add(newFloorNode.Position, newFloorNode);
                unevaluatedNodes.Add(newFloorNode);
            }
        }

        GameObject wall = Resources.Load("WallParent") as GameObject;

        GameObject wallParent = new GameObject("Walls");
        wallParent.transform.SetParent(mazeParent.transform);

        foreach (KeyValuePair<Vector2Int, MazeNode> pair in mazeFloors)
        {
            for (float x = -0.5f; x <= 0.5f; x += 0.5f)
            {
                for (float y = -0.5f; y <= 0.5f; y += 0.5f)
                {
                    if(Mathf.Abs(x) != Mathf.Abs(y))
                    {
                        Vector3 wallPosition = new Vector3(x + pair.Key.x, 0, y + pair.Key.y);
                        if (!mazeWalls.ContainsKey(wallPosition))
                        {
                            float rotation = y != 0 ? 90 : 0;
                            GameObject newWall = Instantiate(wall, wallPosition, Quaternion.Euler(0, rotation, 0), wallParent.transform);
                            mazeWalls.Add(wallPosition, newWall);
                        }
                        pair.Value.walls.Add(mazeWalls[wallPosition]);

                    }
                }
            }
        }
    }

    private void CreateMaze()
    { 
        MazeNode current = mazeFloors[new Vector2Int(Random.Range(0, MazeWidth),0)];
        while(unevaluatedNodes.Count > 0)
        {
            current.Visited = true;
            List<MazeNode> neighbours = GetNeighbours(current);
            while(neighbours.Count > 0)
            {
                MazeNode targetNeighbour = neighbours[Random.Range(0, neighbours.Count)];
                if (!targetNeighbour.Visited)
                {
                    for(int i = targetNeighbour.walls.Count - 1; i >= 0; i--)
                    {
                        GameObject wall = targetNeighbour.walls[i];
                        if (current.walls.Contains(wall))
                        {
                            current.walls.Remove(wall);
                            targetNeighbour.walls.Remove(wall);
                            Destroy(wall);
                        }
                    }
                    unevaluatedNodes.Remove(current);
                    targetNeighbour.Parent = current;
                    current = targetNeighbour;
                    neighbours.Clear();
                }
                else
                {
                    neighbours.Remove(targetNeighbour);
                    if(neighbours.Count == 0)
                    {
                        current = GetBacktrackedNode(current);
                    }
                }
            }
        }
    }

    private List<MazeNode> GetNeighbours(MazeNode from)
    {
        List<MazeNode> result = new();
        for(int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int neighbourPosition = from.Position + new Vector2Int(x, y);
                if(Mathf.Abs(x) == Mathf.Abs(y) || !mazeFloors.ContainsKey(neighbourPosition))
                {
                    continue;
                }
                result.Add(mazeFloors[neighbourPosition]);
            }
        }
        return result;
    }

    private MazeNode GetBacktrackedNode(MazeNode from)
    {
        MazeNode evaluatedNode = from;
        while (true)
        {
            List<MazeNode> neighbours = GetNeighbours(evaluatedNode);
            foreach (MazeNode n in neighbours)
            {
                if (!n.Visited)
                {
                    return evaluatedNode;
                }
            }
            unevaluatedNodes.Remove(evaluatedNode);
            if(unevaluatedNodes.Count == 0)
            {
                return null;
            }
            evaluatedNode = evaluatedNode.Parent;
        }

    }
}
