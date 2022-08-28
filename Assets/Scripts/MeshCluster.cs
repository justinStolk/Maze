using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCluster
{
    private List<MeshFilter> meshFilters = new();

    private int size;
    private int clusterVertices;

    private GameObject clusterContainer;
    private Transform clusterParent;

    public MeshCluster(int maxClusterObjects, Transform parent)
    {
        clusterParent = parent;
        if(maxClusterObjects > ushort.MaxValue)
        {
            throw new System.Exception("Determined max cluster objects is too big to create a mesh from!");
        }
        size = maxClusterObjects;
    }

    public bool CanAddMeshToCluster(MeshFilter meshToEvaluate)
    {
        if (clusterVertices + meshToEvaluate.mesh.vertexCount <= ushort.MaxValue && meshFilters.Count < size)
        {
            return true;
        }
        return false;
    }

    public void AddMeshToCluster(MeshFilter meshToAdd)
    {
        if(clusterVertices + meshToAdd.mesh.vertexCount <= ushort.MaxValue && meshFilters.Count < size)
        {
            if (!meshFilters.Contains(meshToAdd))
            {
                clusterVertices += meshToAdd.mesh.vertexCount;
                meshFilters.Add(meshToAdd);
                return;
            }
            throw new System.Exception("List already contains this particular mesh, this should not happen!");
        }
        throw new System.Exception("Cluster is already full! Consider making a new cluster!");
    }

    public void CreateUnifiedMesh()
    {
        clusterContainer = new GameObject("Mesh Cluster", typeof(MeshFilter), typeof(MeshRenderer));
        clusterContainer.transform.SetParent(clusterParent);

        CombineInstance[] combine = new CombineInstance[meshFilters.Count];
        Debug.Log(combine.Length);
        Debug.Log(meshFilters.Count);

        int i = 0;

        MeshRenderer rend = meshFilters[0].GetComponent<MeshRenderer>();
        Material sharedMaterial = rend.material;
        sharedMaterial.color = rend.material.color;

        while(i < meshFilters.Count)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }

        clusterContainer.GetComponent<MeshFilter>().mesh = new Mesh();
        clusterContainer.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        clusterContainer.GetComponent<MeshRenderer>().sharedMaterial = sharedMaterial;
        clusterContainer.gameObject.SetActive(true);
    }


}
