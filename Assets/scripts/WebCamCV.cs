using UnityEngine;
using System.Collections;
using OpenCvSharp;
using System.Runtime.InteropServices;

public class WebCamCV : MonoBehaviour
{
    public bool viewCam;
    public bool viewThresholded;
    public bool erode;
    public bool dilate;

    [Range(0, 20)]
    public int n_erode;

    [Range(0, 20)]
    public int n_dilate;

    public GameObject[] balls;
    public GameObject board;

    private  BitDepth ImagesDepth = BitDepth.U8;
    private Texture2D viewTexture;
    private Texture2D cvCamTexture;

    BlobLabeling blobLabeling;

    private int m_nWidth;
    private int m_nHeight;

    CvCapture cap;


    [Range(0, 180)]
    public int h_upper;
    [Range(0, 180)]
    public int h_lower;
    [Range(0, 256)]
    public int s_upper;
    [Range(0, 256)]
    public int s_lower;
    [Range(0, 256)]
    public int v_upper;
    [Range(0, 256)]
    public int v_lower;

    void Start()
    {
        blobLabeling = new BlobLabeling();


        cap = new CvCapture(0);
        m_nWidth = cap.FrameWidth;
        m_nHeight = cap.FrameHeight;

        viewCam = false;
        viewThresholded = false;

        h_upper = 180;
        h_lower = 168;
        s_upper = 243;
        s_lower = 137;
        v_upper = 159;
        v_lower = 97;

        viewTexture = new Texture2D(m_nWidth, m_nHeight, TextureFormat.RGB24, false);
        GameObject.Find("viewcv").GetComponent<Renderer>().material.mainTexture = viewTexture;

        cvCamTexture = new Texture2D(m_nWidth, m_nHeight, TextureFormat.RGB24, false);
        this.GetComponent<Renderer>().material.mainTexture = cvCamTexture;
    }

    private void IplImageToViewTexture(IplImage img)
    {
        byte[] data = new byte[img.Width * img.Height * 3];
        Marshal.Copy(img.ImageData, data, 0, img.Width * img.Height * 3);
        this.cvCamTexture.LoadRawTextureData(data);
        this.cvCamTexture.Apply();
    }

    private Color[] ThresholdedIplImageToColorMat(IplImage img)
    {
        Color[] pixels = new Color[img.Width * img.Height];

        for (int i = 0; i < img.Height; i++)
        {
            for (int j = 0; j < img.Width; j++)
            {
                float val = (float)img[i, j].Val0 / 255;
                pixels[i * img.Width + j] = new Color(val, val, val);
            }
        }

        return pixels;
    }

    private IplImage GetThresholdedImage(IplImage img)
    {
        var imgHsv = Cv.CreateImage(Cv.GetSize(img), ImagesDepth, 3);
        var imgThreshed = Cv.CreateImage(Cv.GetSize(img), ImagesDepth, 1);

        CvScalar from = new CvScalar
        {
            Val0 = h_lower,
            Val1 = s_lower,
            Val2 = v_lower
        };

        CvScalar to = new CvScalar
        {
            Val0 = h_upper,
            Val1 = s_upper,
            Val2 = v_upper
        };

        Cv.CvtColor(img, imgHsv, ColorConversion.BgrToHsv);
        Cv.InRangeS(imgHsv, from, to, imgThreshed);
        Cv.ReleaseImage(imgHsv);

        if(erode)
            Cv.Erode(imgThreshed, imgThreshed, null, n_erode);

        if(dilate)
            Cv.Dilate(imgThreshed, imgThreshed, null, n_dilate);

        return imgThreshed;
    }

    void OnDestroy()
    {
        Cv.ReleaseCapture(cap);
    }


    void Update()
    {
        Cv.GrabFrame(cap);
        IplImage img = Cv.RetrieveFrame(cap);
        IplImage thresholdedImg = GetThresholdedImage(img);

        if (viewCam)
        {
            IplImageToViewTexture(img);
        }

        if (viewThresholded)
        {
            Color[] pixels = ThresholdedIplImageToColorMat(thresholdedImg);
            viewTexture.SetPixels(pixels);
            viewTexture.Apply();
        }

        // Labeling
        blobLabeling.setParam(thresholdedImg, 300);
        blobLabeling.DoLabeling();

        Debug.Log("blob size : " + blobLabeling.m_nBlobs);

        for (int i = 0; i < blobLabeling.m_nBlobs && i<balls.Length; i++)
        {
            var ball = balls[i];
            Vector3 ballPos = ball.transform.position;
            Vector3 boardPos = board.transform.position;
            Bounds boardBounds = board.GetComponent<Renderer>().bounds;
            Vector3 _smoothVel = Vector3.zero;

            ballPos.x = blobLabeling.m_recBlobs[i].center.x * (boardBounds.size.x * 1.5f) / m_nWidth + boardPos.x - (boardBounds.size.x * 1.5f) / 2f;
            ballPos.y = blobLabeling.m_recBlobs[i].center.y * (boardBounds.size.y * 1.5f) / m_nHeight + boardPos.y - (boardBounds.size.y * 1.5f) / 2f;

            ball.transform.position = ballPos;
            //ball.transform.position = Vector3.SmoothDamp(ball.transform.position, new Vector3(ballPos.x, ballPos.y), ref _smoothVel, 0.1f);
        }

        for (int i = blobLabeling.m_nBlobs; i < balls.Length; i++)
        {
            balls[i].transform.position = new Vector3(3.7f, -10f, -0.01f);
        }

        GameObject.Find("bezier_control").GetComponent<drawBezierCurve>().drawCurve();
    }
}