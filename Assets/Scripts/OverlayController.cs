using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Logger = Core.Logger;

public class OverlayController : MonoBehaviour {
    public float consoleDeployTime = 0.1f;
    public float consoleHeight = 120;
    public RectTransform console;
    public RectTransform menu;
    public RectTransform limbo;

    [SerializeField] private Button startServerButton;

    [SerializeField] private Button startHostButton;

    [SerializeField] private Button startClientButton;
    
    [SerializeField] private Button joinTeamAButton;
    
    [SerializeField] private Button joinTeamBButton;


    private PlayerInputActions _inputActions;
    private bool _consoleDeployed = false;
    private bool _menuDeployed = false;
    private bool _limboDeployed = false;
    private bool _consoleMoving = false;
    private float _consoleEndMoving = 0f;

    public void Awake() {
        console.gameObject.SetActive(false);
        menu.gameObject.SetActive(false);
        limbo.gameObject.SetActive(false);
        UndeployConsole();
        HideMenu();
        HideLimbo();
        SetupActions();
        SetupButtons();
    }

    private void SetupButtons() {
        // START SERVER
        startServerButton?.onClick.AddListener(() => {
            if (NetworkManager.Singleton.StartServer()) {
                Logger.Info("Server started...");
            }
            else {
                Logger.Info("Unable to start server...");
            }
            HideMenu();
            UndeployConsole();
        });

        // START HOST
        startHostButton?.onClick.AddListener(async () => {
            // this allows the UnityMultiplayer and UnityMultiplayerRelay scene to work with and without
            // relay features - if the Unity transport is found and is relay protocol then we redirect all the 
            // traffic through the relay, else it just uses a LAN type (UNET) communication.
            // if (RelayManager.Instance.IsRelayEnabled) {
            //     await RelayManager.Instance.SetupRelay();
            // }

            if (NetworkManager.Singleton.StartHost()) {
                Logger.Info("Host started...");
            }
            else {
                Logger.Info("Unable to start host...");
            }
            HideMenu();
            UndeployConsole();
        });

        // START CLIENT
        startClientButton?.onClick.AddListener(async () => {
            // if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(joinCodeInput.text)) {
            //     await RelayManager.Instance.JoinRelay(joinCodeInput.text);
            // }

            if (NetworkManager.Singleton.StartClient()) {
                Logger.Info("Client started...");
            }
            else {
                Logger.Info("Unable to start client...");
            }
            HideMenu();
            UndeployConsole();
        });
    }

    private void SetupActions() {
        _inputActions = new PlayerInputActions();
        _inputActions.Player.Enable();
        _inputActions.Player.ConsoleOpen.performed += context => {
            if (!_menuDeployed && !_limboDeployed) {
                if (_consoleDeployed) {
                    UndeployConsole();
                }
                else {
                    DeployConsole();
                }
            }
        };
        _inputActions.Player.Limbo.performed += context => {
            if (!_menuDeployed && !_consoleDeployed) {
                if (_limboDeployed) {
                    HideLimbo();
                }
                else {
                    ShowLimbo();
                }
            }
        };
        _inputActions.Player.Menu.performed += context => {
            if (_consoleDeployed) {
                UndeployConsole();
                return;
            }

            if (_limboDeployed) {
                HideLimbo();
                return;
            }
            if (_menuDeployed) {
                HideMenu();
            }
            else {
                ShowMenu();
            }
        };
    }

    private void Update() {
        if (_consoleMoving) {
            if (Time.time > (_consoleEndMoving + 0.2f)) {
                _consoleMoving = false;
                if (!_consoleDeployed) {
                    console.gameObject.SetActive(false);
                }
            }

            console.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, GetConsolePosition(), 1);
        }
    }

    private float GetConsolePosition() {
        float timeDiff = _consoleEndMoving - Time.time;
        float moment = 0;
        if (timeDiff > 0) {
            moment = timeDiff / consoleDeployTime; // % of the deploy;
        }

        if (_consoleDeployed) {
            return moment * -consoleHeight;
        }
        else {
            return -consoleHeight - (moment * -consoleHeight);
        }
    }

    private void UndeployConsole() {
        _consoleDeployed = false;
        _consoleEndMoving = Time.time + consoleDeployTime;
        _consoleMoving = true;
        UnlockCursor();
    }

    private void DeployConsole() {
        Logger.Draw();
        _consoleDeployed = true;
        _consoleEndMoving = Time.time + consoleDeployTime;
        _consoleMoving = true;
        console.gameObject.SetActive(true);
        LockCursor();
    }

    private void ShowMenu() {
        _menuDeployed = true;
        menu.gameObject.SetActive(true);
        UnlockCursor();
    }

    private void HideMenu() {
        _menuDeployed = false;
        menu.gameObject.SetActive(false);
        LockCursor();
    }

    private void ShowLimbo() {
        _limboDeployed = true;
        limbo.gameObject.SetActive(true);
        UnlockCursor();
    }

    private void HideLimbo() {
        _limboDeployed = false;
        limbo.gameObject.SetActive(false);
        LockCursor();
    }

    private void LockCursor() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void UnlockCursor() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
}