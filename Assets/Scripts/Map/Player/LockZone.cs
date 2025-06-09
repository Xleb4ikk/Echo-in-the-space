using UnityEngine;

public class LockZone : MonoBehaviour
{
    public Transform Player;
    public float relativeTargetAngle = 90f;
    public float rotationDuration = 1.5f;

    public Player playerMovement;
    public PlayerCamera mouseLook;

    private bool isRotating = false;
    private float startY;
    private float targetY;
    private float elapsed = 0f;

    void Update()
    {
        if (!isRotating) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / rotationDuration);
        float smoothT = Mathf.SmoothStep(0, 1, t);

        float newY = Mathf.LerpAngle(startY, targetY, smoothT);
        Player.eulerAngles = new Vector3(Player.eulerAngles.x, newY, Player.eulerAngles.z);

        if (t >= 1f)
        {
            isRotating = false;
            SetControl(true); // Включаем управление после поворота
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform == Player)
        {
            startY = Player.eulerAngles.y;
            targetY = NormalizeAngle(startY + relativeTargetAngle);
            elapsed = 0f;
            isRotating = true;
            SetControl(false); // Отключаем управление во время поворота
        }
    }

    void SetControl(bool canMove)
    {
        if (playerMovement != null) playerMovement.canMove = canMove;
        if (mouseLook != null) mouseLook.canMove = canMove;
    }

    float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }
}
