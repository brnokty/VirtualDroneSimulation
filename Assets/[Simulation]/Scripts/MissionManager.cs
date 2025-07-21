using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public DroneStabilizer drone;
    public OpenCVTargetDetector frontTarget, rightTarget, leftTarget, backTarget, upTarget, downTarget;
    public GameObject cargo;
    public Transform pickupPoint;
    public Transform dropPoint;
    public Transform returnPoint;
    public float dropAlignDistance = 1.2f; // QR'a ne kadar yaklaşınca hizalamaya başlasın?

    enum State { GoingToPickup, PickingUp, GoingToDrop, AligningToQR, Dropping, Returning, Completed }
    State state = State.GoingToPickup;

    void Update()
    {
        switch(state)
        {
            case State.GoingToPickup:
                drone.navigator.SetNewTarget(pickupPoint.gameObject);
                if (Vector3.Distance(drone.transform.position, pickupPoint.position) < 1.5f)
                    state = State.PickingUp;
                break;
            case State.PickingUp:
                cargo.transform.parent = drone.transform;
                cargo.transform.localPosition = new Vector3(0, -0.8f, 0);
                cargo.GetComponent<Rigidbody>().isKinematic = true;
                state = State.GoingToDrop;
                break;
            case State.GoingToDrop:
                drone.navigator.SetNewTarget(dropPoint.gameObject);
                // Drop noktasına yaklaşınca QR arama moduna gir
                if (Vector3.Distance(drone.transform.position, dropPoint.position) < dropAlignDistance)
                    state = State.AligningToQR;
                break;
            case State.AligningToQR:
                // 6 kameradan herhangi biri QR kodu görüyor mu?
                int qrDirection = QRDirection();
                if (qrDirection != -1)
                {
                    // Dronun yönünü QR yönüne çevir, navigator'ı dropPoint olarak bırak
                    drone.navigator.SetNewTarget(dropPoint.gameObject); // Hala drop'a git ama yönü QR yönüne döndür
                    // (Gelişmişte: drone'a bir "align to direction" komutu verebilirsin)
                    if (Vector3.Distance(drone.transform.position, dropPoint.position) < 0.4f)
                        state = State.Dropping;
                }
                break;
            case State.Dropping:
                cargo.transform.parent = null;
                cargo.GetComponent<Rigidbody>().isKinematic = false;
                state = State.Returning;
                break;
            case State.Returning:
                drone.navigator.SetNewTarget(returnPoint.gameObject);
                if (Vector3.Distance(drone.transform.position, returnPoint.position) < 1.5f)
                    state = State.Completed;
                break;
            case State.Completed:
                // Görev tamamlandı
                break;
        }
    }

    // QR'ı hangi kamera görüyor? 0:front, 1:right, 2:left, 3:back, 4:up, 5:down, -1:none
    int QRDirection()
    {
        if (frontTarget != null && frontTarget.TargetDetected) return 0;
        if (rightTarget != null && rightTarget.TargetDetected) return 1;
        if (leftTarget != null && leftTarget.TargetDetected) return 2;
        if (backTarget != null && backTarget.TargetDetected) return 3;
        if (upTarget != null && upTarget.TargetDetected) return 4;
        if (downTarget != null && downTarget.TargetDetected) return 5;
        return -1;
    }
}
