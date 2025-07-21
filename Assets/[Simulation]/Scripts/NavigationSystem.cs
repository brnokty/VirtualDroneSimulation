using UnityEngine;

public class NavigationSystem : MonoBehaviour
{
    private GameObject currentTarget;
    public float waypointTolerance = 1.5f;

    public Vector3 GetCurrentTarget()
    {
        if (currentTarget == null)
            return transform.position;

        return currentTarget.transform.position;
    }

    public void SetTargetIfNotSet(GameObject newTarget)
    {
        if (currentTarget == null)
            currentTarget = newTarget;
    }

    public void SetNewTarget(GameObject newTarget)
    {
        currentTarget = newTarget;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }
}