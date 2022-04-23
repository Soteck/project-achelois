﻿using System;
using Config;
using Controller;
using Enums;
using Network.Shared;
using Player;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Util;
using Random = UnityEngine.Random;

namespace Network {
    public class NetworkPlayer : NetworkBehaviour {
        public NetworkVariable<PlayerNetworkData> networkData = new NetworkVariable<PlayerNetworkData>();
        public NetworkVariable<bool> initialized = new NetworkVariable<bool>();
        public NetworkVariable<Guid> selectedSpawnPoint = new NetworkVariable<Guid>();


        public NetworkVariable<Team> team = new NetworkVariable<Team>();
        public NetworkVariable<PlayerSate> state = new NetworkVariable<PlayerSate>();
        public NetworkVariable<ulong> networkFollowing = new NetworkVariable<ulong>();


        private Team activeTeam = Team.Spectator;
        private PlayerSate activeState = PlayerSate.Disconnected;

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

            if (activeState == PlayerSate.MapCamera && jumpTriggered) {
                RequestStandaloneSpectatorServerRpc();
            }
            //Fire 1 -> spec next player // Fire2 -> previous
            //Jump -> Go to generic spectator
        }


        private void ClientVisuals() {
            if (activeState != state.Value) {
                bool success = false;
                switch (state.Value) {
                    case PlayerSate.MapCamera:
                        DisableSpectator();
                        success = FollowMapCamera();
                        break;
                    case PlayerSate.Following:
                    case PlayerSate.PlayingDead:
                        DisableSpectator();
                        success = FollowPlayer(networkFollowing.Value);
                        break;
                    case PlayerSate.PlayingAlive:
                        DisableSpectator();
                        success = AttachSoldier();
                        break;
                    case PlayerSate.Spectating:
                        success = EnableSpectator();
                        break;
                }

                if (success) {
                    activeState = state.Value;
                }
            }

            if (activeTeam != team.Value) {
                activeTeam = team.Value;
                HudController.ChangeTeam(activeTeam);
            }

            if (currentFollowing != networkFollowing.Value) {
                currentFollowing = networkFollowing.Value;
                FollowPlayer(currentFollowing);
            }
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
            spectatorController.playerCamera.enabled = true;
            CameraUtil.DisableAllCameras(spectatorController.playerCamera);
            return true;
        }

        private bool AttachSpectator() {
            spectatorController = NetSpectatorController.FindByOwnerId(NetworkManager.Singleton.LocalClientId);
            if (spectatorController != null) {
                Quaternion mapCameraRotation = MapController.MapCamera().transform.rotation;
                Vector3 angles = mapCameraRotation.eulerAngles;
                angles.z = 0f;
                spectatorController.playerCamera.transform.localRotation = Quaternion.Euler(angles);
                return true;
            }

            return false;
        }

        private bool AttachSoldier() {
            fpsController = NetPlayerController.FindByOwnerId(NetworkManager.Singleton.LocalClientId);
            if (fpsController != null) {
                spectatorController?.Disable();
                fpsController.Enable();
                fpsController.soldier.Enable();
                fpsController.playerCamera.enabled = true;
                CameraUtil.DisableAllCameras(fpsController.playerCamera);
                return true;
            }

            return false;
        }

        private bool FollowPlayer(ulong networkFollowingValue) {
            NetPlayerController playerController =
                NetPlayerController.FindByOwnerId(NetworkManager.Singleton.LocalClientId);

            if (playerController) {
                PlayableSoldier soldier = playerController.soldier;
                fpsController.playerCamera.enabled = true;
                CameraUtil.DisableAllCameras(soldier.playerCamera);
                return true;
            }

            return false;
        }

        private bool FollowMapCamera() {
            MapController.MapCamera().enabled = true;
            CameraUtil.DisableAllCameras(MapController.MapCamera());
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
            MapController.ServerRequestJoinTeam(teamToJoin, playerId);
        }

        [ServerRpc]
        private void RequestStandaloneSpectatorServerRpc() {
            if (team.Value == Team.Spectator) {
                state.Value = PlayerSate.Spectating;
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
    }
}