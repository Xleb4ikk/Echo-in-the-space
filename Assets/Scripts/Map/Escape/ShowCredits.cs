using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Security;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class ShowTheEnd : MonoBehaviour
{
    public GameObject blackScreen;
    public TMP_Text endText;
    public float displayDuration = 5f;

    public AudioSource secondarySound;

    public float xyi = 0f;

    private bool checkaudio = false;

    private void Update()
    {
        if (checkaudio == false)
        {
            return;
        }

        if (secondarySound != null && secondarySound.isPlaying)
        {
            if (secondarySound.time >= xyi)
            {
                StartCoroutine(ShowEndCoroutine());
            }
        }
    }

    public void AudioCons()
    {
        checkaudio = true;
    }

    private IEnumerator ShowEndCoroutine()
    {

        blackScreen.SetActive(true);
        endText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayDuration);

        SceneManager.LoadScene(0);
    }
}
