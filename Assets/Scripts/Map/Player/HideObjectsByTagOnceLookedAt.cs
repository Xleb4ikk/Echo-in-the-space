using UnityEngine;

public class HideOnLookedAt : MonoBehaviour
{
    public Camera playerCamera;
    public float maxDistance = 100f;
    public string targetTag = "Hideable"; // Тег объекта, который нужно прятать

    private bool isHidden = false;
    private GameObject hiddenObject = null;

    void Update()
    {
        if (isHidden) return;
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            if (hit.transform.CompareTag(targetTag))
            {
                HideObject(hit.transform.gameObject);
            }
        }
    }

    void HideObject(GameObject obj)
    {
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
        foreach (var r in rends)
        {
            r.enabled = false;
        }
        isHidden = true;
        hiddenObject = obj;
    }
}
