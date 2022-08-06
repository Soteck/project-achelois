using System;
using CharacterController;
using Config;
using Enums;
using Map;
using Network.Shared;
using Player;
using Unity.Netcode;
using UnityEngine;
using Util;
using Logger = Core.Logger;

namespace Network {
    public class NetworkPlayer : NetworkBehaviour {
        public NetworkVariable<PlayerNetworkData> networkData = new NetworkVariable<PlayerNetworkData>();
        public NetworkVariable<bool> initialized = new NetworkVariable<bool>();
        public NetworkVariable<Guid> selectedSpawnPoint = new NetworkVariable<Guid>();


        private readonly NetworkVariable<GameTeam> _networkTeam = new NetworkVariable<GameTeam>();
        private readonly NetworkVariable<PlayerState> _networkState = new NetworkVariable<PlayerState>();
        private readonly NetworkVariable<ulong> _networkFollowing = new NetworkVariable<ulong>();


        private GameTeam _activeGameTeam = GameTeam.Spectator;

        private ulong currentFollowing;
        public Camera activeCamera;
        public NetPlayerController fpsController;
        public NetSpectatorController spectatorController;

        private PlayerInputActions _inputActions;

        public void Awake() {
            _inputActions = new PlayerInputActions();
            _inputActions.Player.Enable();
        }


        void Update() {
            if (!initialized.Value) {
                InitializeConfig();
            } else {
                if (IsSpawned) {
                    if (IsClient && IsOwner) {
                        ClientInput();
                    }
                }
            }
        }

        private void InitializeConfig() {
            if (IsOwner) {
                PlayerNetworkData data = new PlayerNetworkData();
                data.playerName = ConfigHolder.playerName;
                data.clientId = NetworkManager.Singleton.LocalClientId;
                SaveNetworkDataServerRpc(data);
            }
        }

        private void ClientInput() {
            bool fire1Triggered = _inputActions.Player.Fire1.WasPerformedThisFrame();
            bool fire2Triggered = _inputActions.Player.Fire2.WasPerformedThisFrame();
            bool jumpTriggered = _inputActions.Player.Jump.WasPerformedThisFrame();

            if (_networkState.Value == PlayerState.MapCamera && jumpTriggered) {
                RequestStandaloneSpectatorServerRpc();
            }
            //Fire 1 -> spec next player // Fire2 -> previous
            //Jump -> Go to generic spectator
        }


        private bool EnableSpectator() {
            bool res = false;
            if (spectatorController == null) {
                res = AttachSpectator();
            } else {
                res = true;
            }

            if (!res) {
                return false;
            }

            spectatorController.Enable();
            activeCamera = spectatorController.playerCamera;
            activeCamera.enabled = true;
            CameraUtil.DisableAllCameras(activeCamera);
            return true;
        }

        private bool AttachSpectator() {
            spectatorController =
                NetworkUtil.FindNetSpectatorControllerByOwnerId(NetworkManager.Singleton.LocalClientId);
            if (spectatorController != null) {
                Quaternion mapCameraRotation = MapMaster.MapInstance().MapCamera().transform.rotation;
                Vector3 angles = mapCameraRotation.eulerAngles;
                angles.z = 0f;
                spectatorController.playerCamera.transform.localRotation = Quaternion.Euler(angles);
                return true;
            }

            return false;
        }

        private bool AttachSoldier() {
            fpsController = NetworkUtil.FindNetPlayerControllerByOwnerId(NetworkManager.Singleton.LocalClientId);
            if (fpsController != null) {
                spectatorController?.Disable();
                fpsController.Enable();
                fpsController.soldier.Enable();
                activeCamera = fpsController.playerCamera;
                activeCamera.enabled = true;
                fpsController.networkPlayer = this;
                CameraUtil.DisableAllCameras(activeCamera);
                return true;
            }

            return false;
        }

        private bool FollowPlayer(ulong networkFollowingValue) {
            NetPlayerController playerController =
                NetworkUtil.FindNetPlayerControllerByOwnerId(networkFollowingValue);

            if (playerController) {
                PlayableSoldier soldier = playerController.soldier;
                activeCamera = soldier.playerCamera;
                activeCamera.enabled = true;
                CameraUtil.DisableAllCameras(activeCamera);
            }

            return true;
        }

        private bool FollowMapCamera() {
            MapMaster.MapInstance().MapCamera().enabled = true;
            activeCamera = MapMaster.MapInstance().MapCamera();
            CameraUtil.DisableAllCameras(activeCamera);
            return true;
        }

        private void DisableSpectator() {
            if (spectatorController != null) {
                spectatorController.Disable();
            }
        }


        public void RequestJoinTeam(GameTeam gameTeamToJoin, GameRole role) {
            RequestJoinTeamServerRpc(gameTeamToJoin, NetworkManager.Singleton.LocalClientId);
        }

        public void ServerNotifyStateChange(PlayerState state) {
            NetworkStateChangedClientRpc(state);
        }

        public void ServerNotifyTeamChange(GameTeam gameTeam) {
            _networkTeam.Value = gameTeam;
        }

        //Client RPC methods
        [ClientRpc]
        private void NetworkStateChangedClientRpc(PlayerState state) {
            if (IsOwner && _networkState.Value != state) {
                bool success = false;
                switch (state) {
                    case PlayerState.MapCamera:
                        DisableSpectator();
                        success = FollowMapCamera();
                        break;
                    case PlayerState.Following:
                    case PlayerState.PlayingDead:
                        DisableSpectator();
                        success = FollowPlayer(_networkFollowing.Value);
                        break;
                    case PlayerState.PlayingAlive:
                        DisableSpectator();
                        success = AttachSoldier();
                        break;
                    case PlayerState.Spectating:
                        success = EnableSpectator();
                        break;
                }

                if (success) {
                    CommitStateChangeServerRpc(state);
                } else {
                    Logger.Error("Unable to change state to " + state);
                }
            }
        }


        //Server RPC methods
        [ServerRpc]
        private void RequestJoinTeamServerRpc(GameTeam gameTeamToJoin, ulong playerId) {
            MapMaster.MapInstance().ServerRequestJoinTeam(gameTeamToJoin, playerId);
        }

        [ServerRpc]
        private void RequestStandaloneSpectatorServerRpc() {
            if (_networkTeam.Value == GameTeam.Spectator) {
                ServerNotifyStateChange(PlayerState.Spectating);
            }
        }

        [ServerRpc]
        private void SaveNetworkDataServerRpc(PlayerNetworkData data) {
            initialized.Value = true;
            networkData.Value = data;
        }

        [ServerRpc]
        private void CommitStateChangeServerRpc(PlayerState state) {
            _networkState.Value = state;
        }

        //Public accessors

        public PlayerState GetPlayerState() {
            return _networkState.Value;
        }

        public GameTeam GetNetworkTeam() {
            return _networkTeam.Value;
        }

        public ulong GetNetworkFollowing() {
            return _networkFollowing.Value;
        }

        public static NetworkPlayer networkPlayerOwner {
            get {
                if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClient != null) {
                    return NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>();
                }

                return null;
            }
        }

        public static NetworkPlayer NetworkPlayerByControllerId(ulong playerId) {
            return NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject.GetComponent<NetworkPlayer>();
        }
    }
}