using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class UI_Script : MonoBehaviour
{
    [Header("Кнопки")]
    public Button NewGame_Button;
    public Button Settings_Button;
    public Button Exit_Button;

    [Header("Тексты")]
    public TextMeshProUGUI NewGame_Button_Text;
    public TextMeshProUGUI Settings_Button_Text;
    public TextMeshProUGUI Exit_Button_Text;

    void Start()
    {
        NewGame_Button.onClick.AddListener(OnNewGameClick);

        AddHoverHandler(NewGame_Button.gameObject, NewGame_Button_Text);
        AddHoverHandler(Settings_Button.gameObject, Settings_Button_Text);
        AddHoverHandler(Exit_Button.gameObject, Exit_Button_Text);
    }

    void Update()
    {
        
    }

    void OnNewGameClick()
    {
        Debug.Log("New Game");
    }

    private void AddHoverHandler(GameObject uiObject, TextMeshProUGUI linkedText)
    {
        var trigger = uiObject.AddComponent<PointerHoverHandler>();
        trigger.targetText = linkedText;
    }
}

public class PointerHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public TextMeshProUGUI targetText;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = new Color32(210, 196, 196, 255);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = Color.white;
    }
}