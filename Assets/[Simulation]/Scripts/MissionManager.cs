using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [Header("Dependencies")]
    public DroneController drone;                  // Drone Controller referansı
    public OpenCVTargetDetector visualDetector;    // OpenCV detector referansı
    public GameObject pizzaBox;                    // Taşınan kargo kutusu

    [Header("Waypoints")]
    public Transform pickupPoint;    // Başlangıç deposu
    public Transform dropPoint;      // Balkon
    public Transform returnPoint;    // Geri dönüş

    private bool hasPickedUp = false;
    private bool hasDropped = false;

    void Update()
    {
        if (!hasPickedUp && IsNear(pickupPoint.position))
        {
            PickUp();
        }

        if (!hasDropped && IsNear(dropPoint.position))
        {
            if (visualDetector.TargetDetected)
            {
                Drop();
            }
        }

        if (hasDropped && IsNear(returnPoint.position))
        {
            Debug.Log("✅ Görev tamamlandı, drone geri döndü!");
        }
    }

    bool IsNear(Vector3 targetPos)
    {
        return Vector3.Distance(drone.transform.position, targetPos) < 2.0f;
    }

    void PickUp()
    {
        Debug.Log("📦 Kargo alındı.");
        pizzaBox.transform.parent = drone.transform;  // Kutuyu drone altına tak
        pizzaBox.transform.localPosition = new Vector3(0, -0.5f, 0);  // Altına yerleştir
        hasPickedUp = true;
    }

    void Drop()
    {
        Debug.Log("🎯 Hedef tespit edildi, kargo bırakıldı!");
        pizzaBox.transform.parent = null;   // Drone'dan ayır
        pizzaBox.GetComponent<Rigidbody>().isKinematic = false; // Physics devreye girsin
        hasDropped = true;

        // Geri dönüş waypoint ayarla
        drone.navigator.SetNewTarget(returnPoint.gameObject);
    }
}