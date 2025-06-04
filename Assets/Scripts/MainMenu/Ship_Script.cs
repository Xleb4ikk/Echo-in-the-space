using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Ship_Script : MonoBehaviour
{
    public Transform residential_module;
    public Transform Solar_Panel1;
    public Button flameButton;

    public float minDelay = 1f;
    public float maxDelay = 3f;
    public float minAngle = 30f;
    public float maxAngle = 180f;
    public float residential_module_Rotate_Speed = 0.5f; // градусов в секунду
    public float rotationSpeed = 0.5f; // градусов в секунду

    private bool isRotating = false;
    private bool NewGame = false;
    private float targetAngleZ;

    public Animator animator;

    void Start()
    {
        StartCoroutine(RotationCycle());
        flameButton.onClick.AddListener(() => {
            animator.SetBool("StartGame", true);
            SceneManager.LoadScene(1);
        });
    }

    void Update()
    {
        residential_module.Rotate(0f, 0f, residential_module_Rotate_Speed * Time.deltaTime);

        if (isRotating && Solar_Panel1 != null)
        {
            float currentZ = Solar_Panel1.localEulerAngles.z;
            float angleDiff = Mathf.DeltaAngle(currentZ, targetAngleZ);
            float step = rotationSpeed * Time.deltaTime;

            if (Mathf.Abs(angleDiff) > 0.1f)
            {
                float rotationThisFrame = Mathf.Clamp(angleDiff, -step, step);
                Solar_Panel1.Rotate(0f, 0f, rotationThisFrame);
            }
            else
            {
                // ”станавливаем только Z, обнул€ем X и Y
                Solar_Panel1.localRotation = Quaternion.Euler(0f, 0f, targetAngleZ);
                isRotating = false;
            }
        }

        if (NewGame == true)
        {

        }
    }


    IEnumerator RotationCycle()
    {
        while (true)
        {
            float delay = UnityEngine.Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            float currentZ = residential_module.localEulerAngles.z;
            float angleToAdd = UnityEngine.Random.Range(minAngle, maxAngle);
            targetAngleZ = (currentZ + angleToAdd) % 360f;

            isRotating = true;

            // ∆дЄм пока вращение завершитс€
            while (isRotating)
                yield return null;
        }
    }
}

