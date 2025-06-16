using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class ActivateGenerator : MonoBehaviour
{
    [Header("Анимации")]
    [SerializeField] private Animator ButtonAnimation;
    [SerializeField] private PlayableDirector director;

    [Header("Тригер кнопки")]
    [SerializeField] private Collider triggerZone;
    [SerializeField] private Transform playerTransform;

    [Header("Скрипты")]
    [SerializeField] private TeleportOnKey TeleportOnKeyPlayer;
    [SerializeField] private PlayerCamera PlayerCameraScript;
    [SerializeField] private Player PlayerMovementScript;

    [Header("Для затемнения экрана")]
    [SerializeField] private CameraFade CameraFade;

    [Header("Камера Анимации")]
    [SerializeField] private GameObject Camera;

    [Header("Двери")]
    [SerializeField] private GameObject AnimationDoor;
    [SerializeField] private GameObject RealDoor;

    [Header("Монстр")]
    [SerializeField] private GameObject FakeMonster;
    [SerializeField] private GameObject RealMonster;
    [SerializeField] private Transform MonsterTransform;

    [Header("Тригер диалога")]
    [SerializeField] private GameObject DialogTriger;

    [Header("Материалы")]
    [SerializeField] private Renderer ButtonMaterial;
    [SerializeField] private Material newMaterial;

    [Header("Звуковые источники")]
    [SerializeField] private AudioSource AmbientAudioSource;
    [SerializeField] private AudioSource GeneratorAudioSource;

    [Header("Звуки")]
    [SerializeField] private AudioClip AmbientAudio;
    [SerializeField] private AudioClip GeneratorAudio;

    [Header("Стены для отключения")]

    [SerializeField] private List<GameObject> VisibleWalls;

    [Header("Переменые")]
    public bool CatScene = false;

    public GameObject ActivateDoorTriger;

    public bool IsPlayerInside { get; private set; }

    Vector3 RunPos = new Vector3(14.80115f, -2.538255f, -50.82444f);

    void Update()
    {
        if (triggerZone != null && playerTransform != null)
        {
            IsPlayerInside = triggerZone.bounds.Contains(playerTransform.position);

            if (IsPlayerInside)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    DialogTriger.SetActive(true);

                    foreach (GameObject GameObject in VisibleWalls)
                    {
                        GameObject.SetActive(false);
                    }
                }
            }      
        }
    }

    public IEnumerator CatSceneLogic()
    {
        ActivateDoorTriger.SetActive(true);
        PlayerCameraScript.canMove = false;
        PlayerMovementScript.canMove = false;
        CameraFade.FadeIn();
        yield return new WaitForSeconds(1);
        RealDoor.SetActive(false);
        AnimationDoor.SetActive(true);
        FakeMonster.SetActive(true);
        Camera.SetActive(true);
        CameraFade.FadeOut();
        yield return new WaitForSeconds(1);
        
        
        TeleportOnKeyPlayer.Teleport(RunPos, 110.2f);
        yield return new WaitForSeconds(1);
        director.stopped += OnDirectorStopped;
        director.Play();
    }

    private void OnDirectorStopped(PlayableDirector pd)
    {
        StartCoroutine(CatSceneCancel());
    }

    public IEnumerator CatSceneCancel()
    {
        CameraFade.FadeIn();
        yield return new WaitForSeconds(1);
        FakeMonster.SetActive(false);
        Camera.SetActive(false);
        CameraFade.FadeOut();
        yield return new WaitForSeconds(1);
        PlayerCameraScript.canMove = true;
        PlayerMovementScript.canMove = true;
        RealMonster.SetActive(true);

        AmbientAudioSource.volume = 1;
        AmbientAudioSource.clip = AmbientAudio;
        AmbientAudioSource.Play();
    }

    public IEnumerator DialogLogic()
    {
        ButtonAnimation.SetBool("ButtonPresed", true);
        yield return new WaitForSeconds(0.30f);
        ButtonMaterial.material = newMaterial;
        yield return new WaitForSeconds(0.50f);
        GeneratorAudioSource.clip = GeneratorAudio;
        GeneratorAudioSource.Play();
    }

    public void StartDialogLogic()
    {
        StartCoroutine(DialogLogic());
    }

    public void StartCoroutineCatScene()
    {
        StartCoroutine(CatSceneLogic());
    }
}
