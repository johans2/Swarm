using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swarm : MonoBehaviour
{
    public ComputeShader swarmComputShader;
    public int numSwarmers = 100000;

    private int kernel;

    void Start()
    {
        kernel = swarmComputShader.FindKernel("SwarmMain");


    }


    void Update()
    {
        swarmComputShader.Dispatch(1, 1, 1, 1);


    }
}
