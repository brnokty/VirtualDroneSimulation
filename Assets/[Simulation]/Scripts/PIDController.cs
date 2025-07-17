using UnityEngine;

[System.Serializable]
public class PIDController
{
    public float kp = 1.0f;   // Proportional
    public float ki = 0.0f;   // Integral
    public float kd = 0.0f;   // Derivative

    private float integral;
    private float lastError;

    public float Update(float target, float current, float deltaTime)
    {
        float error = target - current;
        integral += error * deltaTime;
        float derivative = (error - lastError) / deltaTime;
        lastError = error;

        return kp * error + ki * integral + kd * derivative;
    }
}