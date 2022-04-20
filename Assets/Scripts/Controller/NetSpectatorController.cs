using Unity.Netcode;
using UnityEngine;

namespace Controller {
    public class NetSpectatorController : NetController {
        public Camera playerCamera;

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

        private Vector3 _velocity;

        // player
        private float _xRotation = 0f;
        private float _jumpVelocity;

        [SerializeField]
        private NetworkVariable<Vector3> networkPositionDirection = new NetworkVariable<Vector3>();

        [SerializeField]
        private NetworkVariable<Vector2> networkRotationDirection = new NetworkVariable<Vector2>();

        [SerializeField]
        private NetworkVariable<float> networkVerticalVelocity = new NetworkVariable<float>();
        

        // client caches positions
        private Vector3 _oldInputPosition = Vector3.zero;
        private Vector2 _oldInputRotation = Vector2.zero;
        
        protected new void Awake() {
            base.Awake();
            _jumpVelocity = gravity * -20f;
            inputActions.Player.Disable();
        }
        

        protected override void ClientBeforeInput() {
            //empty
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

        protected override void ClientMovement() {
            if (networkPositionDirection.Value != Vector3.zero)
            {
                controller.Move(transform.TransformDirection(networkPositionDirection.Value));
            }
            if (networkRotationDirection.Value != Vector2.zero)
            {
                playerBody.localRotation = Quaternion.Euler(networkRotationDirection.Value.y, 0f, 0f);
                transform.Rotate(Vector3.up * networkRotationDirection.Value.x);
            }
        }

        protected override void ClientVisuals() {
            
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

        private void MoveKeyboard() {
            Vector2 movementInput = inputActions.Player.Movement.ReadValue<Vector2>();
            bool isJumping = inputActions.Player.Jump.ReadValue<float>() > 0f;
            bool isCrouching = inputActions.Player.Crouch.ReadValue<float>() > 0f;

            Vector3 move = transform.right * movementInput.x + transform.forward * movementInput.y;
            //Debug.Log(move);
            controller.Move(move * speed * Time.deltaTime);

            if (isJumping) {
                _velocity.y = _jumpVelocity * Time.deltaTime;
            }
            else if (isCrouching) {
                _velocity.y = _jumpVelocity * Time.deltaTime * -1;
            }
            else {
                _velocity.y = 0;
            }

            //Debug.Log(isGrounded + " : " + velocity);

            controller.Move(_velocity * Time.deltaTime);
        }

        private Vector3 KeyboardInput() {
            Vector2 movementInput = inputActions.Player.Movement.ReadValue<Vector2>();
            bool isJumping = inputActions.Player.Jump.ReadValue<float>() > 0f;
            bool isCrouching = inputActions.Player.Crouch.ReadValue<float>() > 0f;
            return new Vector3(
                movementInput.x * speed * Time.deltaTime, 
                0, 
                movementInput.y * speed * Time.deltaTime);
        }
        
        protected override void ServerCalculations() {
            //Empty
        }
        
        [ServerRpc]
        private void UpdateClientPositionAndRotationServerRpc(Vector3 newPosition, Vector2 newRotation)
        {
            networkPositionDirection.Value = newPosition;
            networkRotationDirection.Value = newRotation;
        }
    }
}