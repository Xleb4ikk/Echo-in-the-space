using UnityEngine;

public class LaunchTrigger : MonoBehaviour
{
    public EscapePod escapePod;
    public TriggerButton launchButton;

    private bool alreadyLaunched = false;

    void Start()
    {
        if (launchButton != null)
        {
            launchButton.OnButtonPressed += HandleButtonPress;
        }
    }

    void OnDestroy()
    {
        if (launchButton != null)
        {
            launchButton.OnButtonPressed -= HandleButtonPress;
        }
    }

    private void HandleButtonPress()
    {
        if (alreadyLaunched) return;

        // Находим игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Прикрепляем игрока к капсуле
            player.transform.parent = escapePod.transform;
            
            // Настраиваем физику игрока
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.useGravity = false;
                playerRb.isKinematic = true;
                playerRb.linearVelocity = Vector3.zero;
            }

            escapePod.Launch();
            alreadyLaunched = true;
        }
    }
}
