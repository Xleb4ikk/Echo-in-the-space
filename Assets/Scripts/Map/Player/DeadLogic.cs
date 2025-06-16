using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class DeadLogic : MonoBehaviour
{
    [Header("TimeLine")]
    [SerializeField] private PlayableDirector director;

    [Header("Важные обьекты")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Collider triggerZone;
    [SerializeField] private Player player;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private GameObject DeadCamera;
    [SerializeField] private MonsterChase MonsterChase;

    [Header("Скрипты затемнения экрана")]
    [SerializeField] private CameraFade Fade;

    [Header("Настройки сцены")]
    private string sceneToLoad;
    private AsyncOperation preloadOperation;

    [Header("Переменые")]
    [SerializeField] private bool Ischase = false;

    [Header("Приватные Переменые")]
    public bool IsPlayerInside { get; private set; }
    private bool isdead = false;

    void Start()
    {
        sceneToLoad = SceneManager.GetActiveScene().name;

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            preloadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
            preloadOperation.allowSceneActivation = false;
        }
    }

    void Update()
    {
        if (isdead) return;

        if (triggerZone != null && playerTransform != null)
        {
            IsPlayerInside = triggerZone.bounds.Contains(playerTransform.position);

            if (IsPlayerInside)
            {
                MonsterChase.isEnabled = false;
                StartCoroutine(StartDead());
                isdead = true;
            }
        }
    }

    [SerializeField]
    private IEnumerator StartDead()
    {
        player.canMove = false;
        playerCamera.canMove = false;
        DeadCamera.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        director.Play();

        yield return new WaitForSeconds(10f);

        if (preloadOperation != null)
        {
            preloadOperation.allowSceneActivation = true;
        }
    }

    [Tooltip("Вспомогательные мотоды")]
    public void ChangeIschase(bool Value)
    {
        Ischase = Value;
    }

    public void FadeControler()
    {
        if (Fade != null)
        {
            Fade.FadeIn("Вы погибли");
        }
    }
}
