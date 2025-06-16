using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class ShowTheEnd : MonoBehaviour
{
    public GameObject blackScreen;  // ׸���� ����� (Image)
    public TMP_Text endText;            // ����� "The END"
    public float displayDuration = 5f; // ����� ����������� ������

    // �������, ���������� �������
    public void ShowEndScreen()
    {
        StartCoroutine(ShowEndCoroutine());
    }

    private IEnumerator ShowEndCoroutine()
    {

        blackScreen.SetActive(true);
        endText.gameObject.SetActive(true);

        // ����� �� ������
        yield return new WaitForSeconds(displayDuration);

        // �������� ����� ��� ��������� ������ �������� (���� �����, ��������� �����)
        // blackScreen.SetActive(false);
        // endText.gameObject.SetActive(false);
    }
}
