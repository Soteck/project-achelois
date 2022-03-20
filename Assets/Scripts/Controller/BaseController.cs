using UnityEngine;

namespace Controller {
    public abstract class BaseController : MonoBehaviour {

        protected PlayerInputActions inputActions;
        protected abstract void AfterAwake();
        public void Awake() {
            inputActions = new PlayerInputActions();
            AfterAwake();
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