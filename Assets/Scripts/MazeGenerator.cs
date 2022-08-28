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
            Destroy(mazeParent);
            mazeFloors.Clear();
            mazeWalls.Clear();
            clusters.Clear();
        }
        BuildNodes();
        CreateMaze();
        CreateClusters();
        //wallParent.GetComponent<MeshCombiner>().CombineMeshes();
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

        //GameObject floor = Resources.Load("MazeFloor") as GameObject;

        //GameObject floorParent = new GameObject("Floors", typeof(MeshCombiner));
        //floorParent.transform.SetParent(mazeParent.transform);

        for (int x = 0; x < MazeWidth; x++)
        {
            for (int y = 0; y < MazeHeight; y++)
            {
                //Instantiate(floor, new Vector3(x, 0, y), Quaternion.identity, floorParent.transform);

                MazeNode newFloorNode = new MazeNode();
                newFloorNode.Position = new Vector2Int(x, y);
                mazeFloors.Add(newFloorNode.Position, newFloorNode);
                unevaluatedNodes.Add(newFloorNode);
            }
        }

        //floorParent.GetComponent<MeshCombiner>().CombineMeshes();

        //StaticBatchingUtility.Combine(floorParent);

        //GameObject wall = Resources.Load("MazeWall") as GameObject;

        //wallParent = new GameObject("Walls", typeof(MeshCombiner));
        //wallParent.transform.SetParent(mazeParent.transform);

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
                            //float rotation = y != 0 ? 90 : 0;
                            //GameObject newWall = Instantiate(wall, wallPosition, Quaternion.Euler(0, rotation, 0), wallParent.transform);
                            mazeWalls.Add(wallPosition, new MazeWall(wallPosition, y != 0));
                        }
                        pair.Value.walls.Add(mazeWalls[wallPosition]);

                    }
                }
            }
        }

        //StaticBatchingUtility.Combine(wallParent);

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
                        MazeWall wall = targetNeighbour.walls[i];
                        if (current.walls.Contains(wall))
                        {
                            current.walls.Remove(wall);
                            targetNeighbour.walls.Remove(wall);
                            mazeWalls.Remove(wall.Position);
                            //Destroy(wall);
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

    private void CreateClusters()
    {
        MeshCluster cluster = new MeshCluster(MaxClusterSize * MaxClusterSize, mazeParent.transform);
        clusters.Add(cluster);

        int clusterWidth = Mathf.CeilToInt((float)MazeWidth / MaxClusterSize);
        int clusterHeight = Mathf.CeilToInt((float)MazeHeight / MaxClusterSize);

        int clusterWidthIndex = 0;
        int clusterHeightIndex = 0;


        GameObject wall = Resources.Load("MazeWall") as GameObject;

        wallParent = new GameObject("Walls");
        wallParent.transform.SetParent(mazeParent.transform);


        foreach (KeyValuePair<Vector3, MazeWall> pair in mazeWalls)
        {
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
                        MeshFilter newFloor = Instantiate(floor, new Vector3(nodePos.x, 0, nodePos.y), Quaternion.identity, floorParent.transform).GetComponent<MeshFilter>();
                        if (!cluster.CanAddMeshToCluster(newFloor))
                        {
                            Debug.Log("Couldn't add mesh to cluster!");
                            cluster = new MeshCluster(MaxClusterSize * MaxClusterSize, mazeParent.transform);
                            clusters.Add(cluster);
                        }
                        cluster.AddMeshToCluster(newFloor);
                    }
                }
            }
            clusterWidthIndex++;
            if(clusterWidthIndex >= clusterWidth)
            {
                clusterWidthIndex = 0;
                clusterHeightIndex++;
                if(clusterHeightIndex >= clusterHeight)
                {
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
