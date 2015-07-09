using UnityEngine;
using System.Collections;
using OpenCvSharp;
using System.Runtime.InteropServices;

public class CVTest : MonoBehaviour
{
    public BitDepth ImagesDepth = BitDepth.U8;
    private WebCamTexture webcamTexture;
    private Texture2D viewTexture;

    BlobLabeling m_blobLabeling;

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
        h_upper = 129;
        h_lower = 108;
        s_upper = 207;
        s_lower = 131;
        v_upper = 256;
        v_lower = 212;

        m_blobLabeling = new BlobLabeling();

        webcamTexture = new WebCamTexture();
        webcamTexture.Play();
        this.GetComponent<Renderer>().material.mainTexture = webcamTexture;

        viewTexture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGB24, false);
        GameObject.Find("viewcv").GetComponent<Renderer>().material.mainTexture = viewTexture;
    }

    private void IplImageToViewTexture(IplImage img)
    {
        byte[] data = new byte[img.Width * img.Height * 3];
        Marshal.Copy(img.ImageData, data, 0, img.Width * img.Height * 3);
        this.viewTexture.LoadRawTextureData(data);
        this.viewTexture.Apply();
    }

    private void ThresholdedIplImageToViewTexture(IplImage img)
    {
        Color[] pixels = new Color[img.Width * img.Height];

        for (int i = 0; i < img.Height; i++)
        {
            for (int j = 0; j < img.Width; j++)
            {
                float b = (float)img[i, j].Val0;
                float g = (float)img[i, j].Val0;
                float r = (float)img[i, j].Val0;
                Color color = new Color(r / 255.0f, g / 255.0f, b / 255.0f);
                pixels[i * img.Width + j] = color;
            }
        }
        viewTexture.SetPixels(pixels);
        viewTexture.Apply();
    }

    private IplImage WebcamTextureToIplImage()
    {
        IplImage img = new IplImage(webcamTexture.width, webcamTexture.height, BitDepth.U8, 3);
        Color[] pixels = webcamTexture.GetPixels();

        for (int i = 0; i < webcamTexture.height; i++)
        {
            for(int j = 0; j < webcamTexture.width; j++)
            {
                Color pixel = pixels[i * webcamTexture.width + j];
                CvScalar col = new CvScalar
                {
                    Val0 = (double) pixel.r * 255,
					Val1 = (double) pixel.g * 255,
					Val2 = (double) pixel.b * 255
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

    void Update()
    {
        IplImage img = WebcamTextureToIplImage();
        img = GetThresholdedImage(img);
        ThresholdedIplImageToViewTexture(img);

        m_blobLabeling.setParam(viewTexture.GetPixels(), webcamTexture.width, webcamTexture.height, 120);
        m_blobLabeling.DoLabeling();

        Debug.Log(m_blobLabeling.m_nBlobs);

        Vector3 viewPos = this.transform.position;
        Bounds viewBounds = this.GetComponent<Renderer>().bounds;

        for (int i = 0; i < m_blobLabeling.m_nBlobs; i++)
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
}