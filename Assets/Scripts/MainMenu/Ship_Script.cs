using UnityEngine;
using System.Collections;

public class Ship_Script : MonoBehaviour
{
    public Transform residential_module;

    public float minDelay = 1f;
    public float maxDelay = 3f;
    public float minAngle = 30f;
    public float maxAngle = 180f;
    public float rotationSpeed = 90f; // градусов в секунду

    private bool isRotating = false;
    private float targetAngleZ;

    void Start()
    {
        StartCoroutine(RotationCycle());
        Debug.Log("Update метод начал работать после задержкиddsdsdafdsgsghsdgh");
    }

    void Update()
    {
        //residential_module.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        if (isRotating && residential_module != null)
        {
            float currentZ = residential_module.localEulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentZ, targetAngleZ);
            float step = rotationSpeed * Time.deltaTime;

            if (Mathf.Abs(angleDiff) > 0.1f)
            {
                float rotationThisFrame = Mathf.Clamp(angleDiff, -step, step);
                residential_module.Rotate(0f, 0f, rotationThisFrame);
            }
            else
            {
                // Устанавливаем только Z, обнуляем X и Y
                residential_module.localRotation = Quaternion.Euler(0f, 0f, targetAngleZ);
                isRotating = false;
            }
        }
    }

    IEnumerator RotationCycle()
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            float currentZ = residential_module.localEulerAngles.z;
            float angleToAdd = Random.Range(minAngle, maxAngle);
            targetAngleZ = (currentZ + angleToAdd) % 360f;

            isRotating = true;

            // Ждём пока вращение завершится
            while (isRotating)
                yield return null;
        }
    }
}

