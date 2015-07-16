using UnityEngine;
using System.Collections;

public class CamMatrixControl : MonoBehaviour
{
    private Camera cam;

    // Use this for initialization
    void Start()
    {
        cam = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(cam.projectionMatrix);

        Matrix4x4 tmp = cam.projectionMatrix;
        //tmp.m00 = Mathf.Sin(Time.time * 1.2f) * 1f;
        tmp.m01 = Mathf.Sin(Time.time * 1.2f) * 0.5f;
        tmp.m10 = Mathf.Sin(Time.time * 1.5f) * 0.5f;
        //tmp.m11 = Mathf.Sin(Time.time * 1.2f) * 1f;
        cam.projectionMatrix = tmp;
    }
}
