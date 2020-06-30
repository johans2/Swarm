using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSwarm : MonoBehaviour
{
    struct WorldNode
    {
        Vector3 position;
        float occupied;
    }

    struct Swarmer {
        public Vector3 color;
        public Vector3 position;
        public Vector3 previousPosition;
        public Vector3 direction;
        public float scent;
        public float pheromoneScent;
    }

    struct SwarmTarget {
        public Vector3 position;
        public float hp;
    }

    private int NumWorldNodes => worldSize.x * worldSize.y * worldSize.z;
    private Vector3 HivePosition => worldSize / 2;

    public Vector3Int worldSize = new Vector3Int(100, 100, 100);
    public Material worldMaterial;
    public Material swarmerMaterial;
    private RenderTexture worldTexture;
    public ComputeShader swarmComputeShader;
    public Transform hiveTransform;
    public int numSwarmers = 100000;
    public float hiveRadius = 10;
    public float traceAdd = 0.01f;
    public float traceDecay = 0.01f;
    public float spawnRange = 20f;
    public float traceAttraction = 0.005f;
    public float swarmerSpeed = 30;
    public float randomness = 0.3f;

    private int swarmKernel;
    private int worldKernel;
    private ComputeBuffer swarmBuffer;

    private WorldNode[] debugWorldNodes;
    private Matrix4x4 rotMat1;
    private Matrix4x4 rotMat2;
    private Matrix4x4 rotMat3;
    private Matrix4x4 rotMat4;
    private Matrix4x4 rotMat5;
    private Matrix4x4 rotMat6;
    private float sampleSpread = 60f;

    private RenderTexture CreateRenderTexture() {
        RenderTexture r = new RenderTexture(300, 300, 0, RenderTextureFormat.ARGB32);
        r.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        r.volumeDepth = 300;
        r.enableRandomWrite = true;
        r.Create();
        return r;
    }

    private void CreateRotationMatrices()
    {
        var q1 = Quaternion.Euler(sampleSpread, 0, 0);
        rotMat1 = Matrix4x4.Rotate(q1);
        
        var q2 = Quaternion.Euler(0, sampleSpread, 0);
        rotMat2 = Matrix4x4.Rotate(q2);
        
        var q3 = Quaternion.Euler(0, 0, sampleSpread);
        rotMat3 = Matrix4x4.Rotate(q3);
        
        var q4 = Quaternion.Euler(-sampleSpread, 0, 0);
        rotMat4 = Matrix4x4.Rotate(q4);
        
        var q5 = Quaternion.Euler(0, -sampleSpread, 0);
        rotMat5 = Matrix4x4.Rotate(q5);
        
        var q6 = Quaternion.Euler(0, 0, -sampleSpread);
        rotMat6 = Matrix4x4.Rotate(q6);
    }

    void Start()
    {
        transform.position = HivePosition;
        hiveTransform.localScale = new Vector3(hiveRadius, hiveRadius, hiveRadius);

        swarmKernel = swarmComputeShader.FindKernel("TargetSwarmMain");
        worldKernel = swarmComputeShader.FindKernel("TargetWorldUpdateMain");

        // Create rotation matrices
        CreateRotationMatrices();

        // Create the worldtexture
        worldTexture = CreateRenderTexture();
        
        // Create and init swarmer compute buffers
        swarmBuffer = new ComputeBuffer(numSwarmers, 56);
        Swarmer[] swarmers = new Swarmer[numSwarmers];
        for (int i = 0; i < swarmers.Length; i++)
        {
            swarmers[i].color = new Vector3(0.20f,1f,0f);
            swarmers[i].position = HivePosition;/* + new Vector3(  Random.Range(-spawnRange, spawnRange),
                                                                Random.Range(-spawnRange, spawnRange),
                                                                Random.Range(-spawnRange, spawnRange));*/

            swarmers[i].previousPosition = swarmers[i].position;

            swarmers[i].direction = new Vector3( Random.Range(-1.0f, 1.0f), 
                                                Random.Range(-1.0f, 1.0f), 
                                                Random.Range(-1.0f, 1.0f)).normalized;

            swarmers[i].scent = 0;
            swarmers[i].pheromoneScent = 0;
        }
        swarmBuffer.SetData(swarmers);
        
        // Set swarm comput shader data
        swarmComputeShader.SetInt("width", worldSize.x);
        swarmComputeShader.SetInt("height", worldSize.y);
        swarmComputeShader.SetInt("depth", worldSize.z);
        swarmComputeShader.SetTexture(swarmKernel, "worldTex", worldTexture);
        swarmComputeShader.SetBuffer(swarmKernel, "swarmers", swarmBuffer);
        //swarmComputeShader.SetBuffer(swarmtar);
        swarmComputeShader.SetFloats("hiveX", HivePosition.x);
        swarmComputeShader.SetFloats("hiveY", HivePosition.y);
        swarmComputeShader.SetFloats("hiveZ", HivePosition.z);
        swarmComputeShader.SetFloats("traceAdd", traceAdd);
        swarmComputeShader.SetFloats("traceDecay", traceDecay);
        swarmComputeShader.SetFloat("traceAttraction", traceAttraction);
        swarmComputeShader.SetFloat("swarmerSpeed", swarmerSpeed);
        swarmComputeShader.SetMatrix("rot1", rotMat1);
        swarmComputeShader.SetMatrix("rot2", rotMat2);
        swarmComputeShader.SetMatrix("rot3", rotMat3);
        swarmComputeShader.SetMatrix("rot4", rotMat4);
        swarmComputeShader.SetMatrix("rot5", rotMat5);
        swarmComputeShader.SetMatrix("rot6", rotMat6);
        swarmComputeShader.SetFloat("randomness", randomness);
        swarmComputeShader.SetFloat("hiveRadius", hiveRadius);

        swarmComputeShader.SetTexture(worldKernel, "worldTex", worldTexture);

        Debug.Log($"Hiveposition: {HivePosition}");


        // Set rendering materials data
        swarmerMaterial.SetBuffer("swarmers", swarmBuffer);


        // Debug
        debugWorldNodes = new WorldNode[NumWorldNodes];
    }


    void Update()
    {
        hiveTransform.localScale = new Vector3(hiveRadius*2, hiveRadius*2, hiveRadius*2);
        swarmComputeShader.SetFloat("deltaTime", Time.deltaTime);
        swarmComputeShader.SetFloat("elapsedTime", Time.timeSinceLevelLoad);
        swarmComputeShader.SetFloats("traceAdd", traceAdd);
        swarmComputeShader.SetFloats("traceDecay", traceDecay);
        swarmComputeShader.SetFloat("traceAttraction", traceAttraction);
        swarmComputeShader.SetFloat("swarmerSpeed", swarmerSpeed);
        swarmComputeShader.SetFloat("randomness", randomness);
        swarmComputeShader.SetFloat("hiveRadius", hiveRadius);

        swarmComputeShader.Dispatch(worldKernel, 30, 30, 30);

        swarmComputeShader.Dispatch(swarmKernel, 1000, 1, 1);


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
        swarmBuffer.Dispose();
        
    }
}
