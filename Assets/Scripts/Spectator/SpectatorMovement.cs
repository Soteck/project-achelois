using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpectatorMovement : MonoBehaviour {
    public CharacterController controller;

    public float speed = 12f;
    public float mouseSensivity = 100f;
    public float jumpHeight = 3f;
    public float gravity = -9.81f;

    [Header("Player")] [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")] [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public Transform playerBody;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;

    PlayerInputActions inputActions;


    // player
    private float xRotation = 0f;
    private float jumpVelocity;


    public void Awake() {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        jumpVelocity = gravity * -20f;
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }


    public void FixedUpdate() {
        MoveKeyboard();
        MoveMouse();
    }

    public void MoveMouse() {
        Vector2 movementInput = inputActions.Player.Look.ReadValue<Vector2>();
        float mouseX = movementInput.x * mouseSensivity * Time.deltaTime;
        float mouseY = movementInput.y * mouseSensivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerBody.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void MoveKeyboard() {
        Vector2 movementInput = inputActions.Player.Movement.ReadValue<Vector2>();
        bool isJumping = inputActions.Player.Jump.ReadValue<float>() > 0f;
        bool isCrouching = inputActions.Player.Crouch.ReadValue<float>() > 0f;

        Vector3 move = transform.right * movementInput.x + transform.forward * movementInput.y;
        //Debug.Log(move);
        controller.Move(move * speed * Time.deltaTime);

        if (isJumping) {
            velocity.y = jumpVelocity * Time.deltaTime;
        }
        else if (isCrouching) {
            velocity.y = jumpVelocity * Time.deltaTime * -1;
        }
        else {
            velocity.y = 0;
        }

        //Debug.Log(isGrounded + " : " + velocity);

        controller.Move(velocity * Time.deltaTime);
    }
}