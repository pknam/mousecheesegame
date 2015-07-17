using UnityEngine;
using System.Collections;

public class CamMatrixControl : MonoBehaviour
{
    private Camera cam;

    [Range(0, 1)]
    public float m00;
    [Range(0, 1)]
    public float m01;
    [Range(0, 1)]
    public float m10;
    [Range(0, 1)]
    public float m11;

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
        //tmp[0, 0] = Mathf.Sin(Time.time * 1.2f) * 1f;
        //tmp[0, 1] = Mathf.Sin(Time.time * 1.2f) * 0.5f;
        //tmp[1, 0] = Mathf.Sin(Time.time * 1.5f) * 0.5f;
        //tmp[1, 1] = Mathf.Sin(Time.time * 1.2f) * 1f;

        tmp[0, 0] = m00;
        tmp[0, 1] = m01;
        tmp[1, 0] = m10;
        tmp[1, 1] = m11;

        cam.projectionMatrix = tmp;
    }
}
