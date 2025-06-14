using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider))]
public class AmbientTrigger : MonoBehaviour
{
    public AudioSource ambientSource;         // Привязанный AudioSource
    public float fadeDuration = 2f;           // Длительность затухания/возврата
    private bool isFading = false;
    private bool isPaused = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFading)
        {
            if (!isPaused)
                StartCoroutine(FadeOut());
            else
                StartCoroutine(FadeIn());
        }
    }

    private IEnumerator FadeOut()
    {
        isFading = true;
        float startVolume = ambientSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            ambientSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        ambientSource.volume = 0f;
        ambientSource.Pause(); // Пауза, не стоп
        isPaused = true;
        isFading = false;
    }

    private IEnumerator FadeIn()
    {
        isFading = true;
        ambientSource.UnPause();
        float targetVolume = 1f; // Можно сделать переменной
        float startVolume = ambientSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            ambientSource.volume = Mathf.Lerp(startVolume, targetVolume, t / fadeDuration);
            yield return null;
        }

        ambientSource.volume = targetVolume;
        isPaused = false;
        isFading = false;
    }
}
