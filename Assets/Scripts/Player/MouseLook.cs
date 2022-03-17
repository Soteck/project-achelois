using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour {
    public float mouseSensivity = 100f;

    public Transform playerBody;
    PlayerInputActions inputActions;

    private float xRotation = 0f;

    public void Awake() {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }


    // Start is called before the first frame update
    public void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    public void FixedUpdate() {
        Vector2 movementInput = inputActions.Player.Look.ReadValue<Vector2>();
        float mouseX = movementInput.x * mouseSensivity * Time.deltaTime;
        float mouseY = movementInput.y * mouseSensivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}