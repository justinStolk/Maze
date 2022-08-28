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
        //The clusterParent is the object under which the cluster will be created, a way to keep the hierarchy clearer. This can also be null. 
        clusterParent = parent;
        if(maxClusterObjects > ushort.MaxValue)
        {
            throw new System.Exception("Determined max cluster objects is too big to create a mesh from!");
        }
        size = maxClusterObjects;
    }

    public bool CanAddMeshToCluster(MeshFilter meshToEvaluate)
    {
        //This particular mesh can only be added if the total vertex count of the ones we already have + the new one doesn't exceed the maximum vertices you can create in a standard mesh
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
                //We can only add meshes to a cluster if it's not already within the cluster. Then, we'll make sure to track the amount of vertices we have now by addition to our int.
                clusterVertices += meshToAdd.mesh.vertexCount;
                meshFilters.Add(meshToAdd);
                return;
            }
            throw new System.Exception("List already contains this particular mesh, this should not happen!");
        }
        //This exception is thrown when the cluster is full. However, since you can evaluate this by the function above this one, you shouldn't get this. It's a safety net.
        throw new System.Exception("Cluster is already full! Consider making a new cluster!");
    }

    public void CreateUnifiedMesh()
    {
        //We create a new GameObject with MeshFilter and MeshRenderer components. We then parent it under the clusterParent (if applicable)
        clusterContainer = new GameObject("Mesh Cluster", typeof(MeshFilter), typeof(MeshRenderer));
        clusterContainer.transform.SetParent(clusterParent);

        CombineInstance[] combine = new CombineInstance[meshFilters.Count];

        int i = 0;

        //We want to transfer the material over from the existing meshes, so we assume it's the same for all of them and we just get it from the first one.
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
