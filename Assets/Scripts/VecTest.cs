using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VecTest : MonoBehaviour
{

    Vector3 v1 = Vector3.zero;
    Vector3 v2 = Vector3.zero;
    Vector3 v3 = Vector3.zero;
    Vector3 v4 = Vector3.zero;
    Vector3 v5 = Vector3.zero;
    Vector3 v6 = Vector3.zero;

    float sampleSpread = 60;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CreateRotationMatrices();
        //DumbVectors();
        Debug.DrawLine(transform.position, transform.position + transform.forward, Color.cyan);
        Debug.DrawLine(transform.position, transform.position + v1, Color.red);
        Debug.DrawLine(transform.position, transform.position + v2, Color.red);
        Debug.DrawLine(transform.position, transform.position + v3, Color.red);
        Debug.DrawLine(transform.position, transform.position + v4, Color.red);
        Debug.DrawLine(transform.position, transform.position + v5, Color.red);
        Debug.DrawLine(transform.position, transform.position + v6, Color.red);

    }


    private void DumbVectors() {
        v1 = transform.forward + new Vector3(1, 0, 0).normalized * 1.2f;
        v2 = transform.forward + new Vector3(0, 1, 0).normalized * 1.2f;
        v3 = transform.forward + new Vector3(0, 0, 1).normalized * 1.2f;
        v4 = transform.forward + new Vector3(-1, 0, 0).normalized * 1.2f;
        v5 = transform.forward + new Vector3(0, -1, 0).normalized * 1.2f;
        v6 = transform.forward + new Vector3(0, 0, -1).normalized * 1.2f;

    }

    private void CreateRotationMatrices()
    {
        var f4 = new Vector4(transform.forward.x, transform.forward.y, transform.forward.z, 1);

        var q1 = Quaternion.Euler(sampleSpread, 0, 0);
        var m1 = Matrix4x4.Rotate(q1);
        v1 = m1.MultiplyPoint(transform.forward);
        
        

        var q2 = Quaternion.Euler(0, sampleSpread, 0);
        var m2 = Matrix4x4.Rotate(q2);
        v2 = m2.MultiplyPoint(transform.forward);

        var q3 = Quaternion.Euler(0, 0, sampleSpread);
        var m3 = Matrix4x4.Rotate(q3);
        v3 = m3.MultiplyPoint(transform.forward);

        var q4 = Quaternion.Euler(-sampleSpread, 0, 0);
        var m4 = Matrix4x4.Rotate(q4);
        v4 = m4.MultiplyPoint(transform.forward);

        var q5 = Quaternion.Euler(0, -sampleSpread, 0);
        var m5 = Matrix4x4.Rotate(q5);
        v5 = m5.MultiplyPoint(transform.forward);

        var q6 = Quaternion.Euler(0, 0, -sampleSpread);
        var m6 = Matrix4x4.Rotate(q6);
        v6 = m6.MultiplyPoint(transform.forward);
    }
}
