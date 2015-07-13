using UnityEngine;
using System.Collections;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System;

class Visited
{
    public bool visitedFlag;
    public CvPoint returnPoint;
}

class BlobLabeling : System.Object
{
    public IplImage m_image;
    public int m_nWidth;
    public int m_nHeight;

    // labeling을위한 buffer
    public byte[] m_cdataBuf;

    public int m_nThreshold;
    public Visited[] m_vPoint;
    public int m_nBlobs;
    public Rect[] m_recBlobs;

    private const int MAX_BLOBS = 10000;
    private const int MAX_LABEL = 100;

    public BlobLabeling()
    {
        m_nThreshold = 0;
        m_nBlobs = MAX_BLOBS;
        m_image = null;
        m_recBlobs = null;
    }

    public void setParam(IplImage image, int nThreshold)
    {
        if (m_recBlobs != null)
        {
            m_recBlobs = null;
            m_nBlobs = MAX_BLOBS;
        }

        m_image = image;
        m_nWidth = image.Width;
        m_nHeight = image.Height;
        m_nThreshold = nThreshold;
        m_cdataBuf = new byte[m_nWidth * m_nHeight];
    }

    public void DoLabeling()
    {
        m_nBlobs = Labeling(m_image, m_nThreshold);
    }

    private int Labeling(IplImage image, int nThreshold)
    {
        int nNumber;

        // data copy
        IntPtr ptr = image.ImageData;
        for (int j = 0; j < m_nHeight; j++)
            for (int i = 0; i < m_nWidth; i++)
                m_cdataBuf[j * m_nWidth + i] = Marshal.ReadByte(ptr, m_nWidth * j + i);


        initvPoint(m_nWidth, m_nHeight);

        // labeling
        nNumber = _Labeling(m_nWidth, m_nHeight, nThreshold);


        if (nNumber != MAX_BLOBS)
            m_recBlobs = new Rect[nNumber];

        if (nNumber != 0)
            detectLabelingRegion(nNumber, m_nWidth, m_nHeight);

        return nNumber;
    }

    private void initvPoint(int nWidth, int nHeight)
    {
        int nX, nY;

        m_vPoint = new Visited[nWidth * nHeight];

        for (nY = 0; nY < nHeight; nY++)
        {
            for (nX = 0; nX < nWidth; nX++)
            {
                m_vPoint[nY * nWidth + nX] = new Visited()
                {
                    visitedFlag = false,
                    returnPoint = new CvPoint(nX, nY)
                };
            }
        }
    }


    private void detectLabelingRegion(int nLabelNumber, int nWidth, int nHeight)
    {
        int nX, nY;
        int nLabelIndex;

        bool[] bFirstFlag = new bool[255];

        for (int i = 0; i < bFirstFlag.Length; i++)
            bFirstFlag[i] = false;

        for (nY = 1; nY < nHeight - 1; nY++)
        {
            for (nX = 1; nX < nWidth - 1; nX++)
            {
                nLabelIndex = m_cdataBuf[nY * nWidth + nX];

                if (nLabelIndex != 0)	// Is this a new component?, 255 == Object
                {
                    if (bFirstFlag[nLabelIndex] == false)
                    {
                        m_recBlobs[nLabelIndex - 1].x = nX;
                        m_recBlobs[nLabelIndex - 1].y = nY;
                        m_recBlobs[nLabelIndex - 1].width = 0;
                        m_recBlobs[nLabelIndex - 1].height = 0;

                        bFirstFlag[nLabelIndex] = true;
                    }
                    else
                    {
                        int left = (int)m_recBlobs[nLabelIndex - 1].x;
                        int right = left + (int)m_recBlobs[nLabelIndex - 1].width;
                        int top = (int)m_recBlobs[nLabelIndex - 1].y;
                        int bottom = top + (int)m_recBlobs[nLabelIndex - 1].height;

                        if (left >= nX) left = nX;
                        if (right <= nX) right = nX;
                        if (top >= nY) top = nY;
                        if (bottom <= nY) bottom = nY;

                        m_recBlobs[nLabelIndex - 1].x = left;
                        m_recBlobs[nLabelIndex - 1].y = top;
                        m_recBlobs[nLabelIndex - 1].width = right - left;
                        m_recBlobs[nLabelIndex - 1].height = bottom - top;
                    }
                }
            }
        }
    }

    private int _Labeling(int nWidth, int nHeight, int nThreshold)
    {
        byte num = 0;
        int nX, nY;
        int startX, startY, endX, endY;

        for (nY = 0; nY < nHeight; nY++)
        {
            for (nX = 0; nX < nWidth; nX++)
            {
                if (m_cdataBuf[nY * nWidth + nX] == 255)
                {
                    num++;
                    m_cdataBuf[nY * nWidth + nX] = num;

                    startX = nX;
                    startY = nY;
                    endX = nX;
                    endY = nY;

                    __NRFindNeighbor(nWidth, nHeight, nX, nY, ref startX, ref startY, ref endX, ref endY);

                    if (__Area(startX, startY, endX, endY, nWidth, num) < nThreshold)
                    {
                        for (int k = startY; k <= endY; k++)
                        {
                            for (int l = startX; l <= endX; l++)
                            {
                                if (m_cdataBuf[k * nWidth + l] == num)
                                    m_cdataBuf[k * nWidth + l] = 0;
                            }
                        }

                        num--;

                        if (num > 250)
                            return 0;
                    }
                }
            }
        }


        return num;
    }

