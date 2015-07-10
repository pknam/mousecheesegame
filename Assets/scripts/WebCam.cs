using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Threading;

public class WebCam : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private Texture2D viewTexture;

    BlobLabeling m_blobLabeling;

    [Range(0, 1)]
    public float h_upper;
    [Range(0, 1)]
    public float h_lower;
    [Range(0, 1)]
    public float s_upper;
    [Range(0, 1)]
    public float s_lower;
    [Range(0, 1)]
    public float v_upper;
    [Range(0, 1)]
    public float v_lower;

    // Use this for initialization
    void Start()
    {
        h_upper = 0.06f;
        h_lower = 0.012f;
        s_upper = 0.783f;
        s_lower = 0.595f;
        v_upper = 1f;
        v_lower = 0.849f;

        m_blobLabeling = new BlobLabeling();

        // resolusion : 640 * 480
        // FPS : 30
        //webcamTexture = new WebCamTexture(WebCamTexture.devices[0].name, 640, 480, 30);
        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
        this.GetComponent<Renderer>().material.mainTexture = webcamTexture;

        viewTexture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false);
        viewTexture.filterMode = FilterMode.Trilinear;
        GameObject.Find("view").GetComponent<Renderer>().material.mainTexture = viewTexture;
    }

    // Update is called once per frame
    void Update()
    {
        //Color color = webcamTexture.GetPixel(webcamTexture.width / 2, webcamTexture.height / 2);
        //Debug.Log(color.r + ", " + color.g + ", " + color.b);

        //HSV hsv = new HSV(color);
        //Debug.Log(hsv.h + ", " + hsv.s + ", " + hsv.v);

        Color[] cam_pixels = webcamTexture.GetPixels();
        Color[] view_pixels = new Color[cam_pixels.Length];
        
        float h, s, v;
        for (int i = 0; i < cam_pixels.Length; i++)
        {
            RGBToHSV(cam_pixels[i], out h, out s, out v);

            if (h_lower <= h && h <= h_upper &&
                s_lower <= s && s <= s_upper &&
                v_lower <= v && v <= v_upper)
                view_pixels[i] = new Color(1, 1, 1);
            else
                view_pixels[i] = new Color(0, 0, 0);
        }

        viewTexture.SetPixels(view_pixels);
        viewTexture.Apply();

        //m_blobLabeling.setParam(view_pixels, webcamTexture.width, webcamTexture.height, 50);
        //m_blobLabeling.DoLabeling();

        //Debug.Log(m_blobLabeling.m_nBlobs);

        //Vector3 viewPos = this.transform.position;
        //Bounds viewBounds = this.GetComponent<Renderer>().bounds;

        //for(int i=0; i<m_blobLabeling.m_nBlobs; i++)
        //{
        //    Point pt1 = new Point((int)m_blobLabeling.m_recBlobs[i].x, (int)m_blobLabeling.m_recBlobs[i].y);
        //    Point pt2 = new Point(pt1.x + (int)m_blobLabeling.m_recBlobs[i].width, pt1.y + (int)m_blobLabeling.m_recBlobs[i].height);

        //    Debug.DrawLine(
        //        new Vector3(
        //            pt1.x * viewBounds.size.x / webcamTexture.width + viewPos.x - viewBounds.size.x / 2,
        //            pt1.y * viewBounds.size.y / webcamTexture.height + viewPos.y - viewBounds.size.y / 2,
        //            -20),
        //        new Vector3(
        //            pt2.x * viewBounds.size.x / webcamTexture.width + viewPos.x - viewBounds.size.x / 2,
        //            pt2.y * viewBounds.size.y / webcamTexture.height + viewPos.y - viewBounds.size.y / 2,
        //            -20),
        //        Color.green);
        //}
    }

    public static void RGBToHSV(Color rgbColor, out float H, out float S, out float V)
    {
        if ((double)rgbColor.b > (double)rgbColor.g && (double)rgbColor.b > (double)rgbColor.r)
            RGBToHSVHelper(4f, rgbColor.b, rgbColor.r, rgbColor.g, out H, out S, out V);
        else if ((double)rgbColor.g > (double)rgbColor.r)
            RGBToHSVHelper(2f, rgbColor.g, rgbColor.b, rgbColor.r, out H, out S, out V);
        else
            RGBToHSVHelper(0.0f, rgbColor.r, rgbColor.g, rgbColor.b, out H, out S, out V);
    }

    private static void RGBToHSVHelper(float offset, float dominantcolor, float colorone, float colortwo, out float H, out float S, out float V)
    {
        V = dominantcolor;
        if ((double)V != 0.0)
        {
            float num1 = (double)colorone <= (double)colortwo ? colorone : colortwo;
            float num2 = V - num1;
            if ((double)num2 != 0.0)
            {
                S = num2 / V;
                H = offset + (colorone - colortwo) / num2;
            }
            else
            {
                S = 0.0f;
                H = offset + (colorone - colortwo);
            }
            H = H / 6f;
            if ((double)H >= 0.0)
                return;
            H = H + 1f;
        }
        else
        {
            S = 0.0f;
            H = 0.0f;
        }
    }
}