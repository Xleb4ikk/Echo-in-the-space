using UnityEngine;

public class ModelButton : MonoBehaviour
{
    public ButtonType buttonType; // Выбирается в Inspector

    private void OnMouseDown()
    {
        EventManager.ModelClicked(buttonType, gameObject);
    }
}