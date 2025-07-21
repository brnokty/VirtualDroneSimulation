using UnityEngine;

public class UniversalDistanceObstacleDetector : MonoBehaviour
{
    public Direction direction; // Yönünü Inspector'dan ayarla (Up, Down, Left, Right, Forward, Backward)
    public float maxDistance = 0.25f; // Yönüne göre Inspector'dan ayarla (alt için 0.5f önerilir)
    public LayerMask obstacleLayers = ~0;
    public bool ObstacleDetected { get; private set; }
    public float lastHitDistance = 999f;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, obstacleLayers))
        {
            ObstacleDetected = true;
            lastHitDistance = hit.distance;
            
        }
        else
        {
            ObstacleDetected = false;
            lastHitDistance = 999f;
        }
        UIManager.Instance.SetDirectionFrameColor(direction, ObstacleDetected);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = ObstacleDetected ? Color.red : Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * maxDistance);
    }
}