using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float jumpHeight = 0.8f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float lookLimit = 85f;

    [Header("Stamina")]
    [SerializeField] private float sprintDuration = 3f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;
    private float staminaRemaining;

    public float StaminaRemaining => staminaRemaining;

    public float StaminaNormalized =>
        sprintDuration > 0f ? staminaRemaining / sprintDuration : 0f;

    public bool HasStartedMoving { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        staminaRemaining = sprintDuration;

        if (cameraTransform == null)
        {
            Debug.LogError("Assign the player's camera in the Inspector.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard == null || mouse == null)
            return;

        // Escape releases the cursor for working in the editor.
        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            ReleaseCursor();
            return;
        }

        // Click the Game view to capture the mouse.
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            if (mouse.leftButton.wasPressedThisFrame)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            return;
        }

        Look(mouse);
        Move(keyboard);
    }

    private void Look(Mouse mouse)
    {
        Vector2 mouseDelta =
            mouse.delta.ReadValue() *
            mouseSensitivity *
            GameSettingsManager.MouseSensitivityMultiplier;

        transform.Rotate(Vector3.up * mouseDelta.x);

        cameraPitch -= mouseDelta.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -lookLimit, lookLimit);

        cameraTransform.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void Move(Keyboard keyboard)
    {
        Vector2 input = Vector2.zero;

        if (keyboard.wKey.isPressed) input.y += 1f;
        if (keyboard.sKey.isPressed) input.y -= 1f;
        if (keyboard.dKey.isPressed) input.x += 1f;
        if (keyboard.aKey.isPressed) input.x -= 1f;

        // Prevent diagonal movement from being faster.
        input = Vector2.ClampMagnitude(input, 1f);

        bool hasMovementInput = input.sqrMagnitude > 0f;
        bool grounded = controller.isGrounded;

        bool wantsJump =
            keyboard.spaceKey.wasPressedThisFrame && grounded;

        // Jumping also counts as starting the run.
        if (hasMovementInput || wantsJump)
            HasStartedMoving = true;

        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (wantsJump)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        float deltaTime = Time.deltaTime;
        float distance = moveSpeed * deltaTime;

        bool wantsSprint =
            keyboard.leftShiftKey.isPressed &&
            hasMovementInput &&
            staminaRemaining > 0f;

        if (wantsSprint)
        {
            float sprintTime = Mathf.Min(staminaRemaining, deltaTime);

            distance += (sprintSpeed - moveSpeed) * sprintTime;

            staminaRemaining =
                Mathf.Max(0f, staminaRemaining - sprintTime);
        }

        Vector3 direction =
            transform.right * input.x +
            transform.forward * input.y;

        verticalVelocity += gravity * deltaTime;

        Vector3 movement = direction * distance;
        movement.y = verticalVelocity * deltaTime;

        CollisionFlags collisions = controller.Move(movement);

        // Stop upward movement when hitting a ceiling.
        if ((collisions & CollisionFlags.Above) != 0 &&
            verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ReleaseCursor();
    }

    private void OnDisable()
    {
        ReleaseCursor();
    }

    private void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}