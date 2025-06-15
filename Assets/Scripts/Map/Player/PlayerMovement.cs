using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float sprintMultiplier = 1.7f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    private CharacterController controller;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private bool isSprinting = false;

    private float velocityY = 0f;
    private bool isGrounded;

    private PlayerStamina playerStamina;

    public bool canMove = true;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        if (inputActions == null)
            inputActions = new InputSystem_Actions();
            
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Sprint.performed += OnSprint;
        inputActions.Player.Sprint.canceled += OnSprint;
    }

    void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;
            inputActions.Player.Sprint.performed -= OnSprint;
            inputActions.Player.Sprint.canceled -= OnSprint;
            inputActions.Player.Disable();
        }
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerStamina = GetComponent<PlayerStamina>();
    }

    void Update()
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            isSprinting = false;
        }

        isGrounded = controller.isGrounded;

        if (isGrounded && velocityY < 0)
            velocityY = -2f;

        velocityY += gravity * Time.deltaTime;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move.y = velocityY;

        if (isSprinting && playerStamina != null && playerStamina.currentStamina <= 0f)
            isSprinting = false;

        float currentSpeed = isSprinting ? speed * sprintMultiplier : speed;

        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = context.ReadValue<Vector2>();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        if (!canMove)
        {
            isSprinting = false;
            return;
        }

        var stamina = GetComponent<PlayerStamina>();
        if (stamina != null && stamina.currentStamina <= 0f)
        {
            isSprinting = false;
            return;
        }

        isSprinting = context.ReadValueAsButton();
    }

    public bool IsSprinting => isSprinting;
    public Vector2 MoveInput => moveInput;

    public void ResetInput()
    {
        moveInput = Vector2.zero;
        isSprinting = false;
    }

    public void MovementControl(bool Value)
    {
        canMove = Value;
    }
}
