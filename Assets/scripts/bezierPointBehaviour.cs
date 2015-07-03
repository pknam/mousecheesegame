using UnityEngine;
using System.Collections;

public class bezierPointBehaviour : MonoBehaviour
{

    void OnMouseDrag()
    {
        Plane boardPlane = new Plane(Vector3.forward, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        Vector3 tmp = transform.position;

        if (boardPlane.Raycast(ray, out distance))
        {
            tmp = ray.GetPoint(distance);
            tmp.z = transform.position.z;
            transform.position = tmp;
        }
    }

    void Update()
    {
        Bounds b = GameObject.FindGameObjectWithTag("board").GetComponent<SpriteRenderer>().bounds;

        if (b.Contains(new Vector3(transform.position.x, transform.position.y, 0)))
        {
            transform.parent = GameObject.FindGameObjectWithTag("bezier_control").transform;
        }
        else
        {
            transform.parent = null;
        }
    }
}
