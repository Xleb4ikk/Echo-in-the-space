using UnityEngine;

public class EscapePod : MonoBehaviour
{
    public float acceleration = 6f;
    public float launchDuration = 3f;

    private Rigidbody rb;
    private bool launched = false;
    private float launchTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void FixedUpdate()
    {
        if (launched)
        {
            if (Time.time - launchTime < launchDuration)
            {
                rb.linearVelocity = transform.forward * acceleration;
            }
            else
            {
                launched = false;
                // Оставляем скорость как есть (по инерции)
            }
        }
    }

    public void Launch()
    {
        if (launched) return;

        transform.parent = null;
        rb.isKinematic = false;
        launched = true;
        launchTime = Time.time;
    }
}
