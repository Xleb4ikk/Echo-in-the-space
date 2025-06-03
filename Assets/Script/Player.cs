using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;  // гравитация
    public float jumpHeight = 1.5f; // опционально для прыжков

    private CharacterController controller;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;

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
    }

    void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
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

        // Двигаем контроллер
        controller.Move(move * speed * Time.deltaTime);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
}