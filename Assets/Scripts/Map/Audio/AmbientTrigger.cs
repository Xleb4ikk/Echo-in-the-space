using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class AmbientTrigger : MonoBehaviour
{
    public AudioSource ambientSource;
    public float fadeDuration = 2f;
    public int maxTriggerCount = 2; // <-- Количество срабатываний (в инспекторе)
    
    private bool isFading = false;
    private bool isPaused = false;
    private int triggerCount = 0;   // <-- Сколько раз сработал

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isFading && triggerCount < maxTriggerCount)
        {
            triggerCount++;

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

        // Дождаться одного кадра для гарантии, что звук не "проскочит"
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        ambientSource.Pause(); // <-- теперь точно ничего не проиграется (ахуеть просто)
        isPaused = true;
        isFading = false;
    }


    private IEnumerator FadeIn()
    {
        isFading = true;
        ambientSource.UnPause();
        float targetVolume = 1f;
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
