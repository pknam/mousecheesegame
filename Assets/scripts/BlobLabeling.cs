using UnityEngine;
using System.Collections;

class Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

class Visited
{
    public bool visitedFlag;
    public Point point;
}

class BlobLabeling : Object
{
    public Color[] m_image;
    public int m_nWidth;
    public int m_nHeight;

    // labeling을위한 buffer
    public int[] m_cdataBuf;

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

    public void setParam(Color[] image, int nWidth, int nHeight, int nThreshold)
    {
        if (m_recBlobs != null)
        {
            m_recBlobs = null;
            m_nBlobs = MAX_BLOBS;
        }

        m_image = image.Clone() as Color[];
        m_nWidth = nWidth;
        m_nHeight = nHeight;
        m_nThreshold = nThreshold;
    }

    public void DoLabeling()
    {
        m_nBlobs = Labeling(m_image, m_nThreshold);
    }

    private int Labeling(Color[] image, int nThreshold)
    {
        int nNumber;

        m_cdataBuf = new int[m_nWidth * m_nHeight];

        for (int j = 0; j < m_nHeight; j++)
            for (int i = 0; i < m_nWidth; i++)
                if (image[j * m_nWidth + i].Equals(new Color(1, 1, 1)))
                    m_cdataBuf[j * m_nWidth + i] = 255;

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
                m_vPoint[nY * nWidth + nX] = new Visited();
                m_vPoint[nY * nWidth + nX].visitedFlag = false;
                m_vPoint[nY * nWidth + nX].point = new Point(nX, nY);
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
        int num = 0;
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

                    if (__Area(startX, startY, endX, endY, nWidth, num) != 0)
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
        Point CurrentPoint = new Point(nPosX, nPosY);

        m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x].visitedFlag = true;
        m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x].point.x = nPosX;
        m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x].point.y = nPosY;


        while (true)
        {
            if ((CurrentPoint.x != 0) && (m_cdataBuf[CurrentPoint.y * nWidth + CurrentPoint.x - 1] == 255))   // -X 방향
            {
                if (m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x - 1].visitedFlag == false)
                {
                    m_cdataBuf[CurrentPoint.y * nWidth + CurrentPoint.x - 1] = m_cdataBuf[CurrentPoint.y * nWidth + CurrentPoint.x];	// If so, mark it
                    m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x - 1].visitedFlag = true;
                    m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x - 1].point = CurrentPoint;
                    CurrentPoint.x--;

                    if (CurrentPoint.x <= 0)
                        CurrentPoint.x = 0;

                    if (startX >= CurrentPoint.x)
                        startX = CurrentPoint.x;

                    continue;
                }
            }

            if ((CurrentPoint.x != nWidth - 1) && (m_cdataBuf[CurrentPoint.y * nWidth + CurrentPoint.x + 1] == 255))   // -X 방향
            {
                if (m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x + 1].visitedFlag == false)
                {
                    m_cdataBuf[CurrentPoint.y * nWidth + CurrentPoint.x + 1] = m_cdataBuf[CurrentPoint.y * nWidth + CurrentPoint.x];	// If so, mark it
                    m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x + 1].visitedFlag = true;
                    m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x + 1].point = CurrentPoint;
                    CurrentPoint.x++;

                    if (CurrentPoint.x >= nWidth - 1)
                        CurrentPoint.x = nWidth - 1;

                    if (endX <= CurrentPoint.x)
                        endX = CurrentPoint.x;

                    continue;
                }
            }

            if ((CurrentPoint.y != 0) && (m_cdataBuf[(CurrentPoint.y - 1) * nWidth + CurrentPoint.x] == 255))   // -X 방향
            {
                if (m_vPoint[(CurrentPoint.y - 1) * nWidth + CurrentPoint.x].visitedFlag == false)
                {
                    m_cdataBuf[(CurrentPoint.y - 1) * nWidth + CurrentPoint.x] = m_cdataBuf[CurrentPoint.y * nWidth + CurrentPoint.x];	// If so, mark it
                    m_vPoint[(CurrentPoint.y - 1) * nWidth + CurrentPoint.x].visitedFlag = true;
                    m_vPoint[(CurrentPoint.y - 1) * nWidth + CurrentPoint.x].point = CurrentPoint;
                    CurrentPoint.y--;

                    if (CurrentPoint.y <= 0)
                        CurrentPoint.y = 0;

                    if (startY >= CurrentPoint.y)
                        startY = CurrentPoint.y;

                    continue;
                }
            }

            if ((CurrentPoint.y != nHeight - 1) && (m_cdataBuf[(CurrentPoint.y + 1) * nWidth + CurrentPoint.x] == 255))   // -X 방향
            {
                if (m_vPoint[(CurrentPoint.y + 1) * nWidth + CurrentPoint.x].visitedFlag == false)
                {
                    m_cdataBuf[(CurrentPoint.y + 1) * nWidth + CurrentPoint.x] = m_cdataBuf[CurrentPoint.y * nWidth + CurrentPoint.x];	// If so, mark it
                    m_vPoint[(CurrentPoint.y + 1) * nWidth + CurrentPoint.x].visitedFlag = true;
                    m_vPoint[(CurrentPoint.y + 1) * nWidth + CurrentPoint.x].point = CurrentPoint;
                    CurrentPoint.y++;

                    if (CurrentPoint.y >= nHeight - 1)
                        CurrentPoint.y = nHeight - 1;

                    if (endY <= CurrentPoint.y)
                        endY = CurrentPoint.y;

                    continue;
                }
            }

            if ((CurrentPoint.x == m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x].point.x)
                && (CurrentPoint.y == m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x].point.y))
            {
                break;
            }
            else
            {
                CurrentPoint = m_vPoint[CurrentPoint.y * nWidth + CurrentPoint.x].point;
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