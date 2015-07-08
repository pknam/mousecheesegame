using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Threading;

public class WebCam : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    Texture2D viewTexture;

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
        h_upper = 0.995f;
        h_lower = 0.938f;
        s_upper = 0.88f;
        s_lower = 0.673f;
        v_upper = 0.706f;
        v_lower = 0.544f;

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

        m_blobLabeling.setParam(view_pixels, webcamTexture.width, webcamTexture.height, 50);
        m_blobLabeling.DoLabeling();

        Debug.Log(m_blobLabeling.m_nBlobs);

        Vector3 viewPos = this.transform.position;
        Bounds viewBounds = this.GetComponent<Renderer>().bounds;

        for(int i=0; i<m_blobLabeling.m_nBlobs; i++)
        {
            Point pt1 = new Point((int)m_blobLabeling.m_recBlobs[i].x, (int)m_blobLabeling.m_recBlobs[i].y);
            Point pt2 = new Point(pt1.x + (int)m_blobLabeling.m_recBlobs[i].width, pt1.y + (int)m_blobLabeling.m_recBlobs[i].height);

            Debug.DrawLine(
                new Vector3(
                    pt1.x * viewBounds.size.x / webcamTexture.width + viewPos.x - viewBounds.size.x / 2,
                    pt1.y * viewBounds.size.y / webcamTexture.height + viewPos.y - viewBounds.size.y / 2,
                    -20),
                new Vector3(
                    pt2.x * viewBounds.size.x / webcamTexture.width + viewPos.x - viewBounds.size.x / 2,
                    pt2.y * viewBounds.size.y / webcamTexture.height + viewPos.y - viewBounds.size.y / 2,
                    -20),
                Color.green);
        }
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

//class HSV
//{
//    public float h;
//    public float s;
//    public float v;

//    public HSV()
//    {
//        this.h = 0;
//        this.s = 0;
//        this.v = 0;
//    }

//    public HSV(float h, float s, float v)
//    {
//        this.h = h;
//        this.s = s;
//        this.v = v;
//    }

//    public HSV(Color color)
//    {
//        fromColor(color);
//    }

//    public void fromColor(Color color)
//    {
//        float min, max, delta;

//        min = Mathf.Min(color.r, color.g, color.b);
//        max = Mathf.Max(color.r, color.g, color.b);
//        this.v = max;				// v
//        delta = max - min;
//        if (max != 0)
//            this.s = delta / max;		// s
//        else
//        {
//            // r = g = b = 0		// s = 0, v is undefined
//            this.s = 0;
//            this.h = -1;
//            return;
//        }
//        if (color.r == max)
//            this.h = (color.g - color.b) / delta;		// between yellow & magenta
//        else if (color.g == max)
//            this.h = 2 + (color.b - color.r) / delta;	// between cyan & yellow
//        else
//            this.h = 4 + (color.r - color.g) / delta;	// between magenta & cyan
//        this.h *= 60;				// degrees
//        if (this.h < 0)
//            this.h += 360;
//    }
//}