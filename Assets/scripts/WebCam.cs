using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Threading;

public class WebCam : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    Texture2D viewTexture;

    // Use this for initialization
    void Start()
    {
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
            EditorGUIUtility.RGBToHSV(cam_pixels[i], out h, out s, out v);

            if (h < 0.6f && h > 0.5f)
                view_pixels[i] = new Color(1, 1, 1);
            else
                view_pixels[i] = new Color(0, 0, 0);
        }

        viewTexture.SetPixels(view_pixels);
        viewTexture.Apply();
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