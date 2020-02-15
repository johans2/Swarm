using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swarm : MonoBehaviour
{
    struct WorldNode
    {
        Vector3 position;
        float occupied;
    }

    public Vector3Int worldSize = new Vector3Int(10, 10, 10);
    public Material worldMaterial;
    public ComputeShader swarmComputShader;
    public int numSwarmers = 100000;

    private int kernel;
    private ComputeBuffer worldBuffer;
    private int NumWorldNodes => worldSize.x * worldSize.y * worldSize.z;

    private WorldNode[] debugWorldNodes;

    void Start()
    {
        kernel = swarmComputShader.FindKernel("SwarmMain");
        debugWorldNodes = new WorldNode[NumWorldNodes];

        WorldNode[] nodes = new WorldNode[NumWorldNodes];

        worldBuffer = new ComputeBuffer(NumWorldNodes, 16);
        worldBuffer.SetData(nodes);
        
        swarmComputShader.SetInt("width", worldSize.x);
        swarmComputShader.SetInt("height", worldSize.y);
        swarmComputShader.SetInt("depth", worldSize.z);
        swarmComputShader.SetBuffer(kernel, "world", worldBuffer);
        worldMaterial.SetBuffer("world", worldBuffer);

    }


    void Update()
    {
        swarmComputShader.Dispatch(kernel, 10, 10, 10);

        //worldBuffer.GetData(reNodes); Buffer filled!
        
    }

    private void OnRenderObject()
    {
        worldMaterial.SetPass(0);
        
        Graphics.DrawProceduralNow(MeshTopology.Points, 1, NumWorldNodes);
    }

    private void OnDestroy()
    {
        worldBuffer.Dispose();
    }
}
