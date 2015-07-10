using UnityEngine;
using System.Collections;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System.Threading;

public class WebCamCV : MonoBehaviour
{
    public bool viewCam;
    public bool viewThresholded;

    public GameObject ball;
    public GameObject board;

    private  BitDepth ImagesDepth = BitDepth.U8;
    private WebCamTexture webcamTexture;
    private Texture2D viewTexture;
    private Texture2D cvCamTexture;

    BlobLabeling blobLabeling;

    private int m_nWidth;
    private int m_nHeight;

    public int thread_max;
    private Thread[] thread;

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

        //webcamTexture = new WebCamTexture();
        //webcamTexture.Play();
        //this.GetComponent<Renderer>().material.mainTexture = webcamTexture;
        //m_nWidth = webcamTexture.width;
        //m_nHeight = webcamTexture.height;

        viewCam = false;
        viewThresholded = false;

        h_upper = 180;
        h_lower = 169;
        s_upper = 11;
        s_lower = 0;
        v_upper = 256;
        v_lower = 244;


        viewTexture = new Texture2D(m_nWidth, m_nHeight, TextureFormat.RGB24, false);
        GameObject.Find("viewcv").GetComponent<Renderer>().material.mainTexture = viewTexture;

        cvCamTexture = new Texture2D(m_nWidth, m_nHeight, TextureFormat.RGB24, false);
        GameObject.Find("cv_webcam").GetComponent<Renderer>().material.mainTexture = cvCamTexture;

        thread_max = 30;
        thread = new Thread[thread_max];
    }

    private void IplImageToViewTexture(IplImage img)
    {
        byte[] data = new byte[img.Width * img.Height * 3];
        Marshal.Copy(img.ImageData, data, 0, img.Width * img.Height * 3);
        this.cvCamTexture.LoadRawTextureData(data);
        this.cvCamTexture.Apply();
    }

    private void ThresholdedIplImageToViewTexture(IplImage img)
    {
        Color[] pixels = new Color[img.Width * img.Height];

        for (int i = 0; i < img.Height; i++)
        {
            for (int j = 0; j < img.Width; j++)
            {
                float val = (float) img[i, j].Val0 / 255;
                pixels[i * img.Width + j] = new Color(val, val, val);
            }
        }
        viewTexture.SetPixels(pixels);
        viewTexture.Apply();
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

    private IplImage WebcamTextureToIplImage()
    {
        IplImage img = new IplImage(m_nWidth, m_nHeight, BitDepth.U8, 3);
        Color[] pixels = webcamTexture.GetPixels();

        for (int i = 0; i < m_nHeight; i++)
        {
            for (int j = 0; j < m_nWidth; j++)
            {
                Color pixel = pixels[i * m_nWidth + j];
                CvScalar col = new CvScalar
                {
                    Val0 = (double)pixel.r * 255,
                    Val1 = (double)pixel.g * 255,
                    Val2 = (double)pixel.b * 255
                };

                img.Set2D(i, j, col);
            }
        }

        return img;
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

        Cv.Erode(imgThreshed, imgThreshed, null, 1);
        Cv.Dilate(imgThreshed, imgThreshed, null, 9);

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

        if (viewCam)
        {
            IplImageToViewTexture(img);
        }



        img = GetThresholdedImage(img);

        System.Diagnostics.Stopwatch w = System.Diagnostics.Stopwatch.StartNew();
        Color[] pixels = ThresholdedIplImageToColorMat(img);
        w.Stop();
        if (viewThresholded)
        {
            viewTexture.SetPixels(pixels);
            viewTexture.Apply();
        }



        // Labeling
        blobLabeling.setParam(pixels, m_nWidth, m_nHeight, 50);
        blobLabeling.DoLabeling();

        Vector3 viewPos = this.transform.position;

        if (blobLabeling.m_nBlobs > 0)
        {
            Vector3 ballPos = ball.transform.position;
            Vector3 boardPos = board.transform.position;
            Bounds boardBounds = board.GetComponent<Renderer>().bounds;
            Vector3 _smoothVel = Vector3.zero;

            ballPos.x = blobLabeling.m_recBlobs[0].center.x * (boardBounds.size.x * 1.5f) / m_nWidth + boardPos.x - (boardBounds.size.x * 1.5f) / 2f;
            ballPos.y = blobLabeling.m_recBlobs[0].center.y * (boardBounds.size.y * 1.5f) / m_nHeight + boardPos.y - (boardBounds.size.y * 1.5f) / 2f;

            ball.transform.position = Vector3.SmoothDamp(ball.transform.position, new Vector3(ballPos.x, ballPos.y), ref _smoothVel, 0.3f);
        }


        Debug.Log(w.ElapsedMilliseconds + " ms");
    }
}