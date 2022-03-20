using Controller;
using Player;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace Network {

    public class NetworkPlayer : NetworkBehaviour {

        public NetworkVariable<Vector3> position = new NetworkVariable<Vector3>();
        public NetworkVariable<Team> team = new NetworkVariable<Team>();
        
        public FirstPersonController firstPersonPrefab;
        public SpectatorController spectatorPrefab;

        private PlayerInputActions _inputActions;
        private bool locked = true;

        public void Awake() {
            _inputActions = new PlayerInputActions();
            _inputActions.Player.Enable();
            _inputActions.Player.Limbo.performed += Limbo;
            _inputActions.Player.Menu.performed += Menu;
        }

        public void Start() {
            firstPersonPrefab.Disable();
            spectatorPrefab.Enable();
        }
        
        private void Menu(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Performed) {
                locked = !locked;
                if (locked) {
                    Cursor.lockState = CursorLockMode.Locked;
                } else {
                    
                    Cursor.lockState = CursorLockMode.Confined;
                }
            }

        }

        private void Limbo(InputAction.CallbackContext context) {
            if (context.phase == InputActionPhase.Performed) {
                if (firstPersonPrefab.isActiveAndEnabled) {
                    firstPersonPrefab.Disable();
                    spectatorPrefab.Enable();
                    position.Value = spectatorPrefab.gameObject.transform.position;
                    team.Value = Team.SPECTATOR;
                } else {
                    firstPersonPrefab.Enable();
                    spectatorPrefab.Disable();
                    team.Value = Team.TEAM_A;
                    position.Value = firstPersonPrefab.gameObject.transform.position;
                }
            }
        }
    }
}