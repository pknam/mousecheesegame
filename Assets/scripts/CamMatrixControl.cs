using UnityEngine;
using System.Collections;

public class CamMatrixControl : MonoBehaviour
{
    public float x_rotation = 0;
    public float y_rotation = 0;
    public float z_rotation = 0;
    public float scale = 1;
    public Vector2 shearing = Vector2.zero;
    public Vector2 translation = Vector2.zero;


    private Matrix4x4 MatXRotate = Matrix4x4.identity;
    private Matrix4x4 MatYRotate = Matrix4x4.identity;
    private Matrix4x4 MatZRotate = Matrix4x4.identity;
    private Matrix4x4 MatScale = Matrix4x4.identity;
    private Matrix4x4 MatXShearing = Matrix4x4.identity;
    private Matrix4x4 MatYShearing = Matrix4x4.identity;
    private Matrix4x4 MatTranslation = Matrix4x4.identity;

    private Matrix4x4 OriMatProjection;
    private Camera cam;

    // Use this for initialization
    void Start()
    {
        cam = Camera.main;
        OriMatProjection = cam.projectionMatrix;
    }

    // Update is called once per frame
    void Update()
    {
        MatScale[0, 0] = scale;
        MatScale[1, 1] = scale;

        MatXRotate[1, 1] = Mathf.Cos(x_rotation * Mathf.Deg2Rad);
        MatXRotate[1, 2] = -Mathf.Sin(x_rotation * Mathf.Deg2Rad);
        MatXRotate[2, 1] = Mathf.Sin(x_rotation * Mathf.Deg2Rad);
        MatXRotate[2, 2] = Mathf.Cos(x_rotation * Mathf.Deg2Rad);

        MatYRotate[0, 0] = Mathf.Cos(y_rotation * Mathf.Deg2Rad);
        MatYRotate[0, 2] = Mathf.Sin(y_rotation * Mathf.Deg2Rad);
        MatYRotate[2, 0] = -Mathf.Sin(y_rotation * Mathf.Deg2Rad);
        MatYRotate[2, 2] = Mathf.Cos(y_rotation * Mathf.Deg2Rad);

        MatZRotate[0, 0] = Mathf.Cos(z_rotation * Mathf.Deg2Rad);
        MatZRotate[0, 1] = -Mathf.Sin(z_rotation * Mathf.Deg2Rad);
        MatZRotate[1, 0] = Mathf.Sin(z_rotation * Mathf.Deg2Rad);
        MatZRotate[1, 1] = Mathf.Cos(z_rotation * Mathf.Deg2Rad);

        MatXShearing[0, 1] = shearing.x;
        MatYShearing[1, 0] = shearing.y;

        MatTranslation[0, 2] = translation.x;
        MatTranslation[1, 2] = translation.y;

        cam.projectionMatrix = OriMatProjection *
                                MatScale *
                                MatXRotate *
                                MatYRotate *
                                MatZRotate *
                                MatXShearing *
                                MatYShearing *
                                MatTranslation;
    }
}
