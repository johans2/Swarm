﻿using System.Collections;
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
        public Vector3 direction;
        public float life;
        public float startDelay;
        public Vector3 color;
    }

    private int NumWorldNodes => worldSize.x * worldSize.y * worldSize.z;
    private Vector3 HivePosition => worldSize / 2;

    public Vector3Int worldSize = new Vector3Int(100, 100, 100);
    public Material worldMaterial;
    public Material swarmerMaterial;
    private RenderTexture worldTexture;
    public ComputeShader swarmComputShader;
    public int numSwarmers = 100000;
    public float traceAdd = 0.01f;
    public float traceDecay = 0.01f;
    public float spawnRange = 20f;
    public float traceAttraction = 0.005f;
    public float swarmerSpeed = 30;

    private int kernel;
    private ComputeBuffer worldBuffer;
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
        RenderTexture r = new RenderTexture(100, 100, 0, RenderTextureFormat.ARGB32);
        r.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        r.volumeDepth = 100;
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
        kernel = swarmComputShader.FindKernel("SwarmMain");

        // Create rotation matrices
        CreateRotationMatrices();

        // Create and init compute buffers
        worldTexture = CreateRenderTexture();
        worldBuffer = new ComputeBuffer(NumWorldNodes, 16);
        swarmBuffer = new ComputeBuffer(numSwarmers, 44);
        Swarmer[] swarmers = new Swarmer[numSwarmers];
        for (int i = 0; i < swarmers.Length; i++)
        {
            swarmers[i].position = HivePosition + new Vector3(  Random.Range(-spawnRange, spawnRange),
                                                                Random.Range(-spawnRange, spawnRange),
                                                                Random.Range(-spawnRange, spawnRange));

            swarmers[i].direction = new Vector3( Random.Range(-1.0f, 1.0f), 
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
        swarmComputShader.SetFloats("traceAdd", traceAdd);
        swarmComputShader.SetFloats("traceDecay", traceDecay);
        swarmComputShader.SetFloat("traceAttraction", traceAttraction);
        swarmComputShader.SetFloat("swarmerSpeed", swarmerSpeed);
        swarmComputShader.SetMatrix("rot1", rotMat1);
        swarmComputShader.SetMatrix("rot2", rotMat2);
        swarmComputShader.SetMatrix("rot3", rotMat3);
        swarmComputShader.SetMatrix("rot4", rotMat4);
        swarmComputShader.SetMatrix("rot5", rotMat5);
        swarmComputShader.SetMatrix("rot6", rotMat6);

        Debug.Log($"Hiveposition: {HivePosition}");


        // Set rendering materials data
        worldMaterial.SetBuffer("world", worldBuffer);
        swarmerMaterial.SetBuffer("swarmers", swarmBuffer);


        // Debug
        debugWorldNodes = new WorldNode[NumWorldNodes];
    }


    void FixedUpdate()
    {
        swarmComputShader.SetFloat("deltaTime", Time.deltaTime);
        swarmComputShader.SetFloat("elapsedTime", Time.timeSinceLevelLoad);
        swarmComputShader.Dispatch(kernel, 10, 10, 10);
        swarmComputShader.SetFloats("traceAdd", traceAdd);
        swarmComputShader.SetFloats("traceDecay", traceDecay);
        swarmComputShader.SetFloat("traceAttraction", traceAttraction);
        swarmComputShader.SetFloat("swarmerSpeed", swarmerSpeed);


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
