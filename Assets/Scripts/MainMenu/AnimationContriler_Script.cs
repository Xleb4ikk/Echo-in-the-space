using UnityEngine;

public class AnimationContriler_Script : MonoBehaviour
{
    [Header("������� ����")]
    public GameObject mainMenuPanel;

    // ���������� �� Animation Event
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
    }
}
