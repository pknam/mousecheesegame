using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class drawBezierCurve : MonoBehaviour 
{
    [HideInInspector]
    public LineRenderer line;
    
    private int lineRes;

    void Start()
    {
        lineRes = 200;
        line = gameObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Particles/Additive"));
        line.SetWidth(0.3F, 0.3F);
        line.SetColors(Color.green, Color.green);
        line.SetVertexCount(lineRes);
    }

    List<Vector3> getControlPoints()
    {
        List<Vector3> templist = new List<Vector3>();

        if(transform.childCount == 0)
            return templist;

        // start point
        templist.Add(transform.FindChild("mouse").transform.position);
        templist.Add(transform.FindChild("mouse").transform.position);

        foreach (Transform child in transform)
            if(child.tag == "pivot")
                templist.Add(child.transform.position);

        // end point
        templist.Add(transform.FindChild("cheese").transform.position);
        templist.Add(transform.FindChild("cheese").transform.position);

        return templist;
    }

    public void drawCurve()
    {
        Vector3[] allPoints = new Vector3[] { };
        allPoints = getControlPoints().ToArray();
        SplineBuilder crs = new SplineBuilder(allPoints);

        bool available = true;
        GameObject[] walls = GameObject.FindGameObjectsWithTag("wall");

        for (int p = 0; p < lineRes; p++)
        {
            Vector3 temp = crs.Interp((float)p / lineRes);
            line.SetPosition(p, temp);

            temp.z = -0.01f;
            foreach (var wall in walls)
            {
                if (wall.GetComponent<BoxCollider2D>().bounds.Contains(temp))
                    available = false;
            }

            if (available)
                line.SetColors(Color.green, Color.green);
            else
                line.SetColors(Color.red, Color.red);
        }
    }

    void Update()
    {
        //drawCurve();
    }
}

public class SplineBuilder
{
    public Vector3[] pts;

    public SplineBuilder(params Vector3[] pts)
    {
        this.pts = new Vector3[pts.Length];
        System.Array.Copy(pts, this.pts, pts.Length);
    }

    // t range : [0.0f, 1.0f)
    public Vector3 Interp(float t)
    {
        int numSections = pts.Length - 3;
        int curPoint = Mathf.Min(Mathf.FloorToInt(t * (float)numSections), numSections);
        float u = t * (float)numSections - (float)curPoint;

        Vector3 a = pts[curPoint];
        Vector3 b = pts[curPoint + 1];
        Vector3 c = pts[curPoint + 2];
        Vector3 d = pts[curPoint + 3];

        // u값이 0.0f부터 1.0f까지 바뀜
        // 곡선 b에서 c까지를 1로 볼 때, u만큼 지나간 위치를 반환
        return .5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
    }
}
