using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor.Rendering.LookDev;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Script : MonoBehaviour
{
    [Header("Скрипты")]
    public Ship_Script ship_script;

    [Header("Кнопки")]
    public Button NewGame_Button;
    public Button Settings_Button;
    public Button Exit_Button;

    [Header("Тексты")]
    public TextMeshProUGUI NewGame_Button_Text;
    public TextMeshProUGUI Settings_Button_Text;
    public TextMeshProUGUI Exit_Button_Text;

    [Header("Ui")]
    public Image Darkening;

    [Header("Настройки начала игры")]
    public float WaitingDarkening = 2f;
    public float durationDarkening = 3f;
    public float transparentDarkening = 255f;

    private bool DarkeningStart = false;

    void Start()
    {
        NewGame_Button.onClick.AddListener(StartNewGame);

        AddHoverHandler(NewGame_Button.gameObject, NewGame_Button_Text);
        AddHoverHandler(Settings_Button.gameObject, Settings_Button_Text);
        AddHoverHandler(Exit_Button.gameObject, Exit_Button_Text);
    }

    void Update()
    {
        
    }

    public void StartNewGame()
    {
        ship_script.StartNewGameAnimation();
        StartCoroutine(WaitDarkening());
    }

    private void StartChangeScene()
    {
        SceneManager.LoadScene(1);
    }

    private void AddHoverHandler(GameObject uiObject, TextMeshProUGUI linkedText)
    {
        var trigger = uiObject.AddComponent<PointerHoverHandler>();
        trigger.targetText = linkedText;
    }

    IEnumerator WaitDarkening()
    {
        Darkening.gameObject.SetActive(true);

        float timer = 0f;

        yield return new WaitForSeconds(WaitingDarkening);

        Color color = Darkening.color;

        while (timer < durationDarkening)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / durationDarkening);
            Darkening.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        StartChangeScene();
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