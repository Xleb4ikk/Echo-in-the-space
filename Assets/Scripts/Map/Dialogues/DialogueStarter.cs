using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    public TypewriterEffect typewriter;
    public string[] dialogueLines = new string[]
    {
        "Закрыл его. Температура вот-вот упадет.",
        "Ты ведь не хочешь сидеть и дрожать во время шторма.",
        "Время шторма."
    };
    public float activationDelay = 0f;

    void Start()
    {
        if (typewriter == null)
        {
            Debug.LogError("TypewriterEffect reference is missing on " + gameObject.name);
            return;
        }

        Debug.Log("Setting dialogue lines on " + gameObject.name);
        typewriter.dialogueLines = dialogueLines;
        
        if (activationDelay > 0)
        {
            Debug.Log("Will activate dialogue after " + activationDelay + " seconds");
            Invoke("ActivateDialogue", activationDelay);
        }
    }

    public void ActivateDialogue()
    {
        if (typewriter == null)
        {
            Debug.LogError("Cannot activate dialogue: TypewriterEffect reference is missing on " + gameObject.name);
            return;
        }
        
        Debug.Log("Activating dialogue from DialogueStarter on " + gameObject.name);
        typewriter.ActivateDialogue();
    }
}