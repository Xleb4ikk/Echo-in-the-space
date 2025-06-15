using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform playerBody; // Перетащите сюда объект игрока в инспекторе
    public float mouseSensitivity = 1f;

    private InputSystem_Actions inputActions;
    private Vector2 lookInput;
    private float xRotation = 0f;

    public bool canMove = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        // Проверяем, что inputActions инициализирован
        if (inputActions == null)
            inputActions = new InputSystem_Actions();
            
        inputActions.Player.Enable();
        inputActions.Player.Look.performed += OnLook;
        inputActions.Player.Look.canceled += OnLook;
    }

    void OnDisable()
    {
        // Проверяем, что inputActions инициализирован
        if (inputActions != null)
        {
            inputActions.Player.Look.performed -= OnLook;
            inputActions.Player.Look.canceled -= OnLook;
            inputActions.Player.Disable();
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Скрыть и зафиксировать курсор
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return;

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Вращаем только по X (вверх/вниз) — КАМЕРА
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Вращаем только по Y (влево/вправо) — ИГРОК
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    // Добавьте этот метод для сброса ввода камеры
    public void ResetInput()
    {
        lookInput = Vector2.zero;
    }

    public void CamerMovementConstrol(bool Value)
    {
        canMove = Value;
    }
}
