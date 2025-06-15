using UnityEngine;
using TMPro;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [Header("Ссылки на общую панель")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;

    [Header("Настройки для этого триггера")]
    public float delay = 0.05f;
    public AudioClip typingSoundClip;
    public string[] dialogueLines;

    private string fullText;
    private string currentText = "";
    private int currentLine = 0;
    private bool isActive = false;
    private AudioSource audioSource;

    void Start()
    {
        if (dialogueText == null)
        {
            Debug.LogError("Dialogue text reference is missing on " + gameObject.name);
        }
    }

    public void ActivateDialogue()
    {
        Debug.Log("ActivateDialogue called on " + gameObject.name);
        if (!isActive && dialogueLines != null && dialogueLines.Length > 0)
        {
            if (dialoguePanel == null || dialogueText == null)
            {
                Debug.LogError("Cannot activate dialogue: missing references on " + gameObject.name);
                return;
            }
            
            isActive = true;
            dialoguePanel.SetActive(true);
            StartDialogue(dialogueLines[0]);
        }
        else
        {
            if (isActive)
                Debug.Log("Dialogue is already active");
            else if (dialogueLines == null || dialogueLines.Length == 0)
                Debug.LogError("No dialogue lines set on " + gameObject.name);
        }
    }

    private void StartDialogue(string text)
    {
        fullText = text;
        currentText = "";
        dialogueText.text = "";
        StartCoroutine(ShowText());
    }

    private IEnumerator ShowText()
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            currentText = fullText.Substring(0, i);
            dialogueText.text = currentText;
            if (i > 0 && fullText[i-1] != ' ' && audioSource != null && typingSoundClip != null)
            {
                audioSource.PlayOneShot(typingSoundClip);
            }
            yield return new WaitForSeconds(delay);
        }
    }

    public void NextDialogue()
    {
        currentLine++;
        if (currentLine < dialogueLines.Length)
        {
            StartDialogue(dialogueLines[currentLine]);
        }
        else
        {
            isActive = false;
            dialoguePanel.SetActive(false);
        }
    }
}