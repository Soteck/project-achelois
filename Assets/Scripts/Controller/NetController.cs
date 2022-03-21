using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

namespace Controller {
    
    [RequireComponent(typeof(NetworkObject))]
    //[RequireComponent(typeof(ClientNetworkTransform))]
    public abstract class NetController : NetworkBehaviour {
        public CharacterController controller;

        protected PlayerInputActions inputActions;
        
        public Transform groundCheck;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;
        protected bool isGrounded;

        public void Awake() {
            inputActions = new PlayerInputActions();
            inputActions.Player.Enable();
        }
        
        void Update()
        {
            GroundedCheck();
            if (IsClient && IsOwner)
            {
                ClientInput();
            }

            if (IsServer) {
                //ServerGravity();
            }

            ClientMoveAndRotate();
            //ClientVisuals();
        }

        protected abstract void ServerGravity();
        protected abstract void ClientInput();
        protected abstract void ClientMoveAndRotate();
        protected abstract void ClientVisuals();


        public void Enable() {
            gameObject.SetActive(true);
            inputActions.Player.Enable();
        }

        public void Disable() {
            gameObject.SetActive(false);
            inputActions.Player.Disable();
            
        }
        
        private void GroundedCheck() {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            //if (_hasAnimator) {
            //    animator.SetBool(_animIDGrounded, isGrounded);
            //}
        }
    }
}