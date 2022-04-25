using Controller;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterController {
    public class FirstPersonController : BaseController {
        public UnityEngine.CharacterController controller;

        public float speed = 12f;
        public float gravity = -9.81f;
        public float jumpHeight = 3f;

        [Header("Player")] [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Space(10)] [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.05f;

        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;

        private Vector3 velocity;
        private bool isGrounded;

        public Animator animator;
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

        protected override void AfterAwake() {
            inputActions.Player.Jump.performed += Jump;
        }

        private void Start() {
            AssignAnimationIDs();
        }


        public void FixedUpdate() {
            Gravity();
            GroundedCheck();
            Move();
            Vector2 movementInput = inputActions.Player.Movement.ReadValue<Vector2>();

            if (isGrounded && velocity.y < 0f) {
                velocity.y = -2f;
            }

            //TODO: I think this is not needed, redundant with Move(); function (copypaste mistake)
            Vector3 move = transform.right * movementInput.x + transform.forward * movementInput.y;
            //Debug.Log(move);
            controller.Move(move * speed * Time.deltaTime);


            velocity.y += gravity * Time.deltaTime;
            //Debug.Log(isGrounded + " : " + velocity);

            controller.Move(velocity * Time.deltaTime);
        }

        private void Move() {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = inputActions.Player.Sprint.ReadValue<float>() > 0f ? SprintSpeed : MoveSpeed;

            Vector2 movementInput = inputActions.Player.Movement.ReadValue<Vector2>();
            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (movementInput == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = movementInput.magnitude;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset) {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

            // normalise input direction
            Vector3 inputDirection = new Vector3(movementInput.x, 0.0f, movementInput.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (movementInput != Vector2.zero) {
                //_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                //float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                // rotate to face input direction relative to camera position
                //transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, 0f, 0.0f) * Vector3.forward;

            // move the player
            controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                            new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator) {
                animator.SetFloat(_animIDSpeed, _animationBlend);
                animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
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

        private void Gravity() {
            if (isGrounded) {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

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
                _jumpTimeoutDelta = JumpTimeout;

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


        private void GroundedCheck() {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            if (_hasAnimator) {
                animator.SetBool(_animIDGrounded, isGrounded);
            }
        }

        private void AssignAnimationIDs() {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }
    }
}