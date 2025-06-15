using UnityEngine;
using UnityEngine.Events;

public class SmoothCameraRotation : MonoBehaviour
{
    public Transform targetTransform;
    public Vector3 targetEulerAngles;
    public float rotationSpeed = 90f; // градусов в секунду

    private Quaternion targetRotation;
    private Quaternion originalRotation;
    private bool originalRotationSet = false;

    private bool rotatingToTarget = false;
    private bool returningToOriginal = false;

    public UnityEvent onStartRotateToTarget;
    public UnityEvent onCompleteRotateToTarget;
    public UnityEvent onStartReturnToOriginal;
    public UnityEvent onCompleteReturnToOriginal;

    void Start()
    {
        if (targetTransform == null)
            targetTransform = transform;

        originalRotation = targetTransform.rotation;
        targetRotation = Quaternion.Euler(targetEulerAngles);
    }

    void Update()
    {
        if (targetTransform == null) return;

        if (rotatingToTarget)
        {
            targetTransform.rotation = Quaternion.RotateTowards(targetTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(targetTransform.rotation, targetRotation) < 0.1f)
            {
                targetTransform.rotation = targetRotation;
                rotatingToTarget = false;
                onCompleteRotateToTarget?.Invoke();
            }
        }
        else if (returningToOriginal)
        {
            targetTransform.rotation = Quaternion.RotateTowards(targetTransform.rotation, originalRotation, rotationSpeed * Time.deltaTime);
            if (Quaternion.Angle(targetTransform.rotation, originalRotation) < 0.1f)
            {
                targetTransform.rotation = originalRotation;
                returningToOriginal = false;
                onCompleteReturnToOriginal?.Invoke();
                originalRotationSet = false;
            }
        }
    }

    public void StartRotateToTarget()
    {
        if (targetTransform == null) return;

        if (!originalRotationSet)
        {
            originalRotation = targetTransform.rotation;
            originalRotationSet = true;
        }

        targetRotation = Quaternion.Euler(targetEulerAngles);
        rotatingToTarget = true;
        returningToOriginal = false;
        onStartRotateToTarget?.Invoke();
    }

    public void ReturnToOriginalRotation()
    {
        if (targetTransform == null || !originalRotationSet) return;

        returningToOriginal = true;
        rotatingToTarget = false;
        onStartReturnToOriginal?.Invoke();
    }

    public void SetTargetRotation(Vector3 newEulerAngles)
    {
        targetEulerAngles = newEulerAngles;
        targetRotation = Quaternion.Euler(targetEulerAngles);
    }
}
