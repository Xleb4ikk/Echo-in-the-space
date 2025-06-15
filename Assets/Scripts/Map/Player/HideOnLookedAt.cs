using UnityEngine;
using System.Collections;

public class HideOnLookedAt : MonoBehaviour
{
    public Camera playerCamera;
    public float viewAngle = 10f;
    public float maxDistance = 100f;
    public float delay = 0.2f;

    private bool isPlayerInTrigger = false;
    private bool isHiding = false;
    private bool isHidden = false;

    public void PlayerEnteredTrigger()
    {
        isPlayerInTrigger = true;
    }

    public void PlayerExitedTrigger()
    {
        isPlayerInTrigger = false;
    }

    void Update()
    {
        if (!isPlayerInTrigger || isHidden || isHiding || playerCamera == null)
            return;

        Vector3 toObject = transform.position - playerCamera.transform.position;
        float angle = Vector3.Angle(playerCamera.transform.forward, toObject);

        if (angle < viewAngle && toObject.sqrMagnitude <= maxDistance * maxDistance)
        {
            StartCoroutine(HideAfterDelay());
            isHiding = true;
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        Vector3 toObject = transform.position - playerCamera.transform.position;
        float angle = Vector3.Angle(playerCamera.transform.forward, toObject);

        if (angle < viewAngle && toObject.sqrMagnitude <= maxDistance * maxDistance)
        {
            gameObject.SetActive(false);
            isHidden = true;
        }

        isHiding = false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;

        Gizmos.color = Color.cyan;

        Vector3 origin = playerCamera.transform.position;
        Vector3 forward = playerCamera.transform.forward;

        Gizmos.DrawLine(origin, origin + forward * maxDistance);

        Quaternion leftRot = Quaternion.AngleAxis(-viewAngle, playerCamera.transform.up);
        Quaternion rightRot = Quaternion.AngleAxis(viewAngle, playerCamera.transform.up);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + leftDir * maxDistance);
        Gizmos.DrawLine(origin, origin + rightDir * maxDistance);
    }
#endif
}
