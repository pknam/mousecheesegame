using UnityEngine;
using System.Collections;
using OpenCvSharp;
using System.Runtime.InteropServices;

public class CamCalib : MonoBehaviour
{
    CvMat _image_points;
    CvMat _object_points;
    CvMat _point_counts;

    CvMat _intrinsic_matrix;
    CvMat _distortion_coeffs;

    IplImage _mapx;
    IplImage _mapy;

    float _cell_w;	// 체스판에서 한 격자의 가로방향 넓이
    float _cell_h;	// 체스판에서 한 격자의 세로방향 넓이

    int _n_boards;	// 인식할 체스판 수를 지정한다.
    int _board_w;	// 체스판의 가로방향 코너 수
    int _board_h;	// 체스판의 세로방향 코너 수
    int _board_n;	// 가로 x 세로 방향의 코너 수
    int _board_total;
    int _successes;

    public CamCalib(int board_w = 9, int board_h = 6, int n_boards = 2, float cell_w = 0.035f, float cell_h = 0.035f)
    {
        _board_n = _board_w * _board_h;

        // 체스판으로부터 찾은 코너를 저장할 저장공간 할당
        _image_points = Cv.CreateMat(_n_boards * _board_n, 2, MatrixType.F32C1);
        _object_points = Cv.CreateMat(_n_boards * _board_n, 3, MatrixType.F32C1);
        _point_counts = Cv.CreateMat(_n_boards, 1, MatrixType.F32C1);

        //Intrinsic Matrix - 3x3			   Lens Distorstion Matrix - 4x1
        //	[fx 0 cx]							[k1 k2 p1 p2   k3(optional)]
        //	[0 fy cy]
        //	[0  0  1]

        _intrinsic_matrix = null;
        _distortion_coeffs = null;

        _mapx = null;
        _mapy = null;

        _successes = 0;
    }

    public void LoadCalibParams(CvSize image_size)
    {
        	// 파일로 저장된 내부행렬과 왜곡 계수를 불러오기
#if true //140911 : 카메라 내외부 인수 문제 해결하기.
 	_intrinsic_matrix  = Cv.Load<CvMat>("./para/Intrinsics.xml");
 	_distortion_coeffs = Cv.Load<CvMat>("./para/Distortion.xml");
#else
	Mat Mc = Mat_<double>(3,3);
	Mat dist = Mat_<double>(4,1);

	//0. Load camera parameter
	FileStorage	cvfs("./para/camera.xml", CV_STORAGE_READ);
	FileNode node(cvfs.fs, NULL);
	FileNode fn = node[string("mat_array")];
	read(fn[0], Mc);
	read(fn[1], dist);
	printf("#Load IR camera parameter!!!\n");

	_intrinsic_matrix = &(CvMat)Mc;
	_distortion_coeffs = &(CvMat)dist;
#endif


	if (_intrinsic_matrix != null && _distortion_coeffs != null) {
		// 왜곡 제거를 위한 지도를 생성
		_mapx = Cv.CreateImage( image_size, BitDepth.F32, 1 );
        _mapy = Cv.CreateImage(image_size, BitDepth.F32, 1);

		// 왜곡 제거를 위한 지도를 구성
	    Cv.InitUndistortMap (_intrinsic_matrix, _distortion_coeffs, _mapx, _mapy);

		_successes = _n_boards + 1;
	}
    }

    public bool FindChessboard(IplImage src, IplImage dst)
    {
	    IplImage gray = Cv.CreateImage (Cv.GetSize(src), BitDepth.U8, 1);
	
	    Cv.CvtColor(src, gray, ColorConversion.BgrToGray);

	    // 체스판 코너 찾기
	    CvPoint2D32f[] corners = new CvPoint2D32f[_board_n];
	    int corner_count = 0;
	    bool found = Cv.FindChessboardCorners(src, Cv.Size(_board_w, _board_h), out corners, out corner_count, ChessboardFlag.AdaptiveThresh | ChessboardFlag.FilterQuads);

	    // 검출된 코너로부터 서브픽셀 정확도로 코너 좌표를 구한다.
	    Cv.FindCornerSubPix (gray, corners, corner_count, Cv.Size(11,11), Cv.Size(-1,-1), Cv.TermCriteria(CriteriaType.Epsilon | CriteriaType.Iteration, 30, 0.1 ));

	    // 코너를 dst 이미지에 그린다.
	    Cv.DrawChessboardCorners (dst, Cv.Size(_board_w, _board_h), corners, found);

	    // 코너를 정상적으로 찾았다면, 코너 데이터를 저장한다.
	    bool ret = false;
	    if (found && corner_count == _board_n) {
		    for( int i=_successes*_board_n, j=0; j<_board_n; ++i, ++j ) {
                _image_points[i, 0] = corners[j].X;
                _image_points[i, 1] = corners[j].Y;
                _object_points[i, 0] = (j%_board_w)*_cell_w;
			    _object_points[i, 1] = (float)(_board_h - j/_board_w - 1)*_cell_h;
			    _object_points[i, 2] = 0.0f;
		    }
		    _point_counts[_successes, 0] = _board_n; 
		
		    ret = true;
	    }

	    Cv.ReleaseImage(gray);  
	    return ret;
    }

    public void Undistort(IplImage src, IplImage dst)
    {
        Debug.Assert(_mapx != null);
        Debug.Assert(_mapy != null);

        // 카메라 입력영상(src)에서 왜곡을 제거한 영상(dst)을 만든다.
        Cv.Remap(src, dst, _mapx, _mapy);			// Undistort image
    }

    public void CalibrateCamera(CvSize image_size)
    {
	    if (_intrinsic_matrix != null)  Cv.ReleaseMat(_intrinsic_matrix);
	    if (_distortion_coeffs != null) Cv.ReleaseMat(_distortion_coeffs);

	    if (_mapx != null) Cv.ReleaseImage(_mapx);
	    if (_mapy != null) Cv.ReleaseImage(_mapy);

	    _intrinsic_matrix  = Cv.CreateMat(3, 3, MatrixType.F32C1);
	    _distortion_coeffs = Cv.CreateMat(4, 1, MatrixType.F32C1);

	    // 초점 거리 비율을 1.0으로 설정하여 내부행렬을 초기화
	    _intrinsic_matrix[0, 0] = 1.0f;
	    _intrinsic_matrix[1, 1] = 1.0f;

	    // 실제 카메라 보정함수
	    Cv.CalibrateCamera2 (_object_points, _image_points, _point_counts, image_size, _intrinsic_matrix, _distortion_coeffs, null, null, 0);

        System.IntPtr tmp1 = System.IntPtr.Zero;
        System.IntPtr tmp2 = System.IntPtr.Zero;

	    // 내부 행렬과 왜곡 계수를 파일로 저장
        Marshal.StructureToPtr(_intrinsic_matrix, tmp1, false);
	    Cv.Save("Intrinsics.xml", tmp1);

        Marshal.StructureToPtr(_distortion_coeffs, tmp2, false);
	    Cv.Save("Distortion.xml", tmp2);

	    // 왜곡 제거를 위한 지도를 생성
	    _mapx = Cv.CreateImage( image_size, BitDepth.F32, 1 );
        _mapy = Cv.CreateImage(image_size, BitDepth.F32, 1);

	    // 왜곡 제거를 위한 지도를 구성
	    Cv.InitUndistortMap (_intrinsic_matrix, _distortion_coeffs, _mapx, _mapy);
    }
}
