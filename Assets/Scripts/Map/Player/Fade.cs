using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraFade : MonoBehaviour
{
    public Image fadeImage;          // UI Image, растянутый на весь экран, цвет черный
    public float fadeDuration = 1f;  // длительность затемнения

    void Start()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(false);
        }
    }

    public void FadeIn()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(Fade(0f, 1f));
        }
    }

    public void FadeOut()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(Fade(1f, 0f));
        }
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }

        c.a = endAlpha;
        fadeImage.color = c;

        if (endAlpha == 0f)
            fadeImage.gameObject.SetActive(false);
    }
}
