using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Ship_Script : MonoBehaviour
{
    [Header("Скрипты")]
    public UI_Script script;

    [Header("Модули корабля")]
    public Transform residential_module;
    public Transform Solar_Panel1;

    [Header("Настройки вращения солнечной панели")]
    public float minDelay = 1f;
    public float maxDelay = 3f;
    public float minAngle = 30f;
    public float maxAngle = 180f;
    public float rotationSpeed = 0.5f; // градусов в секунду

    [Header("Настройки вращения Жилого модуля")]
    public float residential_module_Rotate_Speed = 0.5f; // градусов в секунду

    private bool StartAnimation = false;
    private bool isRotating = false;
    private float targetAngleZ;

    public Animator animator;

    void Start()
    {
        StartAnimation = animator.GetBool("StartGame");
        Debug.Log(StartAnimation);
        StartCoroutine(RotationCycle());
    }

    void Update()
    {
        UpdateAnimatorSpeed();
        RotateResidentialModule();
        UpdateSolarPanelRotation();
    }

    public void StartNewGameAnimation()
    {
        animator.SetBool("StartGame", true);
        StartAnimation = animator.GetBool("StartGame");
    }

    void UpdateAnimatorSpeed()
    {
        if (StartAnimation == true)
        {
            float curveSpeed = animator.GetFloat("SpeedAnim");
            curveSpeed = Mathf.Max(0.3f, curveSpeed);
            animator.speed = curveSpeed;
            Debug.Log(curveSpeed);
        }
    }

    void RotateResidentialModule()
    {
        residential_module.Rotate(0f, 0f, residential_module_Rotate_Speed * Time.deltaTime);
    }

    void UpdateSolarPanelRotation()
    {
        if (!isRotating || Solar_Panel1 == null) return;

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
            Solar_Panel1.localRotation = Quaternion.Euler(0f, 0f, targetAngleZ);
            isRotating = false;
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

            // Ждём пока вращение завершится
            while (isRotating)
                yield return null;
        }
    }
}

