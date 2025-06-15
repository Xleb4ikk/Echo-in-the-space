using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerButton : MonoBehaviour
{
    public string animationBoolName = "IsPressed";
    public string animationDoorBoolName = "IsOpen";
    public AudioClip soundEffect;
    public Animator targetAnimator;
    public Animator DoorAnimator; 
    public AudioSource audioSource;
    public Renderer ButtonRenderer;
    public Material ButtonMaterialEnabled;
    public Material ButtonMaterialDisabled;
    public bool IsPlayerInside { get; private set; }
    public Transform playerTransform;
    public Collider triggerZone;

    private void Start()
    {
        audioSource.clip = soundEffect;
    }

    private void Update()
    {
        if (triggerZone != null && playerTransform != null)
        {
            IsPlayerInside = triggerZone.bounds.Contains(playerTransform.position);

            if (IsPlayerInside)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    StartCoroutine(ButtonPresed());
                }
            }
        }
    }

    IEnumerator ButtonPresed()
    {
        ButtonRenderer.material = ButtonMaterialEnabled;

        if (targetAnimator != null)
            targetAnimator.SetBool(animationBoolName, true);

        if (soundEffect != null && audioSource != null)
            audioSource.Play();

        yield return new WaitForSeconds(0.10f);

        if (targetAnimator != null)
            targetAnimator.SetBool(animationBoolName, false);

        yield return new WaitForSeconds(0.30f);

        ButtonRenderer.material = ButtonMaterialDisabled;

        if (DoorAnimator != null)
            DoorAnimator.SetBool(animationDoorBoolName, true);
    }
}
