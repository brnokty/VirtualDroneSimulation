using UnityEngine;

public class NavigationSystem : MonoBehaviour
{
    public Transform[] waypoints;
    public float waypointTolerance = 1.0f;

    private int currentIndex = 0;

    public Vector3 GetCurrentTarget()
    {
        if (waypoints == null || waypoints.Length == 0)
            return transform.position;

        Vector3 target = waypoints[currentIndex].position;

        if (Vector3.Distance(transform.position, target) < waypointTolerance)
        {
            currentIndex = Mathf.Min(currentIndex + 1, waypoints.Length - 1);
        }

        return target;
    }
}