using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

public class OpenCVTargetDetector : MonoBehaviour
{
    [Header("Camera")]
    public Camera droneCam;
    public RenderTexture renderTexture;

    [Header("Color Range")]
    public Scalar lowerHSV = new Scalar(0, 100, 100);   // Alt HSV (ör: kırmızı)
    public Scalar upperHSV = new Scalar(10, 255, 255);  // Üst HSV

    public bool TargetDetected { get; private set; }

    Texture2D tex2D;
    Mat matRGBA;
    Mat matHSV;
    Mat mask;
    Mat hierarchy;
    MatOfPoint biggestContour;

    void Start()
    {
        tex2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        matRGBA = new Mat(renderTexture.height, renderTexture.width, CvType.CV_8UC4);
        matHSV = new Mat(renderTexture.height, renderTexture.width, CvType.CV_8UC3);
        mask = new Mat(renderTexture.height, renderTexture.width, CvType.CV_8UC1);
        hierarchy = new Mat();
    }

    void Update()
    {
        // RenderTexture → Texture2D
        RenderTexture.active = renderTexture;
        tex2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex2D.Apply();
        RenderTexture.active = null;

        // Texture2D → Mat
        Utils.texture2DToMat(tex2D, matRGBA);

        // RGBA → HSV
        Imgproc.cvtColor(matRGBA, matHSV, Imgproc.COLOR_RGBA2RGB);
        Imgproc.cvtColor(matHSV, matHSV, Imgproc.COLOR_RGB2HSV);

        // HSV Threshold
        Core.inRange(matHSV, lowerHSV, upperHSV, mask);

        // Contours
        var contours = new System.Collections.Generic.List<MatOfPoint>();
        Imgproc.findContours(mask, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);

        float biggestArea = 0;
        biggestContour = null;

        foreach (var contour in contours)
        {
            double area = Imgproc.contourArea(contour);
            if (area > biggestArea)
            {
                biggestArea = (float)area;
                biggestContour = contour;
            }
        }

        TargetDetected = biggestContour != null && biggestArea > 500;

        if (TargetDetected)
        {
            Debug.Log($"🎯 OpenCV target detected! Area: {biggestArea}");
        }
    }
}
