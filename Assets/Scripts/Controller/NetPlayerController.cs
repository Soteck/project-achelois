using Config;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Controller {
    public class NetPlayerController : NetworkBehaviour {
        public Camera playerCamera;
        public CharacterController controller;
        public Transform lookTransform;
        public PlayableSoldier soldier;

        public float speed = 12f;
        public float sprintMultiplier = 1.5f;
        public float gravity = -9.81f;
        public float jumpHeight = 3f;
        [FormerlySerializedAs("Fall timeout")] [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float fallTimeout = 0.15f;

        [FormerlySerializedAs("Jump timeout")] [Space(10)] [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float jumpTimeout = 0.05f;
        
        //Ground
        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;
        protected bool isGrounded;
        
        // player
        private PlayerInputActions _inputActions;

        // client caches positions
        private float _internalXRotation;
        private float _verticalVelocity;
        
        // timeout delta times
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private float _terminalVelocity = 53.0f;
        
        protected void Awake() {
            _inputActions = new PlayerInputActions();
            playerCamera.enabled = false;
            _inputActions.Player.Disable();
        }

        void Update() {
            if (IsSpawned) {
                if (IsClient && IsOwner) {
                    GroundCheck();
                    ClientInput();
                    JumpAndGravity();
                }
            }
        }

        private void JumpAndGravity() {
            bool falling = false;
            if (isGrounded) {
                // reset the fall timeout timer
                _fallTimeoutDelta = fallTimeout;

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f) {
                    _verticalVelocity = -2f;
                }


                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f) {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
                else {
                    if (_inputActions.Player.Jump.WasPerformedThisFrame()) {
                        _verticalVelocity = Mathf.Sqrt(jumpHeight * 2f);
                    }
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
            
            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity) {
                _verticalVelocity += gravity * Time.deltaTime;
            }
            controller.Move(transform.TransformDirection(new Vector3(0, _verticalVelocity, 0)));
        }

        private void GroundCheck() {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        private void ClientInput() {
            Vector3 movementInput = KeyboardInput();
            Vector3 rotationInput = MouseInput();
            controller.Move(transform.TransformDirection(movementInput));

            //Unity Y axis is vertical, rotating through it will look -Y = left // +Y = right
            //Unity X axis is horizontal, rotating through it will look -X = down // +X = up
            //Unity Z axis is depth, won't rotate through it
            transform.Rotate(new Vector3(0f, rotationInput.x, 0f), Space.World);

            _internalXRotation = Mathf.Clamp(_internalXRotation + rotationInput.y, -90f, 90f);
            lookTransform.localRotation = Quaternion.Euler(_internalXRotation, 0f, 0f);
        }


        private Vector2 MouseInput() {
            // -X = Left // + X = Right
            // -Y = UP // +Y = DOWN
            float mouseSensitivity = ConfigHolder.mouseSensitivity;
            Vector2 movementInput = _inputActions.Player.Look.ReadValue<Vector2>();

            float mouseXInput = movementInput.x * mouseSensitivity * Time.deltaTime;
            float mouseYInput;
            if (ConfigHolder.invertMouse) {
                mouseYInput = movementInput.y * -mouseSensitivity * Time.deltaTime;
            }
            else {
                mouseYInput = movementInput.y * mouseSensitivity * Time.deltaTime;
            }

            return new Vector2(mouseXInput, mouseYInput);
        }

        private Vector3 KeyboardInput() {
            Vector2 controllerHorizontalInput = _inputActions.Player.Movement.ReadValue<Vector2>();
            float moveSpeed = speed;
            bool isSprinting = _inputActions.Player.Sprint.IsPressed();


            if (isSprinting) {
                moveSpeed *= sprintMultiplier;
            }

            return new Vector3(
                controllerHorizontalInput.x * moveSpeed * Time.deltaTime,
                0,
                controllerHorizontalInput.y * moveSpeed * Time.deltaTime);
        }

        public void Enable() {
            gameObject.SetActive(true);
            _inputActions.Player.Enable();
        }

        public void Disable() {
            gameObject.SetActive(false);
            _inputActions.Player.Disable();
        }


        public static NetPlayerController FindByOwnerId(ulong ownerId) {
            NetPlayerController[] allControllers = FindObjectsOfType<NetPlayerController>();
            foreach (NetPlayerController controller in allControllers) {
                if (controller.OwnerClientId == ownerId) {
                    return controller;
                }
            }

            return null;
        }
    }
}