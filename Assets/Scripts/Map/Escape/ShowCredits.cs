using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ShowTheEnd : MonoBehaviour
{
    public GameObject blackScreen;  // Чёрный экран (Image)
    public TMP_Text endText;            // Текст "The END"
    public float displayDuration = 5f; // Время отображения экрана

    // Функция, вызываемая кнопкой
    public void ShowEndScreen()
    {
        StartCoroutine(ShowEndCoroutine());
    }

    private IEnumerator ShowEndCoroutine()
    {

        blackScreen.SetActive(true);
        endText.gameObject.SetActive(true);

        // Пауза на экране
        yield return new WaitForSeconds(displayDuration);

        // Оставьте экран или выполните другие действия (если нужно, отключите экран)
        // blackScreen.SetActive(false);
        // endText.gameObject.SetActive(false);
    }
}
