using System;
using CharacterController;
using Config;
using Controller;
using Enums;
using Map;
using Network.Shared;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

namespace Network {
    public class NetworkPlayer : NetworkBehaviour {
        public NetworkVariable<PlayerNetworkData> networkData = new NetworkVariable<PlayerNetworkData>();
        public NetworkVariable<bool> initialized = new NetworkVariable<bool>();
        public NetworkVariable<Guid> selectedSpawnPoint = new NetworkVariable<Guid>();


        public NetworkVariable<Team> networkTeam = new NetworkVariable<Team>();
        public NetworkVariable<PlayerState> networkState = new NetworkVariable<PlayerState>();
        public NetworkVariable<ulong> networkFollowing = new NetworkVariable<ulong>();


        private Team activeTeam = Team.Spectator;
        private PlayerState activeState = PlayerState.Disconnected;

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
            }
            else {
                if (IsSpawned) {
                    if (IsClient && IsOwner) {
                        ClientInput();
                        ClientVisuals();
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

            if (activeState == PlayerState.MapCamera && jumpTriggered) {
                RequestStandaloneSpectatorServerRpc();
            }
            //Fire 1 -> spec next player // Fire2 -> previous
            //Jump -> Go to generic spectator
        }


        private void ClientVisuals() {
            if (activeState != networkState.Value) {
                bool success = false;
                switch (networkState.Value) {
                    case PlayerState.MapCamera:
                        DisableSpectator();
                        success = FollowMapCamera();
                        break;
                    case PlayerState.Following:
                    case PlayerState.PlayingDead:
                        DisableSpectator();
                        success = FollowPlayer(networkFollowing.Value);
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
                    activeState = networkState.Value;
                }
            }

            if (activeTeam != networkTeam.Value) {
                activeTeam = networkTeam.Value;
                HudController.ChangeTeam(activeTeam);
            }

            // if (currentFollowing != networkFollowing.Value) {
            //     currentFollowing = networkFollowing.Value;
            //     FollowPlayer(currentFollowing);
            // }
        }

        private bool EnableSpectator() {
            bool res = false;
            if (spectatorController == null) {
                res = AttachSpectator();
            }
            else {
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
            spectatorController = NetworkUtil.FindNetSpectatorControllerByOwnerId(NetworkManager.Singleton.LocalClientId);
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
                return true;
            }

            return false;
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


        public void RequestJoinTeam(Team teamToJoin) {
            RequestJoinTeamServerRpc(teamToJoin, NetworkManager.Singleton.LocalClientId);
        }


        //Server RPC methods
        [ServerRpc]
        private void RequestJoinTeamServerRpc(Team teamToJoin, ulong playerId) {
            MapMaster.MapInstance().ServerRequestJoinTeam(teamToJoin, playerId);
        }

        [ServerRpc]
        private void RequestStandaloneSpectatorServerRpc() {
            if (networkTeam.Value == Team.Spectator) {
                networkState.Value = PlayerState.Spectating;
            }
        }

        [ServerRpc]
        private void SaveNetworkDataServerRpc(PlayerNetworkData data) {
            initialized.Value = true;
            networkData.Value = data;
        }

        //Public accessors
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