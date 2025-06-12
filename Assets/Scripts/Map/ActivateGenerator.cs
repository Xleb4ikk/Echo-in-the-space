using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class ActivateGenerator : MonoBehaviour
{
    public Collider triggerZone;
    public Transform playerTransform;
    public Transform MonsterTransform;
    public Animator ButtonAnimation;
    public PlayableDirector director;
    public GameObject Camera;
    public Renderer ButtonMaterial;
    public Material newMaterial;
    public PlayerCamera PlayerCameraScript;
    public Player PlayerMovementScript;
    public TeleportOnKey TeleportOnKeyPlayer;
    public CameraFade CameraFade;
    public GameObject FakeMonster;
    public GameObject RealMonster;

    public bool IsPlayerInside { get; private set; }

    public bool CatScene = false;

    Vector3 RunPos = new Vector3(14.80115f, -2.538255f, -50.82444f);

    void Start()
    {
        
    }

    void Update()
    {
        if (triggerZone != null && playerTransform != null)
        {
            IsPlayerInside = triggerZone.bounds.Contains(playerTransform.position);

            if (IsPlayerInside)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    StartCoroutine(CatSceneLogic());
                }
            }      
        }
    }

    IEnumerator CatSceneLogic()
    {
        PlayerCameraScript.canMove = false;
        PlayerMovementScript.canMove = false;
        CameraFade.FadeIn();
        yield return new WaitForSeconds(1);
        FakeMonster.SetActive(true);
        Camera.SetActive(true);
        CameraFade.FadeOut();
        yield return new WaitForSeconds(1);
        ButtonAnimation.SetBool("ButtonPresed", true);
        ButtonMaterial.material = newMaterial;
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
    }
}
