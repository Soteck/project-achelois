using System;
using Config;
using Controller;
using Enums;
using Network.Shared;
using Player;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Util;

namespace Network {
    public class NetworkPlayer : NetController {
        public NetworkVariable<Team> team = new NetworkVariable<Team>();
        public NetworkVariable<PlayerSate> state = new NetworkVariable<PlayerSate>();

        public NetworkVariable<PlayerNetworkData> networkData = new NetworkVariable<PlayerNetworkData>();
        public NetworkVariable<Guid> selectedSpawnPoint = new NetworkVariable<Guid>();

        private Team activeTeam = Team.Spectator;
        private PlayerSate activeState = PlayerSate.MapCamera;
        private ulong currentFollowing;

        public Camera activeCamera;
        public PlayableSoldier currentSoldier;


        public new void Awake() {
            base.Awake();
        }

        public override void OnGainedOwnership() {
            if (IsOwner) {
                PlayerNetworkData data = new PlayerNetworkData();
                data.playerName = ConfigHolder.playerName;
                data.clientId = NetworkManager.Singleton.LocalClientId;
                SaveNetworkDataServerRpc(data);
            }
        }

        protected override void ClientInput() {
            bool fire1Triggered = inputActions.Player.Fire1.ReadValue<float>() > 0f;
            bool fire2Triggered = inputActions.Player.Fire2.ReadValue<float>() > 0f;
            bool jumpTriggered = inputActions.Player.Jump.ReadValue<float>() > 0f;

            //Fire 1 -> spec next player // Fire2 -> previous
            //Jump -> Go to generic spectator
        }


        protected override void ClientVisuals() {
            if (activeState != state.Value) {
                //TODO
            }

            if (activeTeam != team.Value) {
                //TODO
            }
        }

        protected override void ServerCalculations() {
            //Empty
        }

        protected override void ClientBeforeInput() {
            //Empty
        }

        protected override void ClientMovement() {
            //Empty
        }

        public void RequestJoinTeam(Team teamToJoin) {
            this.RequestJoinTeamServerRpc(teamToJoin, NetworkManager.Singleton.LocalClientId);
        }

        [ServerRpc]
        private void RequestJoinTeamServerRpc(Team teamToJoin, ulong playerId) {
            MapController.ServerRequestJoinTeam(teamToJoin, playerId);
        }

        public static NetworkPlayer networkPlayerOwner {
            get {
                if (NetworkManager.Singleton.IsClient && NetworkManager.Singleton.LocalClient != null) {
                    return NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>();
                }

                return null;
            }
        }


        [ServerRpc]
        private void SaveNetworkDataServerRpc(PlayerNetworkData data) {
            networkData.Value = data;
        }
    }
}