using UnityEngine;

public class VisualTargetDetector : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera droneCam;
    public RenderTexture renderTexture;

    [Header("Detection Settings")]
    [Range(0f, 1f)] public float redThreshold = 0.5f;
    [Range(0f, 1f)] public float greenMax = 0.3f;
    [Range(0f, 1f)] public float blueMax = 0.3f;
    public float detectionRatio = 0.01f; // %1 piksel yeterli

    public bool TargetDetected { get; private set; }

    private Texture2D tex2D;

    void Start()
    {
        if (renderTexture == null)
        {
            Debug.LogError("RenderTexture atanmadı!");
            return;
        }

        tex2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        droneCam.targetTexture = renderTexture;
    }

    void Update()
    {
        RenderTexture.active = renderTexture;
        tex2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex2D.Apply();
        RenderTexture.active = null;

        int w = tex2D.width;
        int h = tex2D.height;
        int detected = 0;

        for (int y = h / 3; y < h * 2 / 3; y += 2)
        {
            for (int x = 0; x < w; x += 2)
            {
                Color pixel = tex2D.GetPixel(x, y);
                if (pixel.r > redThreshold && pixel.g < greenMax && pixel.b < blueMax)
                {
                    detected++;
                }
            }
        }

        float ratio = (float)detected / ((w / 2) * (h / 3));
        TargetDetected = ratio > detectionRatio;
    }
}