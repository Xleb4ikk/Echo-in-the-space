using UnityEngine;

public class AnimationContriler_Script : MonoBehaviour
{
    [Header("Ui ��������")]
    public GameObject mainMenuPanel;

    //[Header("���������� ��������")]

    // ���������� �� Animation Event
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
    }
}
