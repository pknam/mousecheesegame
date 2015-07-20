using UnityEngine;
using System.Collections;

public class CamMatrixControl : MonoBehaviour
{
    private Camera cam;

    // [Range(-1, 1)]
    public float m00;
    // [Range(-1, 1)]
    public float m01;
    // [Range(-1, 1)]
    public float m02;
    // [Range(-1, 1)]
    public float m03;

    // [Range(-1, 1)]
    public float m10;
    // [Range(-1, 1)]
    public float m11;
    // [Range(-1, 1)]
    public float m12;
    // [Range(-1, 1)]
    public float m13;

    // [Range(-1, 1)]
    public float m20;
    // [Range(-1, 1)]
    public float m21;
    // [Range(-1, 1)]
    public float m22;
    // [Range(-1, 1)]
    public float m23;

    // [Range(-1, 1)]
    public float m30;
    // [Range(-1, 1)]
    public float m31;
    // [Range(-1, 1)]
    public float m32;
    // [Range(-1, 1)]
    public float m33;

    // Use this for initialization
    void Start()
    {
        cam = this.GetComponent<Camera>();

        Matrix4x4 tmp = cam.projectionMatrix;

        m00 = tmp[0, 0];
        m01 = tmp[0, 1];
        m02 = tmp[0, 2];
        m03 = tmp[0, 3];

        m10 = tmp[1, 0];
        m11 = tmp[1, 1];
        m12 = tmp[1, 2];
        m13 = tmp[1, 3];

        m20 = tmp[2, 0];
        m21 = tmp[2, 1];
        m22 = tmp[2, 2];
        m23 = tmp[2, 3];

        m30 = tmp[3, 0];
        m31 = tmp[3, 1];
        m32 = tmp[3, 2];
        m33 = tmp[3, 3];
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(cam.projectionMatrix);

        Matrix4x4 tmp = cam.projectionMatrix;
        //tmp[0, 0] = Mathf.Sin(Time.time * 1.2f) * 1f;
        //tmp[0, 1] = Mathf.Sin(Time.time * 1.2f) * 0.5f;
        //tmp[1, 0] = Mathf.Sin(Time.time * 1.5f) * 0.5f;
        //tmp[1, 1] = Mathf.Sin(Time.time * 1.2f) * 1f;

        tmp[0, 0] = m00;
        tmp[0, 1] = m01;
        tmp[0, 2] = m02;
        tmp[0, 3] = m03;

        tmp[1, 0] = m10;
        tmp[1, 1] = m11;
        tmp[1, 2] = m12;
        tmp[1, 3] = m13;

        tmp[2, 0] = m20;
        tmp[2, 1] = m21;
        tmp[2, 2] = m22;
        tmp[2, 3] = m23;

        tmp[3, 0] = m30;
        tmp[3, 1] = m31;
        tmp[3, 2] = m32;
        tmp[3, 3] = m33;

        cam.projectionMatrix = tmp;
    }
}
