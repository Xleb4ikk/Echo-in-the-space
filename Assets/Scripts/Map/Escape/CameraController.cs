using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player; // Ссылка на объект игрока
    public float initialDistance = 10f; // Начальное расстояние камеры от игрока
    public float finalDistance = 20f; // Конечное расстояние камеры от игрока
    public float transitionSpeed = 2f; // Скорость отдаления камеры

    private Vector3 targetPosition; // Целевая позиция камеры
    private bool isMoving = false; // Флаг для проверки, началось ли движение

    void Update()
    {
        // Проверяем, если камера должна двигаться
        if (isMoving)
        {
            // Постепенно отдаляем камеру
            float step = transitionSpeed * Time.deltaTime;
            Vector3 direction = (transform.position - player.position).normalized;
            targetPosition = player.position + direction * finalDistance;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

            // Останавливаем движение, если достигли конечной позиции
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
            }
        }
    }

    // Метод для мгновенного перемещения камеры и запуска плавного отдаления
    public void TriggerCameraMovement()
    {
        Vector3 direction = (transform.position - player.position).normalized;
        transform.position = player.position + direction * initialDistance;
        isMoving = true;
    }
}
