using System;
using Enums;
using Network.Shared;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Util;

namespace Controller {
    public class NetFirstPersonController : NetController {

        public float speed = 12f;
        public float gravity = -9.81f;
        public float jumpHeight = 3f;
        public Transform aimComponent;
        public float mouseSensivity = 15f;
        public Camera playerCamera;

        [FormerlySerializedAs("Move speed")] [Header("Player")] [Tooltip("Move speed of the character in m/s")]
        public float moveSpeed = 2.0f;

        [FormerlySerializedAs("Sprint speed")] [Tooltip("Sprint speed of the character in m/s")]
        public float sprintSpeed = 5.335f;

        [FormerlySerializedAs("Speed changeRate")] [Tooltip("Acceleration and deceleration")]
        public float speedChangeRate = 10.0f;

        [FormerlySerializedAs("Fall timeout")] [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float fallTimeout = 0.15f;

        [FormerlySerializedAs("Jump timeout")] [Space(10)] [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float jumpTimeout = 0.05f;

        public Animator animator;
        public PlayableSoldier soldier;

        // player
        private float _terminalVelocity = 53.0f;


        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        
        //Ground
        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;
        protected bool isGrounded;
        
        
        [SerializeField]
        private NetworkVariable<Vector3> networkPositionDirection = new NetworkVariable<Vector3>();

        [SerializeField]
        private NetworkVariable<Vector2> networkRotationDirection = new NetworkVariable<Vector2>();

        [SerializeField]
        private NetworkVariable<float> networkVerticalVelocity = new NetworkVariable<float>();

        [SerializeField]
        private NetworkVariable<SoldierState> networkPlayerState = new NetworkVariable<SoldierState>();


        // client caches positions
        private Vector3 _oldInputPosition = Vector3.zero;
        private Vector2 _oldInputRotation = Vector2.zero;
        private SoldierState _oldSoldierState = SoldierState.Idle;

        public new void Awake() {
            base.Awake();
            _oldInputPosition = transform.position;
            inputActions.Player.Jump.performed += Jump;
            inputActions.Player.Disable();
            soldier = gameObject.GetComponent<PlayableSoldier>();
        }

        public override void OnNetworkDespawn() {
            inputActions.Player.Disable();
        }

        protected override void ClientVisuals()
        {
            if (_oldSoldierState != networkPlayerState.Value)
            {
                _oldSoldierState = networkPlayerState.Value;
                animator.SetTrigger($"{networkPlayerState.Value}");
            }
        }

        private void Jump(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Performed && isGrounded) {
                // Jump
                if (_jumpTimeoutDelta <= 0.0f) {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    UpdateVerticalVelocityServerRpc(Mathf.Sqrt(jumpHeight * -2f * gravity));

                    UpdatePlayerStateServerRpc(SoldierState.JumpStart);
                }
            }
        }

        protected override void ServerCalculations() {
            bool falling = false;
            float verticalVelocity = networkVerticalVelocity.Value;
            if (isGrounded) {
                // reset the fall timeout timer
                _fallTimeoutDelta = fallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (verticalVelocity < 0.0f) {
                    verticalVelocity = -2f;
                }


                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f) {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else {
                // reset the jump timeout timer
                _jumpTimeoutDelta = jumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f) {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else {
                    falling = true;
                }
            }

            if (falling) {
                UpdatePlayerStateServerRpc(SoldierState.OnAir);
            }
            else {
                UpdatePlayerStateServerRpc(SoldierState.Idle);
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (verticalVelocity < _terminalVelocity) {
                verticalVelocity += gravity * Time.deltaTime;
            }

            if (networkVerticalVelocity.Value != verticalVelocity) {
                UpdateVerticalVelocityServerRpc(verticalVelocity);
            }
        }

        protected override void ClientInput() {
            Vector3 inputPosition = KeyboardInput();
            Vector2 inputRotation = MouseInput();
            if (_oldInputPosition != inputPosition ||
                _oldInputRotation != inputRotation)
            {
                _oldInputPosition = inputPosition;
                _oldInputRotation = inputRotation;
                Debug.Log(inputPosition);
                UpdateClientPositionAndRotationServerRpc(inputPosition, inputRotation );
            }
        }
        

        private Vector2 MouseInput() {
            Vector2 movementInput = inputActions.Player.Look.ReadValue<Vector2>();
            float mouseX = movementInput.x * mouseSensivity * Time.deltaTime; //Up-Down
            float mouseY = movementInput.y * mouseSensivity * Time.deltaTime; //Left-Right

            float positionX = _oldInputRotation.y;
            positionX -= mouseY;
            positionX = Mathf.Clamp(positionX, -90f, 90f);

            return new Vector2(mouseX, positionX);
        }

        private Vector3 KeyboardInput() {
            Vector2 movementInput = inputActions.Player.Movement.ReadValue<Vector2>();
            return new Vector3(
                movementInput.x * speed * Time.deltaTime, 
                0, 
                movementInput.y * speed * Time.deltaTime);
        }

        protected override void ClientMovement()
        {
            if (networkPositionDirection.Value != Vector3.zero)
            {
                controller.Move(transform.TransformDirection(networkPositionDirection.Value));
            }
            if (networkRotationDirection.Value != Vector2.zero)
            {
                aimComponent.localRotation = Quaternion.Euler(networkRotationDirection.Value.y, 0f, 0f);
                transform.Rotate(Vector3.up * networkRotationDirection.Value.x);
            }
            
        }
        
        protected override void ClientBeforeInput() {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        
        
        [ServerRpc]
        private void UpdateClientPositionAndRotationServerRpc(Vector3 newPosition, Vector2 newRotation)
        {
            networkPositionDirection.Value = newPosition;
            networkRotationDirection.Value = newRotation;
        }

        [ServerRpc]
        private void UpdatePlayerStateServerRpc(SoldierState state)
        {
            networkPlayerState.Value = state;
        }

        [ServerRpc]
        private void UpdateVerticalVelocityServerRpc(float velocity) {
            networkVerticalVelocity.Value = velocity;
            networkPositionDirection.Value =
                new Vector3(networkPositionDirection.Value.x, velocity, networkPositionDirection.Value.z);
        }
    }
}