using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneStabilizer : MonoBehaviour
{
    [Header("Stabilizasyon ve Uçuş")]
    public float baseHoverForce = 9.81f;
    public float altitudeKp = 8f;
    public float maxVerticalSpeed = 5f;
    public float forwardKp = 10f;
    public float maxTotalSpeed = 5f;
    public float yawKp = 7f;
    public float maxYawRate = 60f;
    public float rollKp = 5f;
    public float pitchKp = 5f;

    [Header("Engel Sensörleri (Raycast)")]
    public UniversalDistanceObstacleDetector frontObstacle, rightObstacle, leftObstacle, backObstacle, upObstacle, downObstacle;

    [Header("Target Dedektörleri (OpenCV)")]
    public OpenCVTargetDetector frontTarget, rightTarget, leftTarget, backTarget, upTarget, downTarget;

    [Header("Navigasyon")]
    public NavigationSystem navigator;

    [Header("Kaçınma")]
    public float avoidStrength = 7f;

    // Bu değer MissionManager tarafından set edilecek:
    [HideInInspector] public int forcedYawDirection = -1; // -1: yok, 0:front, 1:right, 2:left, 3:back, 4:up, 5:down

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.angularDrag = 6f;
        rb.drag = 1.5f;
    }

    void FixedUpdate()
    {
        if (navigator == null) return;
        Vector3 target = navigator.GetCurrentTarget();

        // 1. Yükseklik kontrolü
        float altitudeError = Mathf.Clamp(target.y - transform.position.y, -maxVerticalSpeed, maxVerticalSpeed);
        float verticalForce = baseHoverForce * rb.mass + (altitudeError * altitudeKp);
        rb.AddForce(Vector3.up * verticalForce);

        // 2. Roll & Pitch stabilizasyonu
        Quaternion currentRot = rb.rotation;
        Vector3 rightWorld = currentRot * Vector3.right;
        Vector3 forwardWorld = currentRot * Vector3.forward;
        float pitchError = Vector3.SignedAngle(forwardWorld, Vector3.ProjectOnPlane(forwardWorld, Vector3.up), rightWorld);
        float rollError = Vector3.SignedAngle(rightWorld, Vector3.ProjectOnPlane(rightWorld, Vector3.up), forwardWorld);
        float rollCorrection = -rollError * rollKp;
        float pitchCorrection = -pitchError * pitchKp;
        rb.AddTorque(currentRot * new Vector3(pitchCorrection, 0, rollCorrection));

        // 3. Yaw kontrolü (klasik: waypoint'e göre veya forcedYawDirection varsa kameraya döndür)
        float desiredYaw = 0f;
        bool useForcedYaw = false;
        if (forcedYawDirection >= 0)
        {
            useForcedYaw = true;
            Vector3 dir = Vector3.zero;
            switch(forcedYawDirection)
            {
                case 0: dir = transform.forward; break;
                case 1: dir = transform.right; break;
                case 2: dir = -transform.right; break;
                case 3: dir = -transform.forward; break;
                case 4: dir = transform.up; break;
                case 5: dir = -transform.up; break;
            }
            if (dir != Vector3.zero)
                desiredYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        }
        else
        {
            Vector3 flatTarget = new Vector3(target.x, transform.position.y, target.z);
            Vector3 forwardDir = flatTarget - transform.position;
            if (forwardDir.sqrMagnitude > 0.2f)
                desiredYaw = Mathf.Atan2(forwardDir.x, forwardDir.z) * Mathf.Rad2Deg;
            else
                desiredYaw = transform.eulerAngles.y;
        }

        float currentYaw = transform.eulerAngles.y;
        float yawError = Mathf.DeltaAngle(currentYaw, desiredYaw);

        if (Mathf.Abs(yawError) > 2f)
        {
            float yawRate = Mathf.Clamp(yawError * yawKp - rb.angularVelocity.y * 4f, -maxYawRate, maxYawRate);
            rb.AddTorque(Vector3.up * yawRate);
        }
        else
        {
            rb.angularVelocity = new Vector3(rb.angularVelocity.x, Mathf.Lerp(rb.angularVelocity.y, 0, 0.3f), rb.angularVelocity.z);
        }

        // 4. Linear hareket – sadece bir ana yön aktif
        Vector3 moveDir = Vector3.zero;
        bool escaping = false;

        // QR hizalaması sırasında, forcedYawDirection hangi yöndeyse O yöne gitsin
        if (useForcedYaw)
        {
            switch (forcedYawDirection)
            {
                case 0: if (frontObstacle != null && !frontObstacle.ObstacleDetected) moveDir = transform.forward; break;
                case 1: if (rightObstacle != null && !rightObstacle.ObstacleDetected) moveDir = transform.right; break;
                case 2: if (leftObstacle != null && !leftObstacle.ObstacleDetected) moveDir = -transform.right; break;
                case 3: if (backObstacle != null && !backObstacle.ObstacleDetected) moveDir = -transform.forward; break;
                case 4: if (upObstacle != null && !upObstacle.ObstacleDetected) moveDir = transform.up; break;
                case 5: if (downObstacle != null && !downObstacle.ObstacleDetected) moveDir = -transform.up; break;
            }
        }
        else if (frontObstacle != null && !frontObstacle.ObstacleDetected)
        {
            moveDir = transform.forward;
        }
        else if (rightObstacle != null && !rightObstacle.ObstacleDetected)
        {
            escaping = true;
            moveDir = transform.right;
        }
        else if (leftObstacle != null && !leftObstacle.ObstacleDetected)
        {
            escaping = true;
            moveDir = -transform.right;
        }
        else if (upObstacle != null && !upObstacle.ObstacleDetected)
        {
            escaping = true;
            moveDir = transform.up;
        }
        else if (downObstacle != null && !downObstacle.ObstacleDetected)
        {
            escaping = true;
            moveDir = -transform.up;
        }
        else if (backObstacle != null && !backObstacle.ObstacleDetected)
        {
            escaping = true;
            moveDir = -transform.forward;
        }

        // Frenleme (kaçınmada)
        if (escaping)
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.2f);

        // Hız limiti ve hareket
        Vector3 planarVel = rb.velocity; planarVel.y = 0f;
        if (moveDir == Vector3.zero)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.18f);
        }
        else if (planarVel.magnitude < maxTotalSpeed)
        {
            rb.AddForce(moveDir.normalized * (escaping ? avoidStrength : forwardKp), ForceMode.Acceleration);
        }

        // Açısal hız frenlemesi (X/Z eksenleri)
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, new Vector3(0, rb.angularVelocity.y, 0), Time.fixedDeltaTime * 3.5f);
    }
}
