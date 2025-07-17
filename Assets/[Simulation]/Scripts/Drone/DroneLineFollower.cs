using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneLineFollower : MonoBehaviour
{
    [Header("Drone Settings")]
    public Camera droneCam;
    public RenderTexture renderTexture;
    public float forwardForce = 10f;
    public float turnForce = 5f;
    public float hoverForceMultiplier = 1.5f;

    private Rigidbody rb;
    private Texture2D tex2D;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        tex2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        droneCam.targetTexture = renderTexture;
    }

    void FixedUpdate()
    {
        // Hover kuvveti
        rb.AddForce(Vector3.up * rb.mass * Physics.gravity.magnitude * hoverForceMultiplier);

        // Görüntüyü oku
        RenderTexture.active = renderTexture;
        tex2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        tex2D.Apply();
        RenderTexture.active = null;

        int width = tex2D.width;
        int height = tex2D.height;

        int scanLineY = height / 2;
        int bandHeight = 10;

        int sampleXSum = 0;
        int sampleCount = 0;

        for (int y = scanLineY - bandHeight; y <= scanLineY + bandHeight; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = tex2D.GetPixel(x, y);
                float grayscale = pixel.r * 0.299f + pixel.g * 0.587f + pixel.b * 0.114f;

                if (grayscale < 0.3f)
                {
                    sampleXSum += x;
                    sampleCount++;
                }
            }
        }

        if (sampleCount > 0)
        {
            // ✅ Çizgi bulundu → ileri git
            rb.AddForce(transform.forward * forwardForce);

            // Ortalamaya göre dön
            float avgX = (float)sampleXSum / sampleCount;
            float centerX = width / 2f;
            float threshold = 10f;

            if (avgX < centerX - threshold)
                rb.AddTorque(Vector3.up * -turnForce);
            else if (avgX > centerX + threshold)
                rb.AddTorque(Vector3.up * turnForce);
        }
        else
        {
            // ❌ Çizgi bulunmadı → ileri kuvvet yok
            // İstersen yavaşlatmak için ek frenleme de ekleyebilirsin:
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
}
