using UnityEngine;

public class WingController : MonoBehaviour
{
    [Header("Drone Wings")]
    public Transform[] wings; // 4 kanadı buraya sürükleyip bırak

    [Header("Rotation Speed")]
    public float rotationSpeed = 500f; // rpm değil, degrees per second

    void Update()
    {
        RotateWings();
    }

    void RotateWings()
    {
        foreach (Transform wing in wings)
        {
            if (wing != null)
            {
                wing.Rotate(new Vector3(0,0,1), rotationSpeed * Time.deltaTime);
            }
        }
    }
}