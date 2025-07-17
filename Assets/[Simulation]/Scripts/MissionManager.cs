using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public DroneController drone;
    public VisualTargetDetector visualDetector;
    public GameObject pizzaBox;

    void Update()
    {
        if (visualDetector.TargetDetected)
        {
            DropPizza();
        }
    }

    void DropPizza()
    {
        if (pizzaBox != null && pizzaBox.activeSelf)
        {
            Debug.Log("🍕 Pizza bırakıldı!");
            pizzaBox.SetActive(false);
        }
    }
}