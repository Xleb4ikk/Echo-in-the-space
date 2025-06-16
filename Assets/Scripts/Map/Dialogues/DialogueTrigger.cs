using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string text;
    public string speakerName;
    public Color speakerNameColor = Color.yellow;
    public AudioClip typingSound;
    [Range(0.1f, 2.0f)]
    public float typingSpeed = 1.0f;

    public float lineDelay = 0f;
    public float eventDelay = 0f;

    public UnityEvent onLineEvent;
}

public class DialogueTrigger : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text speakerNameText;

    [Header("Dialogue Settings")]
    public DialogueLine[] dialogueLines;
    public float defaultTypingDelay = 0.05f;
    public float activationDelay = 0f;
    public float defaultLineDelay = 0f;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Player")]
    public GameObject playerObject;
    public string playerTag = "Player";

    [Header("Trigger Settings")]
    public bool activateOnStart = false;
    public bool activateOnTrigger = true;
    public bool debugMode = true;

    private bool isDialogueActive = false;
    private int currentLine = 0;
    private string currentText = "";
    private bool isTyping = false;
    private bool hasTriggered = false;
    private bool canProceed = false;
    private bool isDelayingAfterLine = false;
    private bool preventSkip = false;

    private Coroutine typingCoroutine = null;
    private Coroutine eventCoroutine = null;
    private Coroutine delayCoroutine = null;

    void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (activateOnStart)
            Invoke("ActivateDialogue", activationDelay);

        if (activateOnTrigger)
        {
            Collider col = GetComponent<Collider>();
            if (col == null || !col.isTrigger)
                Debug.LogWarning("[DialogueTrigger] Collider must be marked as Trigger.");
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isDialogueActive || isDelayingAfterLine) return;

        bool nextPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) nextPressed = true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) nextPressed = true;

        if (nextPressed)
        {
            if (isTyping)
            {
                if (preventSkip) return;

                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);

                dialogueText.text = dialogueLines[currentLine].text;
                isTyping = false;

                float waitTime = dialogueLines[currentLine].lineDelay > 0 ?
                                 dialogueLines[currentLine].lineDelay :
                                 defaultLineDelay;

                if (waitTime > 0)
                {
                    isDelayingAfterLine = true;
                    delayCoroutine = StartCoroutine(DelayBeforeNext(waitTime));
                    return;
                }

                canProceed = true;
            }
            else if (canProceed)
            {
                NextLine();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        CheckAndActivate(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckAndActivate(collision.gameObject);
    }

    private void CheckAndActivate(GameObject obj)
    {
        if (!activateOnTrigger || hasTriggered) return;

        if ((playerObject == null && obj.CompareTag(playerTag)) || obj == playerObject)
        {
            hasTriggered = true;
            ActivateDialogue();
        }
    }

    public void ActivateDialogue()
    {
        if (isDialogueActive || dialogueLines.Length == 0) return;

        isDialogueActive = true;
        currentLine = 0;
        dialoguePanel?.SetActive(true);
        ShowCurrentDialogueLine();
    }

    private void ShowCurrentDialogueLine()
    {
        DialogueLine line = dialogueLines[currentLine];

        if (speakerNameText != null && !string.IsNullOrEmpty(line.speakerName))
        {
            speakerNameText.text = line.speakerName;
            speakerNameText.color = line.speakerNameColor;
        }

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText(line));

        if (eventCoroutine != null) StopCoroutine(eventCoroutine);
        eventCoroutine = StartCoroutine(InvokeEventWithDelay(line));
    }

    private IEnumerator InvokeEventWithDelay(DialogueLine line)
    {
        if (line.eventDelay > 0f)
            yield return new WaitForSeconds(line.eventDelay);

        line.onLineEvent?.Invoke();
    }

    private IEnumerator DelayBeforeNext(float delay)
    {
        yield return new WaitForSeconds(delay);
        isDelayingAfterLine = false;
        preventSkip = false;
        canProceed = true;
    }

    private IEnumerator TypeText(DialogueLine line)
    {
        isTyping = true;
        isDelayingAfterLine = false;
        canProceed = false;
        preventSkip = line.lineDelay > 0;

        dialogueText.text = "";
        currentText = "";

        float delay = defaultTypingDelay / line.typingSpeed;

        foreach (char letter in line.text)
        {
            currentText += letter;
            dialogueText.text = currentText;

            if (audioSource != null && line.typingSound != null && letter != ' ')
                audioSource.PlayOneShot(line.typingSound);

            yield return new WaitForSeconds(delay);
        }

        isTyping = false;

        float waitTime = line.lineDelay > 0 ? line.lineDelay : defaultLineDelay;

        if (waitTime > 0)
        {
            isDelayingAfterLine = true;
            delayCoroutine = StartCoroutine(DelayBeforeNext(waitTime));
        }
        else
        {
            preventSkip = false;
            canProceed = true;
        }
    }

    public void NextLine()
    {
        if (isTyping)
        {
            if (preventSkip) return;

            if (typingCoroutine != null)
                StopCoroutine(typingCoroutine);

            dialogueText.text = dialogueLines[currentLine].text;
            isTyping = false;

            float waitTime = dialogueLines[currentLine].lineDelay > 0 ?
                             dialogueLines[currentLine].lineDelay :
                             defaultLineDelay;

            if (waitTime > 0)
            {
                isDelayingAfterLine = true;
                delayCoroutine = StartCoroutine(DelayBeforeNext(waitTime));
                return;
            }

            canProceed = true;
            return;
        }

        currentLine++;

        if (currentLine < dialogueLines.Length)
        {
            ShowCurrentDialogueLine();
        }
        else
        {
            EndDialogue();
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialoguePanel?.SetActive(false);
    }
}
