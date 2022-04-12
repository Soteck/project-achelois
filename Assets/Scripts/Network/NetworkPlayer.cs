using System;
using Config;
using Controller;
using Network.Shared;
using Player;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

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
        public static NetworkPlayer networkPlayerOwner;
        public PlayableSoldier currentSoldier;


        public new void Awake() {
            base.Awake();
            if (IsClient && IsOwner) {
                networkPlayerOwner = this;
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
        
        void Start() {
            if (IsClient && IsOwner) {
                PlayerNetworkData data = new PlayerNetworkData();
                data.playerName = ConfigHolder.playerName;
                data.clientId = NetworkManager.Singleton.LocalClientId;
                SaveNetworkDataServerRpc(data);
            }
        }

        

        [ServerRpc]
        private void SaveNetworkDataServerRpc(PlayerNetworkData data) {
            networkData.Value = data;
        }
    }
}