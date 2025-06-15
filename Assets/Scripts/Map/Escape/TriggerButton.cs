using UnityEngine;
using UnityEngine.InputSystem;

public class TriggerButton : MonoBehaviour
{
    public string animationBoolName = "IsPressed";   // ��� ��������� � Animator
    public AudioClip soundEffect;                    // ���� ��� ���������
    public Animator targetAnimator;                  // Animator ����� ������ ��������
    public AudioSource audioSource;
    private bool playerInTrigger = false;            // ����: ����� � ����
    public bool IsPlayerInside { get; private set; }
    public Transform playerTransform;
    public Collider triggerZone;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (playerInTrigger && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (targetAnimator != null)
                targetAnimator.SetBool(animationBoolName, true);

            if (soundEffect != null && audioSource != null)
                audioSource.PlayOneShot(soundEffect);
        }

        if (triggerZone != null && playerTransform != null)
        {
            IsPlayerInside = triggerZone.bounds.Contains(playerTransform.position);

            if (IsPlayerInside)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    if (targetAnimator != null)
                        targetAnimator.SetBool(animationBoolName, true);

                    if (soundEffect != null && audioSource != null)
                        audioSource.PlayOneShot(soundEffect);

                    Debug.Log("Process ");
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;

            if (targetAnimator != null)
                targetAnimator.SetBool(animationBoolName, false);
        }
    }
}
