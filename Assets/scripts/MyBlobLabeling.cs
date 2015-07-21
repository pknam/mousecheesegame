using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System;

public class MyBlobLabeling : System.Object
{
    public int m_nBlobs;
    public List<CvRect> m_recBlobs;

    private int m_nWidth;
    private int m_nHeight;

    // labeling을위한 buffer
    private byte[] m_cdataBuf;

    private const int MAX_BLOBS = 254;

    public MyBlobLabeling()
    {
        m_nBlobs = MAX_BLOBS;
        m_recBlobs = null;
    }

    public void Label(IplImage image)
    {
        m_recBlobs = new List<CvRect>();
        m_nBlobs = 0;
        m_nWidth = image.Width;
        m_nHeight = image.Height;
        m_cdataBuf = new byte[m_nWidth * m_nHeight];

        // copy pixel data
        Marshal.Copy(image.ImageData, m_cdataBuf, 0, m_nWidth * m_nHeight);

        m_nBlobs = _Labeling();
    }

    private int _Labeling()
    {
        int nX, nY;
        byte nLabel = 0;

        // search blobs
        for (nY = 0; nY < m_nHeight; nY++)
        {
            for (nX = 0; nX < m_nWidth; nX++)
            {
                // found new blob
                if (m_cdataBuf[nY * m_nWidth + nX] == 255)
                {
                    CvRect rt = FindNeighbor(nX, nY, nLabel);

                    // ignore under 50
                    if (rt.Width * rt.Height > 50)
                    {
                        m_recBlobs.Add(rt);
                        nLabel++;
                    }

                    if (nLabel == MAX_BLOBS)
                        return m_recBlobs.Count;
                }
            }
        }

        return m_recBlobs.Count;
    }

    // Blob Labeling using BFS
    private CvRect FindNeighbor(int nX, int nY, byte nLabel)
    {
        Queue<CvPoint> q = new Queue<CvPoint>();
        int left = nX;
        int right = nX;
        int top = nY;
        int bottom = nY;

        q.Enqueue(new CvPoint(nX, nY));

        while(q.Count > 0)
        {
            CvPoint ptTmp = q.Dequeue();

            // if already visited
            if (m_cdataBuf[ptTmp.Y * m_nWidth + ptTmp.X] == nLabel)
                continue;

            // check visited
            m_cdataBuf[ptTmp.Y * m_nWidth + ptTmp.X] = nLabel;

            if (ptTmp.X > 0 && m_cdataBuf[ptTmp.Y * m_nWidth + (ptTmp.X - 1)] == 255)
                q.Enqueue(new CvPoint(ptTmp.X - 1, ptTmp.Y));
            if (ptTmp.Y > 0 && m_cdataBuf[(ptTmp.Y - 1) * m_nWidth + ptTmp.X] == 255)
                q.Enqueue(new CvPoint(ptTmp.X, ptTmp.Y - 1));
            if (ptTmp.X < m_nWidth - 1 && m_cdataBuf[ptTmp.Y * m_nWidth + (ptTmp.X + 1)] == 255)
                q.Enqueue(new CvPoint(ptTmp.X + 1, ptTmp.Y));
            if (ptTmp.Y < m_nHeight - 1 && m_cdataBuf[(ptTmp.Y + 1) * m_nWidth + ptTmp.X] == 255)
                q.Enqueue(new CvPoint(ptTmp.X, ptTmp.Y + 1));

            if (ptTmp.X < left)
                left = ptTmp.X;
            if (ptTmp.X > right)
                right = ptTmp.X;
            if (ptTmp.Y < top)
                top = ptTmp.Y;
            if (ptTmp.Y > bottom)
                bottom = ptTmp.Y;
        }

        return new CvRect(left, top, right - left, bottom - top);
    }
}