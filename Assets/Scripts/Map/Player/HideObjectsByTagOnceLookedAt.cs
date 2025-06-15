using UnityEngine;
using System.Collections;

public class HideOnLookedAt : MonoBehaviour
{
    public Camera playerCamera;
    public float maxDistance = 100f;
    public string targetTag = "Hideable"; // ��� �������
    public float Delay = 0.2f; // ������������ ������ 0.2 �������

    private bool isHiding = false;
    private bool isHidden = false;

    void Update()
    {
        if (isHidden || isHiding || playerCamera == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            if (hit.transform.CompareTag(targetTag))
            {
                StartCoroutine(HideObjectAfterDelay(hit.transform.gameObject));
                isHiding = true;
            }
        }
    }

    private IEnumerator HideObjectAfterDelay(GameObject obj)
    {
        yield return new WaitForSeconds(Delay);

        // ������� ������ ���� �� ��� ��� �� ������ � �������
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            if (hit.transform.gameObject == obj)
            {
                obj.SetActive(false); // ������ ������������
                isHidden = true;
            }
        }
    }

}
