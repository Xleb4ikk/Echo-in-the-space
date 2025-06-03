using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float sprintMultiplier = 1.7f; // 70% быстрее
    public float gravity = -9.81f;  // гравитация
    public float jumpHeight = 1.5f; // опционально для прыжков

    private CharacterController controller;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool isSprinting = false;

    private float velocityY = 0f;
    private bool isGrounded;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Sprint.performed += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;
    }

    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Sprint.performed -= OnSprint;
        inputActions.Player.Sprint.canceled -= OnSprint;
        inputActions.Player.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocityY < 0)
        {
            velocityY = -2f; // небольшой отскок вниз, чтобы "держаться" на земле
        }

        // Применяем гравитацию
        velocityY += gravity * Time.deltaTime;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        // Добавляем вертикальное движение
        move.y = velocityY;

        // Выбираем скорость: обычная или спринт
        float currentSpeed = isSprinting ? speed * sprintMultiplier : speed;

        // Двигаем контроллер
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.ReadValueAsButton();
    }
}