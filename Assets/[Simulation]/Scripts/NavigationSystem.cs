using System;
using UnityEngine;

public class NavigationSystem : MonoBehaviour
{
    [HideInInspector] public GameObject[] waypoints;
    public float waypointTolerance = 1.0f;

    private int currentIndex = 0;

    private void Start()
    {
        waypoints = GameObject.FindGameObjectsWithTag("TargetPoint");

    }

    public Vector3 GetCurrentTarget()
    {
        if (waypoints == null || waypoints.Length == 0)
            return transform.position;

        Vector3 target = waypoints[currentIndex].transform.position;

        if (Vector3.Distance(transform.position, target) < waypointTolerance)
        {
            currentIndex = Mathf.Min(currentIndex + 1, waypoints.Length - 1);
        }

        return target;
    }
    
    // NavigationSystem.cs
    public void SetNewTarget(GameObject newWaypoint)
    {
        waypoints = new GameObject[] { newWaypoint };
        currentIndex = 0;
    }

}