using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using Rect = UnityEngine.Rect;

public class OpenCVObstacleDetector : MonoBehaviour
{
    public RenderTexture cameraTexture;
    public int minObstaclePixelCount = 1000; // Engelin beyaz pikselleri

    public bool ObstacleDetected { get; private set; }

    private Texture2D tex2D;
    private Mat mat;

    void Start()
    {
        tex2D = new Texture2D(cameraTexture.width, cameraTexture.height, TextureFormat.RGB24, false);
        mat = new Mat(cameraTexture.height, cameraTexture.width, CvType.CV_8UC3);
    }

    void Update()
    {
        RenderTexture.active = cameraTexture;
        tex2D.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);
        tex2D.Apply();
        RenderTexture.active = null;

        Utils.texture2DToMat(tex2D, mat);
        Mat gray = new Mat();
        Imgproc.cvtColor(mat, gray, Imgproc.COLOR_RGB2GRAY);

        Mat thresh = new Mat();
        Imgproc.threshold(gray, thresh, 50, 255, Imgproc.THRESH_BINARY);

        int whitePixels = Core.countNonZero(thresh);
        ObstacleDetected = whitePixels > minObstaclePixelCount;

        gray.Dispose();
        thresh.Dispose();
    }
}