using Core;
using Enums;
using Player;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.UI;
using Logger = Core.Logger;
using NetworkPlayer = Network.NetworkPlayer;

public class OverlayController : MonoBehaviour {
    public float consoleDeployTime = 0.1f;
    public float consoleHeight = 120;
    public RectTransform console;
    public TMP_InputField consoleInput;
    
    public RectTransform menu;
    public RectTransform limbo;
    public RectTransform hud;
    public RectTransform playerStats;

    [SerializeField] private Button startServerButton;

    [SerializeField] private Button startHostButton;

    [SerializeField] private Button startClientButton;

    [SerializeField] private Button quitGameButton;
    
    [SerializeField] private Button joinTeamAButton;
    
    [SerializeField] private Button joinTeamBButton;
    
    [SerializeField] private Button joinSpectatorButton;


    private PlayerInputActions _inputActions;
    private bool _consoleDeployed = false;
    private bool _menuDeployed = false;
    private bool _limboDeployed = false;
    private bool _consoleMoving = false;
    private float _consoleEndMoving = 0f;

    private bool _showingStats = false;

    public void Awake() {
        console.gameObject.SetActive(false);
        menu.gameObject.SetActive(false);
        limbo.gameObject.SetActive(false);
        HideAllElements();
        SetupActions();
        SetupButtons();
        consoleInput.onSubmit.AddListener(value => {
            consoleInput.text = "";
            FocusConsole();
            Logger.Info(value);
            ConsoleCommandController.ExecuteCommand(value);
        });
    }

    private void SetupButtons() {
        // START SERVER
        startServerButton.onClick.AddListener(() => {
            ConsoleCommandController.ExecuteCommand("server start");
            HideAllElements();
        });

        // START HOST
        startHostButton.onClick.AddListener(async () => {
            ConsoleCommandController.ExecuteCommand("server host");
            HideAllElements();
        });

        // START CLIENT
        startClientButton.onClick.AddListener(async () => {
            ConsoleCommandController.ExecuteCommand("connect localhost");
            HideAllElements();
        });
        
        joinTeamAButton.onClick.AddListener(() => {
            NetworkPlayer.networkPlayerOwner.RequestJoinTeam(Team.TeamA);
            HideAllElements();
        });
        
        joinTeamBButton.onClick.AddListener(() => {
            NetworkPlayer.networkPlayerOwner.RequestJoinTeam(Team.TeamB);
            HideAllElements();
        });        
        
        joinSpectatorButton.onClick.AddListener(() => {
            NetworkPlayer.networkPlayerOwner.RequestJoinTeam(Team.Spectator);
            HideAllElements();
        });
        
        quitGameButton.onClick.AddListener(() => {
            ConsoleCommandController.ExecuteCommand("quit");
            HideAllElements();
        });
    }

    private void HideAllElements() {
        HideMenu();
        UndeployConsole();
        HideLimbo();
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

        if (_inputActions.Player.PlayerStats.IsPressed()) {
            if (!_showingStats) {
                _showingStats = true;
                ShowStats();
            }
        }
        else {
            if (_showingStats) {
                _showingStats = false;
                HideStats();
            }
        }
    }

    private void HideStats() {
        //hud.GetComponent<CanvasRenderer>().SetAlpha(0f);
        hud.gameObject.SetActive(true);
        playerStats.gameObject.SetActive(false);
        //playerStats.GetComponent<Image>().CrossFadeAlpha(1f, 1.0f, false);
    }

    private void ShowStats() {
        hud.gameObject.SetActive(false);
        playerStats.gameObject.SetActive(true);
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
        LockCursor();
    }

    private void DeployConsole() {
        _consoleDeployed = true;
        _consoleEndMoving = Time.time + consoleDeployTime;
        _consoleMoving = true;
        console.gameObject.SetActive(true);
        UnlockCursor();
        FocusConsole();
    }

    private void FocusConsole() {
        consoleInput.Select();
        consoleInput.ActivateInputField();
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