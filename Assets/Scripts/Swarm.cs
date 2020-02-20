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
        public float startDelay;
        public Vector3 color;
    }

    private int NumWorldNodes => worldSize.x * worldSize.y * worldSize.z;
    private Vector3 HivePosition => worldSize / 2;

    public Vector3Int worldSize = new Vector3Int(100, 100, 100);
    public Material worldMaterial;
    public Material swarmerMaterial;
    public RenderTexture worldTexture;
    public ComputeShader swarmComputShader;
    public int numSwarmers = 100000;

    private int kernel;
    private ComputeBuffer worldBuffer;
    private ComputeBuffer swarmBuffer;

    private WorldNode[] debugWorldNodes;

    private RenderTexture CreateRenderTexture() {
        RenderTexture r = new RenderTexture(100, 100, 100, RenderTextureFormat.ARGB32);
        r.enableRandomWrite = true;
        return r;
    }

    void Start()
    {
        kernel = swarmComputShader.FindKernel("SwarmMain");

        // Create and init compute buffers
        worldTexture.enableRandomWrite = true;
        worldBuffer = new ComputeBuffer(NumWorldNodes, 16);
        swarmBuffer = new ComputeBuffer(numSwarmers, 44);
        Swarmer[] swarmers = new Swarmer[numSwarmers];
        for (int i = 0; i < swarmers.Length; i++)
        {
            swarmers[i].position = HivePosition + new Vector3(  Random.Range(-0.1f, 0.1f),
                                                                Random.Range(-0.1f, 0.1f),
                                                                Random.Range(-0.1f, 0.1f));

            swarmers[i].velocity = new Vector3( Random.Range(-1.0f, 1.0f), 
                                                Random.Range(-1.0f, 1.0f), 
                                                Random.Range(-1.0f, 1.0f)).normalized;
            swarmers[i].life = Random.Range(0f,3.0f);

            swarmers[i].startDelay = 0;// Random.Range(0, 15f);
        }
        swarmBuffer.SetData(swarmers);
        
        // Set comput shader data
        swarmComputShader.SetInt("width", worldSize.x);
        swarmComputShader.SetInt("height", worldSize.y);
        swarmComputShader.SetInt("depth", worldSize.z);
        swarmComputShader.SetBuffer(kernel, "world", worldBuffer);
        swarmComputShader.SetTexture(kernel, "worldTex", worldTexture);
        swarmComputShader.SetBuffer(kernel, "swarmers", swarmBuffer);
        swarmComputShader.SetFloats("hiveX", HivePosition.x);
        swarmComputShader.SetFloats("hiveY", HivePosition.y);
        swarmComputShader.SetFloats("hiveZ", HivePosition.z);

        Debug.Log($"Hiveposition: {HivePosition}");


        // Set rendering materials data
        worldMaterial.SetBuffer("world", worldBuffer);
        swarmerMaterial.SetBuffer("swarmers", swarmBuffer);


        // Debug
        debugWorldNodes = new WorldNode[NumWorldNodes];
    }


    void Update()
    {
        swarmComputShader.SetFloat("deltaTime", Time.deltaTime);
        swarmComputShader.SetFloat("elapsedTime", Time.timeSinceLevelLoad);
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
