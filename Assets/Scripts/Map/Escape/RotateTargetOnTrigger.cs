using UnityEngine;
using System.Collections;

public class RotateTargetOnTrigger : MonoBehaviour
{
    public Transform targetObject;
    public Vector3 desiredWorldEulerRotation = new Vector3(30.541f, -90f, 0f);
    public float rotationDuration = 1.5f;

    private bool isRotating = false;

    private void OnTriggerEnter(Collider other)
    {
        if (targetObject == null || targetObject.parent == null || isRotating)
        {
            return;
        }

        Quaternion worldTargetRotation = Quaternion.Euler(desiredWorldEulerRotation);
        Quaternion parentRotation = targetObject.parent.rotation;

        Quaternion localTargetRotation = Quaternion.Inverse(parentRotation) * worldTargetRotation;

        StartCoroutine(RotateSmoothly(targetObject, targetObject.localRotation, localTargetRotation, rotationDuration));
    }

    private IEnumerator RotateSmoothly(Transform target, Quaternion from, Quaternion to, float duration)
    {
        isRotating = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }

        target.localRotation = to;
        isRotating = false;
    }
}
