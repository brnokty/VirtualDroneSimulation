using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.ObjdetectModule;
using Rect = UnityEngine.Rect;

public class OpenCVTargetDetector : MonoBehaviour
{
    public RenderTexture cameraTexture;
    public bool TargetDetected { get; private set; }

    private Texture2D tex2D;
    private Mat mat;
    private QRCodeDetector qrDetector;

    void Start()
    {
        tex2D = new Texture2D(cameraTexture.width, cameraTexture.height, TextureFormat.RGB24, false);
        mat = new Mat(cameraTexture.height, cameraTexture.width, CvType.CV_8UC3);
        qrDetector = new QRCodeDetector();
    }

    void Update()
    {
        // RenderTexture'dan görüntüyü al
        RenderTexture.active = cameraTexture;
        tex2D.ReadPixels(new Rect(0, 0, cameraTexture.width, cameraTexture.height), 0, 0);
        tex2D.Apply();
        RenderTexture.active = null;

        Utils.texture2DToMat(tex2D, mat);

        // QR kodu tespit et ve oku
        Mat points = new Mat();
        Mat straight_qrcode = new Mat();
        string decodedText = qrDetector.detectAndDecode(mat, points, straight_qrcode);

        // SADECE "Target" ise true olsun
        TargetDetected = decodedText == "Target";

        if (TargetDetected)
            Debug.Log("🎯 QR kodu tespit edildi ve doğru içerik bulundu: Target");

        points.Dispose();
        straight_qrcode.Dispose();
    }
}