using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Controller {
    public class NetFirstPersonController : NetController {

        public float speed = 12f;
        public float gravity = -9.81f;
        public float jumpHeight = 3f;
        public Transform aimComponent;
        public float mouseSensivity = 15f;

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
        
        
        private Vector3 _velocity;

        private bool _hasAnimator = true;


        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        // player
        private float _speed;
        private float _animationBlend;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;


        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        
        [SerializeField]
        private NetworkVariable<Vector3> networkPositionDirection = new NetworkVariable<Vector3>();

        [SerializeField]
        private NetworkVariable<Vector2> networkRotationDirection = new NetworkVariable<Vector2>();

        [SerializeField]
        private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();


        // client caches positions
        private Vector3 _oldInputPosition = Vector3.zero;
        private Vector2 _oldInputRotation = Vector2.zero;
        private PlayerState _oldPlayerState = PlayerState.Idle;

        public new void Awake() {
            base.Awake();
            inputActions.Player.Jump.performed += Jump;
        }

        private void Start() {
            AssignAnimationIDs();
        }

        protected override void ClientVisuals()
        {
            if (_oldPlayerState != networkPlayerState.Value)
            {
                _oldPlayerState = networkPlayerState.Value;
                animator.SetTrigger($"{networkPlayerState.Value}");
            }
        }

        public void Jump(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Performed && isGrounded) {
                // Jump
                if (_jumpTimeoutDelta <= 0.0f) {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                    // update animator if using character
                    if (_hasAnimator) {
                        animator.SetBool(_animIDJump, true);
                    }
                }
            }
        }

        protected override void ServerGravity() {
            if (isGrounded) {
                // reset the fall timeout timer
                _fallTimeoutDelta = fallTimeout;

                // update animator if using character
                if (_hasAnimator) {
                    animator.SetBool(_animIDJump, false);
                    animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f) {
                    _verticalVelocity = -2f;
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
                    // update animator if using character
                    if (_hasAnimator) {
                        animator.SetBool(_animIDFreeFall, true);
                    }
                }
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity) {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }




        private void AssignAnimationIDs() {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        protected override void ClientInput() {
            Vector3 inputPosition = KeyboardInput();
            Vector2 inputRotation = MouseInput();
            if (_oldInputPosition != inputPosition ||
                _oldInputRotation != inputRotation)
            {
                _oldInputPosition = inputPosition;
                _oldInputRotation = inputRotation;
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
            //Debug.Log("keyboard movement input old input: " + _oldInputPosition);
            return new Vector3(
                movementInput.x * speed * Time.deltaTime, 
                0, 
                movementInput.y * speed * Time.deltaTime);
        }

        protected override void ClientMoveAndRotate()
        {
            if (networkPositionDirection.Value != Vector3.zero)
            {
                //Debug.Log("Moving controller to: " + networkPositionDirection.Value);
                controller.Move(networkPositionDirection.Value);
            }
            if (networkRotationDirection.Value != Vector2.zero)
            {
                aimComponent.localRotation = Quaternion.Euler(networkRotationDirection.Value.y, 0f, 0f);
                transform.Rotate(Vector3.up * networkRotationDirection.Value.x);
            }
        }
        
        
        [ServerRpc]
        public void UpdateClientPositionAndRotationServerRpc(Vector3 newPosition, Vector2 newRotation)
        {
            //Debug.Log("Updating " + newPosition + newRotation);
            networkPositionDirection.Value = newPosition;
            networkRotationDirection.Value = newRotation;
        }

        [ServerRpc]
        public void UpdatePlayerStateServerRpc(PlayerState state)
        {
            networkPlayerState.Value = state;
        }
    }
}