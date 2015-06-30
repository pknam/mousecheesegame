using UnityEngine;
using System.Collections;

public class bezierPointBehaviour : MonoBehaviour
{
    void OnMouseDrag()
    {
        transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));

        if (GameObject.FindGameObjectWithTag("board").GetComponent<BoxCollider2D>().bounds.Contains(new Vector3(transform.position.x, transform.position.y, 1)))
            Debug.Log("in board");
        else
            Debug.Log("out board");
    }
}
