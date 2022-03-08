using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f;
    public float jumpHeight = 3f; 

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    PlayerInputActions inputActions;

    public void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.Jump.performed += Jump;

    }


    public void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        Vector2 movementInput = inputActions.Player.Movement.ReadValue<Vector2>();

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        Vector3 move = transform.right * movementInput.x + transform.forward * movementInput.y;
        controller.Move(move * speed * Time.deltaTime);


        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            controller.Move(velocity * Time.deltaTime);
        }

    }
}
