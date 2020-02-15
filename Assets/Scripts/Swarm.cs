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

    struct Swarmer {
        public Vector3 position;
        public Vector3 velocity;
        public float life;
    }

    private int NumWorldNodes => worldSize.x * worldSize.y * worldSize.z;

    public Vector3Int worldSize = new Vector3Int(10, 10, 10);
    public Material worldMaterial;
    public Material swarmerMaterial;
    public ComputeShader swarmComputShader;
    public int numSwarmers = 100000;
    public float curlE = 0.1f;
    public float dir = 1f;

    private int kernel;
    private ComputeBuffer worldBuffer;
    private ComputeBuffer swarmBuffer;

    private WorldNode[] debugWorldNodes;

    void Start()
    {
        kernel = swarmComputShader.FindKernel("SwarmMain");
        
        // Create and init compute buffers
        worldBuffer = new ComputeBuffer(NumWorldNodes, 16);
        swarmBuffer = new ComputeBuffer(numSwarmers, 28);
        Swarmer[] swarmers = new Swarmer[numSwarmers];
        for (int i = 0; i < swarmers.Length; i++)
        {
            swarmers[i].position = Vector3.zero + new Vector3(  Random.Range(-0.1f, 0.1f),
                                                                Random.Range(-0.1f, 0.1f),
                                                                Random.Range(-0.1f, 0.1f));

            swarmers[i].velocity = new Vector3( Random.Range(-1.0f, 1.0f), 
                                                Random.Range(-1.0f, 1.0f), 
                                                Random.Range(-1.0f, 1.0f)).normalized;
            swarmers[i].life = Random.Range(-10f,20.0f);
        }
        swarmBuffer.SetData(swarmers);
        
        // Set comput shader data
        swarmComputShader.SetInt("width", worldSize.x);
        swarmComputShader.SetInt("height", worldSize.y);
        swarmComputShader.SetInt("depth", worldSize.z);
        swarmComputShader.SetBuffer(kernel, "world", worldBuffer);
        swarmComputShader.SetBuffer(kernel, "swarmers", swarmBuffer);
        //swarmComputShader.SetTexture(kernel, "noiseTex", noiseTex)

        // Set rendering materials data
        worldMaterial.SetBuffer("world", worldBuffer);
        swarmerMaterial.SetBuffer("swarmers", swarmBuffer);


        // Debug
        debugWorldNodes = new WorldNode[NumWorldNodes];
    }


    void Update()
    {
        swarmComputShader.SetFloat("deltaTime", Time.deltaTime);
        swarmComputShader.SetFloat("curlE", curlE);
        swarmComputShader.SetFloat("dir", dir);
        swarmComputShader.Dispatch(kernel, 10, 10, 10);

        //worldBuffer.GetData(reNodes); Buffer filled!
    }

    private void OnRenderObject()
    {
        //worldMaterial.SetPass(0);
        
        //Graphics.DrawProceduralNow(MeshTopology.Points, 1, NumWorldNodes);
        
        swarmerMaterial.SetPass(0);

        Graphics.DrawProceduralNow(MeshTopology.Points, 1, numSwarmers);
    }

    private void OnDestroy()
    {
        worldBuffer.Dispose();
        swarmBuffer.Dispose();
    }
}
