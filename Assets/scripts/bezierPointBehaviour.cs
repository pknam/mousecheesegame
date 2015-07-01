using UnityEngine;
using System.Collections;

public class bezierPointBehaviour : MonoBehaviour
{
    void OnMouseDrag()
    {
        Vector3 tmp = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
        tmp.z = transform.position.z;

        transform.position = tmp;
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
