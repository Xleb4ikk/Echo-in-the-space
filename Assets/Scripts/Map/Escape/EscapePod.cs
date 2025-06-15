using UnityEngine;

public class EscapePod : MonoBehaviour
{
    public float acceleration = 7f; // ускорение в м/с^2
    public float launchDuration = 3f;

    private Rigidbody rb;
    private bool launched = false;
    private bool accelerating = false;
    private float launchTime;
    private float currentSpeed = 0f;
    private Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true; // Делаем капсулу кинематической с самого начала
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        if (launched)
        {
            float elapsed = Time.time - launchTime;

            if (elapsed < launchDuration && accelerating)
            {
                // Увеличиваем скорость
                currentSpeed += acceleration * Time.fixedDeltaTime;
                // Перемещаем капсулу напрямую
                transform.position += transform.forward * currentSpeed * Time.fixedDeltaTime;
            }
            else if (accelerating)
            {
                // Заканчиваем ускорение, но продолжаем движение
                accelerating = false;
            }
            else
            {
                // Продолжаем движение с постоянной скоростью
                transform.position += transform.forward * currentSpeed * Time.fixedDeltaTime;
            }
        }
    }

    public void Launch()
    {
        if (launched) return;

        transform.parent = null;
        rb.useGravity = false;
        rb.isKinematic = true;
        currentSpeed = 0f;
        startPosition = transform.position;

        launched = true;
        accelerating = true;
        launchTime = Time.time;
    }
}
