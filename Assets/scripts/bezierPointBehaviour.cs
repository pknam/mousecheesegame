using UnityEngine;
using System.Collections;

public class bezierPointBehaviour : MonoBehaviour
{
    void OnMouseDrag()
    {
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));

        if (GameObject.FindGameObjectWithTag("board").GetComponent<BoxCollider2D>().bounds.Contains(new Vector3(transform.position.x, transform.position.y, 1)))
        {
            transform.parent = GameObject.Find("bezier_control").transform;
        }
        else
        {
            transform.parent = null;
        }
    }
}
