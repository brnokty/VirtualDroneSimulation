using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneController : MonoBehaviour
{
    [Header("Flight Settings")]
    public float hoverHeight = 5f;
    public float maxThrust = 20f;
    public float maxTorque = 10f;

    [Header("PID Controllers")]
    public PIDController altitudePID = new PIDController { kp = 10f, ki = 0.1f, kd = 5f };
    public PIDController pitchPID = new PIDController { kp = 5f, ki = 0.1f, kd = 2f };
    public PIDController rollPID = new PIDController { kp = 5f, ki = 0.1f, kd = 2f };
    public PIDController yawPID = new PIDController { kp = 5f, ki = 0.1f, kd = 2f };

    [Header("Navigation")]
    public NavigationSystem navigator;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Hedef waypoint'i çek
        Vector3 target = navigator.GetCurrentTarget();

        Vector3 flatTarget = new Vector3(target.x, 0, target.z);
        Vector3 flatPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 direction = (flatTarget - flatPosition).normalized;

        float distance = Vector3.Distance(flatTarget, flatPosition);

        // Altitude PID
        float thrust = altitudePID.Update(hoverHeight, transform.position.y, dt);
        thrust = Mathf.Clamp(thrust, -maxThrust, maxThrust);
        rb.AddForce(Vector3.up * thrust);

        // Pitch/Roll PID → Hedef yönüne yönlen
        Vector3 localDirection = transform.InverseTransformDirection(direction);

        float pitch = pitchPID.Update(0, localDirection.z, dt);
        float roll = rollPID.Update(0, -localDirection.x, dt);

        rb.AddTorque(transform.right * pitch * maxTorque);
        rb.AddTorque(transform.forward * roll * maxTorque);

        // Yaw PID → Dönüş
        float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float currentYaw = transform.eulerAngles.y;
        float yawError = Mathf.DeltaAngle(currentYaw, targetYaw);

        float yaw = yawPID.Update(0, yawError, dt);
        rb.AddTorque(Vector3.up * yaw * maxTorque);
    }
}
