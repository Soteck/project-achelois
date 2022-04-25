using Config;
using Unity.Netcode;
using UnityEngine;

namespace CharacterController {
    public class NetSpectatorController : NetworkBehaviour {
        public Camera playerCamera;
        public UnityEngine.CharacterController controller;
        public Transform lookTransform;

        public float speed = 12f;
        public float sprintMultiplier = 1.5f;

        // player
        private PlayerInputActions _inputActions;

        // client caches positions
        private float _internalXRotation;

        protected void Awake() {
            _inputActions = new PlayerInputActions();
            playerCamera.enabled = false;
            _inputActions.Player.Disable();
        }

        void Update() {
            if (IsSpawned) {
                if (IsClient && IsOwner) {
                    ClientInput();
                }
            }
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
            bool isJumping = _inputActions.Player.Jump.IsPressed();
            bool isCrouching = _inputActions.Player.Crouch.IsPressed();
            bool isSprinting = _inputActions.Player.Sprint.IsPressed();

            float verticalMovement = 0;
            if (isJumping) {
                verticalMovement += 1;
            }

            if (isCrouching) {
                verticalMovement -= 1;
            }

            if (isSprinting) {
                moveSpeed *= sprintMultiplier;
            }

            //If player presses both buttons it wont move. Not my business.
            return new Vector3(
                controllerHorizontalInput.x * moveSpeed * Time.deltaTime,
                verticalMovement * moveSpeed * Time.deltaTime,
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


        public static NetSpectatorController FindByOwnerId(ulong ownerId) {
            NetSpectatorController[] allControllers = FindObjectsOfType<NetSpectatorController>();
            foreach (NetSpectatorController controller in allControllers) {
                if (controller.OwnerClientId == ownerId) {
                    return controller;
                }
            }

            return null;
        }
    }
}