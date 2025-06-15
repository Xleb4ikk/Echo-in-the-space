using UnityEngine;

public class ActivateObjectOnTriger : MonoBehaviour
{
    public Collider triggerZone;
    public GameObject triggerObject;
    public Transform playerTransform;
    public GameObject GameObject;
    public bool Destroid = false;
    private bool CompleteScript = false;

    void Update()
    {
        //if (CompleteScript == true) { return; }

        if(triggerZone != null && playerTransform != null)
        {
            bool isPlayerInside = triggerZone.bounds.Contains(playerTransform.position);

            if (isPlayerInside)
            {
                GameObject.SetActive(true);
            }
            
            //if (Destroid == true)
            //{
            //    triggerObject.SetActive(false);
            //}

            CompleteScript = true;
        }
    }
}
