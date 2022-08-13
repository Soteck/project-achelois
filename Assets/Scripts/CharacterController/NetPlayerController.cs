using Config;
using Enums;
using Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Serialization;
using Util;
using NetworkPlayer = Network.NetworkPlayer;

namespace CharacterController {
    public class NetPlayerController : NetworkBehaviour {
        public Camera playerCamera;
        public UnityEngine.CharacterController controller;
        public Transform lookTransform;
        public PlayableSoldier soldier;
        public Animator animator;
        public NetworkAnimator netAnimator;

        public float speed = 2f;
        public float sprintMultiplier = 2f;
        public float gravity = -9.81f;
        public float jumpHeight = 3f;

        [FormerlySerializedAs("Fall timeout")]
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float fallTimeout = 0.15f;

        [FormerlySerializedAs("Jump timeout")]
        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
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
        
        
        private NetworkPlayer _networkPlayer = null;


        //Local client-side references only
        private Vector2? _pastPosition = null;
        private static readonly int KnockedDown = Animator.StringToHash("knockedDown");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int Speed = Animator.StringToHash("Speed");

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
                    OwnerVisuals();
                }

                ClientVisuals();
            }
        }

        private void ClientVisuals() {
            animator.SetBool(KnockedDown, soldier.IsKnockedDown() || soldier.IsDead());
            Vector2 actualPosition = transform.position;
            if (_pastPosition != null) {
                float moveSpeed = (actualPosition - (Vector2) _pastPosition).magnitude / Time.deltaTime;
                animator.SetFloat(Speed, moveSpeed);
            }

            _pastPosition = actualPosition;
        }

        private void OwnerVisuals() {
            animator.SetBool(Grounded, isGrounded);
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
                    if (_inputActions.Player.Jump.WasPerformedThisFrame() && soldier.IsAlive()) {
                        _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
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

            controller.Move(transform.TransformDirection(new Vector3(0, _verticalVelocity * Time.deltaTime, 0)));
        }

        private void GroundCheck() {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        private void ClientInput() {
            if (soldier.IsDead()) {
                return;
            }
            if (soldier.IsAlive()) {
                Vector3 movementInput = KeyboardInput();
                Vector3 controllerMotion = transform.TransformDirection(movementInput);
                //Debug.Log("Moving controller: " + controllerMotion);
                controller.Move(controllerMotion);
            }
            
            Vector3 rotationInput = MouseInput();

            //Unity Y axis is vertical, rotating through it will look -Y = left // +Y = right
            //Unity X axis is horizontal, rotating through it will look -X = down // +X = up
            //Unity Z axis is depth, won't rotate through it
            transform.Rotate(new Vector3(0f, rotationInput.x, 0f), Space.World);

            _internalXRotation = Mathf.Clamp(_internalXRotation + rotationInput.y, -90f, 90f);
            Quaternion lookEuler = Quaternion.Euler(_internalXRotation, 0f, 0f);

            lookTransform.localRotation = lookEuler;
            //playerCamera.transform.localRotation = lookEuler;
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
            float moveSpeed = 0;
            if (controllerHorizontalInput[0] != 0 || controllerHorizontalInput[1] != 0) {
                
                moveSpeed = speed;
                
                if (_inputActions.Player.Sprint.IsPressed()) {
                    moveSpeed *= sprintMultiplier;
                }
                
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

        public NetworkPlayer networkPlayer {
            get {
                if (_networkPlayer == null) {
                    _networkPlayer = NetworkUtil.FindNetworkPlayerByOwnerId(OwnerClientId);
                }

                return _networkPlayer;
            }

            set => _networkPlayer = value;
        }
    }
}