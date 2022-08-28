using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MazeGenerator : MonoBehaviour
{
    public int MazeWidth { get; private set; }
    public int MazeHeight { get; private set;}

    [Range(8, 64)]
    public int MaxClusterSize;

    [SerializeField] private InputField widthInputField;
    [SerializeField] private InputField heighInputField;

    private List<MazeNode> unevaluatedNodes;
    private List<MeshCluster> clusters = new();

    private Dictionary<Vector2Int, MazeNode> mazeFloors = new();
    private Dictionary<Vector3, MazeWall> mazeWalls = new();

    private GameObject mazeParent;
    private GameObject wallParent;

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
            //We clear the List and Dictionaries if we had already made a maze. We destroy the mazeParent and since the maze objects are children, they're also destroyed along with it.
            Destroy(mazeParent);
            mazeFloors.Clear();
            mazeWalls.Clear();
            clusters.Clear();
        }
        BuildNodes();
        CreateMaze();
        CreateClusters();
    }

    public void SetHeight(string fieldInput)
    {
        //We receive a value for the height from the InputField, clamp it between 10 and 250 and return this value to the InputField
        //(setting it correctly if it was below or above the allowed values.)
        int heightValue = Mathf.Clamp(int.Parse(fieldInput), 10, 250);
        heighInputField.text = heightValue.ToString();
        MazeHeight = heightValue;
    }

    public void SetWidth(string fieldInput)
    {        
        //We receive a value for the width from the InputField, clamp it between 10 and 250 and return this value to the InputField
        //(setting it correctly if it was below or above the allowed values.)
        int widthvalue = Mathf.Clamp(int.Parse(fieldInput), 10, 250);
        widthInputField.text = widthvalue.ToString();
        MazeWidth = widthvalue;
    }

    private void BuildNodes()
    {
        //Whether or not this was just destroyed, we're going to make a new GameObject to build the maze underneath.
        mazeParent = new GameObject("Maze");
            
        unevaluatedNodes = new();

        for (int x = 0; x < MazeWidth; x++)
        {
            for (int y = 0; y < MazeHeight; y++)
            {
                //We create new nodes over the width and height of the Maze, set their positions and add them to the Dictionary and the unevaluated nodes.
                MazeNode newFloorNode = new MazeNode();
                newFloorNode.Position = new Vector2Int(x, y);
                mazeFloors.Add(newFloorNode.Position, newFloorNode);
                unevaluatedNodes.Add(newFloorNode);
            }
        }

        //We're setting up walls on every side of a MazeNode and add the position to the wall Dictionary. We also let the nodes keep track of the walls adjacent to them.
        foreach (KeyValuePair<Vector2Int, MazeNode> pair in mazeFloors)
        {
            for (float x = -0.5f; x <= 0.5f; x += 0.5f)
            {
                for (float y = -0.5f; y <= 0.5f; y += 0.5f)
                {
                    if(Mathf.Abs(x) != Mathf.Abs(y))
                    {
                        Vector3 wallPosition = new Vector3(x + pair.Key.x, 0.5f, y + pair.Key.y);
                        if (!mazeWalls.ContainsKey(wallPosition))
                        {
                            mazeWalls.Add(wallPosition, new MazeWall(wallPosition, y != 0));
                        }
                        pair.Value.Walls.Add(mazeWalls[wallPosition]);

                    }
                }
            }
        }

    }

    private void CreateMaze()
    { 
        //We start the maze at a random node at the bottom of the screen.
        MazeNode current = mazeFloors[new Vector2Int(Random.Range(0, MazeWidth),0)];
        while(unevaluatedNodes.Count > 0)
        {
            //We mark our current MazeNode as visited and get it's neighbours.
            current.Visited = true;
            List<MazeNode> neighbours = GetNeighbours(current);
            while(neighbours.Count > 0)
            {
                //We grab a random neighbour from all that are available.
                MazeNode targetNeighbour = neighbours[Random.Range(0, neighbours.Count)];
                if (!targetNeighbour.Visited)
                {
                    //We're going to evaluate the neighbours walls, because we want to travel to it. Between every node is 1 shared wall and we're evaluating to find it.
                    //If the current wall is shared between nodes, both nodes remove it. This changes the list's size, which is why we're counting back.
                    for(int i = targetNeighbour.Walls.Count - 1; i >= 0; i--)
                    {
                        MazeWall wall = targetNeighbour.Walls[i];
                        if (current.Walls.Contains(wall))
                        {
                            current.Walls.Remove(wall);
                            targetNeighbour.Walls.Remove(wall);
                            mazeWalls.Remove(wall.Position);
                        }
                    }
                    //Now that we've found a valid neighbouring node, we have evaluated the current node and we can set the neighbour's parent to this node.
                    //After this, the neighbour becomes our current node and we can remove the other neighbours from the list and start over.
                    unevaluatedNodes.Remove(current);
                    targetNeighbour.Parent = current;
                    current = targetNeighbour;
                    neighbours.Clear();
                }
                else
                {
                    //If the neighbour was already visited, we can skip it and we remove it from the list.
                    neighbours.Remove(targetNeighbour);
                    if(neighbours.Count == 0)
                    {
                        //If no unvisited neighbours are left, we've reached a dead-end and we should go back until we have an unvisited neighbour again.
                        current = GetBacktrackedNode(current);
                    }
                }
            }
        }
    }

    private List<MazeNode> GetNeighbours(MazeNode from)
    {
        //We create a list for the result and get the neighbours, through Manhattan distance, from the "from" node. Skips possibilities outside of the maze for the border nodes too.
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
            //We're checking if the evaluatedNode has a neighbour that isn't visited. If yes, we can return this node. If no, we make sure this node isn't evaluated again.
            //After that, we check if the unevaluatedNodes list is empty, if yes, we're done. If no, we set the evaluated node to it's own parent and do this again.
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

    private void CreateClusters()
    {
        //We create a new cluster, set it's size and the parent object.
        MeshCluster cluster = new MeshCluster(MaxClusterSize * MaxClusterSize, mazeParent.transform);
        clusters.Add(cluster);

        //Calculation for the amount of necessary clusters.
        int clusterWidth = Mathf.CeilToInt((float)MazeWidth / MaxClusterSize);
        int clusterHeight = Mathf.CeilToInt((float)MazeHeight / MaxClusterSize);

        int clusterWidthIndex = 0;
        int clusterHeightIndex = 0;

        //Both the walls and floors are prefabs that are loaded at runtime, to keep the inspector clean.
        GameObject wall = Resources.Load("MazeWall") as GameObject;

        wallParent = new GameObject("Walls");
        wallParent.transform.SetParent(mazeParent.transform);


        foreach (KeyValuePair<Vector3, MazeWall> pair in mazeWalls)
        {
            //We instantiate a wall and rotate it based on the information if it has to be rotated, stored in the wall itself.
            float rotation = pair.Value.Rotated ? 90 : 0;
            Instantiate(wall, pair.Key, Quaternion.Euler(0, rotation, 0), wallParent.transform);
        }

        GameObject floor = Resources.Load("MazeFloor") as GameObject;
        GameObject floorParent = new GameObject("Floors");

        floorParent.transform.SetParent(mazeParent.transform);

        while (true)
        {
            for (int w = 0; w < MaxClusterSize; w++)
            {
                for (int h = 0; h < MaxClusterSize; h++)
                {
                    Vector2Int nodePos = new Vector2Int(w + MaxClusterSize * clusterWidthIndex, h + MaxClusterSize * clusterHeightIndex);
                    if (mazeFloors.ContainsKey(nodePos))
                    {
                        //We create a new floor here and get the meshfilter, so we can add it to the cluster for combining.
                        MeshFilter newFloor = Instantiate(floor, new Vector3(nodePos.x, 0, nodePos.y), Quaternion.identity, floorParent.transform).GetComponent<MeshFilter>();
                        if (!cluster.CanAddMeshToCluster(newFloor))
                        {
                            //If it doesn't fit in the cluster, we create a new one and add it to the list.
                            cluster = new MeshCluster(MaxClusterSize * MaxClusterSize, mazeParent.transform);
                            clusters.Add(cluster);
                        }
                        //We always add the new floor to a cluster, already existing or just created.
                        cluster.AddMeshToCluster(newFloor);
                    }
                }
            }
            //It is predetermined how many clusters are necessary and this code handles it.
            clusterWidthIndex++;
            if(clusterWidthIndex >= clusterWidth)
            {
                clusterWidthIndex = 0;
                clusterHeightIndex++;
                if(clusterHeightIndex >= clusterHeight)
                {
                    //If we're done, we loop through every cluster and let them create a singular mesh from the separate ones they contain.
                    foreach (MeshCluster c in clusters)
                    {
                        c.CreateUnifiedMesh();
                    }
                    return;
                }
            }
        }
    }



}
