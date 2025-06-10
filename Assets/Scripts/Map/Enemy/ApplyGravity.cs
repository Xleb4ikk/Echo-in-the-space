using UnityEngine;

public class ApplyGravity : MonoBehaviour
{
    public float gravity = -9.81f; // сила гравитации
    public float groundLevel = 0.0f; // уровень земли
    private float verticalVelocity = 0f;

    void Update()
    {
        // Если не на земле — применяем гравитацию
        if (transform.position.y > groundLevel)
        {
            verticalVelocity += gravity * Time.deltaTime;
            transform.position += new Vector3(0, verticalVelocity * Time.deltaTime, 0);
        }
        else
        {
            // Если на земле — обнуляем вертикальную скорость
            transform.position = new Vector3(transform.position.x, groundLevel, transform.position.z);
            verticalVelocity = 0;
        }
    }
}