    private int __NRFindNeighbor(int nWidth, int nHeight, int nPosX, int nPosY, ref int startX, ref int startY, ref int endX, ref int endY)
    {
        CvPoint CurrentPoint = new CvPoint(nPosX, nPosY);

        m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].visitedFlag = true;
        m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].returnPoint.X = nPosX;
        m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].returnPoint.Y = nPosY;


        while (true)
        {
            if ((CurrentPoint.X != 0) && (m_cdataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X - 1] == 255))   // -X 방향
            {
                if (m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X - 1].visitedFlag == false)
                {
                    m_cdataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X - 1] = m_cdataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X];	// If so, mark it
                    m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X - 1].visitedFlag = true;
                    m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X - 1].returnPoint = CurrentPoint;
                    CurrentPoint.X--;

                    if (CurrentPoint.X <= 0)
                        CurrentPoint.X = 0;

                    if (startX >= CurrentPoint.X)
                        startX = CurrentPoint.X;

                    continue;
                }
            }

            if ((CurrentPoint.X != nWidth - 1) && (m_cdataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X + 1] == 255))   // +X 방향
            {
                if (m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X + 1].visitedFlag == false)
                {
                    m_cdataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X + 1] = m_cdataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X];	// If so, mark it
                    m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X + 1].visitedFlag = true;
                    m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X + 1].returnPoint = CurrentPoint;
                    CurrentPoint.X++;

                    if (CurrentPoint.X >= nWidth - 1)
                        CurrentPoint.X = nWidth - 1;

                    if (endX <= CurrentPoint.X)
                        endX = CurrentPoint.X;

                    continue;
                }
            }

            if ((CurrentPoint.Y != 0) && (m_cdataBuf[(CurrentPoint.Y - 1) * nWidth + CurrentPoint.X] == 255))   // -Y 방향
            {
                if (m_vPoint[(CurrentPoint.Y - 1) * nWidth + CurrentPoint.X].visitedFlag == false)
                {
                    m_cdataBuf[(CurrentPoint.Y - 1) * nWidth + CurrentPoint.X] = m_cdataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X];	// If so, mark it
                    m_vPoint[(CurrentPoint.Y - 1) * nWidth + CurrentPoint.X].visitedFlag = true;
                    m_vPoint[(CurrentPoint.Y - 1) * nWidth + CurrentPoint.X].returnPoint = CurrentPoint;
                    CurrentPoint.Y--;

                    if (CurrentPoint.Y <= 0)
                        CurrentPoint.Y = 0;

                    if (startY >= CurrentPoint.Y)
                        startY = CurrentPoint.Y;

                    continue;
                }
            }

            if ((CurrentPoint.Y != nHeight - 1) && (m_cdataBuf[(CurrentPoint.Y + 1) * nWidth + CurrentPoint.X] == 255))   // +Y 방향
            {
                if (m_vPoint[(CurrentPoint.Y + 1) * nWidth + CurrentPoint.X].visitedFlag == false)
                {
                    m_cdataBuf[(CurrentPoint.Y + 1) * nWidth + CurrentPoint.X] = m_cdataBuf[CurrentPoint.Y * nWidth + CurrentPoint.X];	// If so, mark it
                    m_vPoint[(CurrentPoint.Y + 1) * nWidth + CurrentPoint.X].visitedFlag = true;
                    m_vPoint[(CurrentPoint.Y + 1) * nWidth + CurrentPoint.X].returnPoint = CurrentPoint;
                    CurrentPoint.Y++;

                    if (CurrentPoint.Y >= nHeight - 1)
                        CurrentPoint.Y = nHeight - 1;

                    if (endY <= CurrentPoint.Y)
                        endY = CurrentPoint.Y;

                    continue;
                }
            }

            if ((CurrentPoint.X == m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].returnPoint.X)
                && (CurrentPoint.Y == m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].returnPoint.Y))
            {
                break;
            }
            else
            {
                CurrentPoint = m_vPoint[CurrentPoint.Y * nWidth + CurrentPoint.X].returnPoint;
            }
        }

        return 0;
    }

    private int __Area(int startX, int startY, int endX, int endY, int nWidth, int nLevel)
    {
        int nArea = 0;

        for (int nY = startY; nY < endY; nY++)
        {
            for (int nX = startX; nX < endX; nX++)
            {
                if (m_cdataBuf[nY * nWidth + nX] == nLevel)
                    nArea++;
            }
        }

        return nArea;
    }
}