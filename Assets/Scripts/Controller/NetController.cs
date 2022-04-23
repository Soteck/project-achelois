using Unity.Netcode;
using Unity.Netcode.Samples;
using UnityEngine;

namespace Controller {
    
    [RequireComponent(typeof(NetworkObject))]
    //[RequireComponent(typeof(ClientNetworkTransform))]
    public abstract class NetController : NetworkBehaviour {
        public CharacterController controller;

        protected PlayerInputActions inputActions;

        public void Awake() {
            inputActions = new PlayerInputActions();
            inputActions.Player.Enable();
        }
        
        void Update()
        {
            if (IsSpawned) {
                if (IsClient && IsOwner)
                {
                    ClientBeforeInput();
                    ClientInput();
                }

                if (IsServer) {
                    //ServerCalculations();
                }

                if (IsClient) {
                    ClientMovement();
                    ClientVisuals();
                }
            }
        }

        protected abstract void ServerCalculations();
        protected abstract void ClientBeforeInput();
        protected abstract void ClientInput();
        protected abstract void ClientMovement();
        protected abstract void ClientVisuals();

        public void EnableInputActions() {
            inputActions.Player.Enable();
        }
        
        public void Enable() {
            gameObject.SetActive(true);
            inputActions.Player.Enable();
        }
        
        public void Disable() {
            gameObject.SetActive(false);
            inputActions.Player.Disable();
        }
        
    }
}