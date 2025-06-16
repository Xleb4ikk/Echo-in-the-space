using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CameraFade : MonoBehaviour
{
    public Image fadeImage;              // UI Image, черный экран
    public TMP_Text fadeText;            // TMP Text поверх изображения
    public float fadeDuration = 1f;      // длительность затемнения

    void Start()
    {
        if (fadeImage != null)
        {
            SetAlpha(fadeImage, 0f);
            fadeImage.gameObject.SetActive(false);
        }

        if (fadeText != null)
        {
            SetAlpha(fadeText, 0f);
            fadeText.gameObject.SetActive(false);
        }
    }

    public void FadeIn(string text = "")
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
        }

        if (fadeText != null)
        {
            fadeText.text = text;
            fadeText.gameObject.SetActive(true);
        }

        StartCoroutine(Fade(0f, 1f));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);

            if (fadeImage != null)
                SetAlpha(fadeImage, alpha);

            if (fadeText != null)
                SetAlpha(fadeText, alpha);

            yield return null;
        }

        if (fadeImage != null)
        {
            SetAlpha(fadeImage, endAlpha);
            if (endAlpha == 0f) fadeImage.gameObject.SetActive(false);
        }

        if (fadeText != null)
        {
            SetAlpha(fadeText, endAlpha);
            if (endAlpha == 0f) fadeText.gameObject.SetActive(false);
        }
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }

    private void SetAlpha(TMP_Text text, float alpha)
    {
        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }
}
