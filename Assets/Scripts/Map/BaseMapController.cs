using System.Collections.Generic;
using CharacterController;
using Config;
using Controller;
using Core;
using Enums;
using Map.Maps;
using Player;
using Unity.Netcode;
using UnityEngine;
using Util;
using Logger = Core.Logger;
using NetworkPlayer = Network.NetworkPlayer;

namespace Map {
    public class BaseMapController<T> : NetworkSingleton<T>, BaseMapControllerInterface where T : Component {
        //Init data
        public int duration = 15 * 60;
        public int teamARespawn = 5;
        public int teamBRespawn = 6;

        //Input data
        public Camera mapCamera;
        public GameObject controlablePlayerPrefab;
        public GameObject spectatorPrefab;

        //Variables sync between clients and server
        private NetworkVariable<int> playersInGame = new NetworkVariable<int>();
        private NetworkVariable<int> mapDuration = new NetworkVariable<int>();
        private NetworkVariable<float> timeElapsed = new NetworkVariable<float>();
        private NetworkVariable<int> team_a_respawn_count = new NetworkVariable<int>();
        private NetworkVariable<int> team_b_respawn_count = new NetworkVariable<int>();


        //This variables are on the server, but not on the client
        private Dictionary<ulong, NetworkPlayer> _allPlayers;
        private List<ulong> _teamAPlayers;
        private List<ulong> _teamBPlayers;
        protected bool _serverInit = false;


        private void ServerInit() {
            if (!_serverInit) {
                //Init server variables
                mapDuration.Value = duration;
                timeElapsed.Value = 0;
                team_a_respawn_count.Value = 0;
                team_b_respawn_count.Value = 0;
                _allPlayers = new Dictionary<ulong, NetworkPlayer>();
                _teamAPlayers = new List<ulong>();
                _teamBPlayers = new List<ulong>();
                _serverInit = true;
            }
        }

        protected void Update() {
            if (IsServer && _serverInit) {
                timeElapsed.Value += Time.deltaTime;
                if (timeElapsed.Value / teamARespawn > team_a_respawn_count.Value) {
                    ServerRespawnTeamA();
                }

                if (timeElapsed.Value / teamBRespawn > team_b_respawn_count.Value) {
                    ServerRespawnTeamB();
                }
            }
        }

        private void ServerRespawnTeamA() {
            Logger.Info("Respawning team A");
            team_a_respawn_count.Value++;
            int teamSpawningNumber = 0;
            foreach (ulong playerId in _teamAPlayers) {
                NetworkPlayer player = _allPlayers[playerId];
                if (player.networkState.Value != PlayerState.PlayingAlive) {
                    ServerSpawnControllablePlayer(playerId, player, teamSpawningNumber++);
                }
            }
        }

        private void ServerRespawnTeamB() {
            Logger.Info("Respawning team B");
            team_b_respawn_count.Value++;
            int teamSpawningNumber = 0;
            foreach (ulong playerId in _teamBPlayers) {
                NetworkPlayer player = _allPlayers[playerId];
                if (player.networkState.Value != PlayerState.PlayingAlive) {
                    ServerSpawnControllablePlayer(playerId, player, teamSpawningNumber++);
                }
            }
        }

        private void ServerSpawnControllablePlayer(ulong playerId, NetworkPlayer player, int number) {
            //GameObject go = NetworkObjectPool.Instance.GetNetworkObject(controlablePlayerPrefab).gameObject;
            Vector3 position = SpawnArea.GetSpawnPosition(player.selectedSpawnPoint.Value, number);
            GameObject go = Instantiate(controlablePlayerPrefab, position, Quaternion.identity);
            //go.transform.position = new Vector3(Random.Range(-10, 10), 10.0f, Random.Range(-10, 10));
            //go.transform.position = go.transform.TransformDirection(position);

            NetworkObject no = go.GetComponent<NetworkObject>();
            no.transform.position = position;
            no.SpawnWithOwnership(playerId);
            //no.ChangeOwnership(playerId);
            player.networkState.Value = PlayerState.PlayingAlive;
            PlayableSoldier po = go.GetComponent<PlayableSoldier>();
            po.networkHealth.Value = 100f;
            po.networkObjective.Value = Constants.OBJECTIVE_NONE;
            po.networkInMenu.Value = false;
            po.networkTexting.Value = false;
            // DisableAllCameras(player.activeCamera);
        }

        protected void Start() {
            NetworkManager.Singleton.OnServerStarted += () => {
                //NetworkObjectPool.Instance.InitializePool();
                ServerInit();
                ServerAddConnectedClient(NetworkManager.Singleton.LocalClientId);
                NetworkManager.Singleton.OnClientConnectedCallback += ServerAddConnectedClient;

                NetworkManager.Singleton.OnClientDisconnectCallback += (id) => {
                    playersInGame.Value--;
                    _allPlayers[id] = null;
                    _teamAPlayers.Remove(id);
                    _teamBPlayers.Remove(id);
                };
            };
        }

        private void ServerAddConnectedClient(ulong playerId) {
            playersInGame.Value++;
            NetworkPlayer playerObject = NetworkManager.Singleton.ConnectedClients[playerId].PlayerObject
                .GetComponent<NetworkPlayer>();
            _allPlayers[playerId] = playerObject;

            Transform cameraTransform = mapCamera.transform;
            Vector3 spectatorRotation = cameraTransform.rotation.eulerAngles;
            spectatorRotation.z = 0;
            spectatorRotation.x = 0;
            GameObject spectator = Instantiate(spectatorPrefab, cameraTransform.position, Quaternion.Euler(spectatorRotation));
            NetworkObject no = spectator.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(playerId);
            playerObject.spectatorController = spectator.GetComponent<NetSpectatorController>();

            playerObject.networkState.Value = PlayerState.MapCamera;
        }

        private void DoServerRequestJoinTeam(Team team, ulong playerId) {
            NetworkPlayer player = this._allPlayers[playerId];
            Team from = Team.Spectator;
            bool canJoin = false;
            //Check if the change can be done
            if (player != null) {
                from = player.networkTeam.Value;
                if (team != from) {
                    if (team == Team.Spectator || !ConfigHolder.autoBalance) {
                        canJoin = true;
                    }
                    else {
                        if (team == Team.TeamA) {
                            canJoin = _teamAPlayers.Count <= _teamBPlayers.Count;
                        }
                        else if (team == Team.TeamB) {
                            canJoin = _teamBPlayers.Count <= _teamAPlayers.Count;
                        }
                    }
                }
            }
            else {
                Logger.Error("Request to join over a non existing playerId: " + playerId);
            }

            if (canJoin) {
                //Remove information from de previous team
                if (from == Team.TeamA) {
                    _teamAPlayers.Remove(playerId);
                }
                else if (from == Team.TeamB) {
                    _teamBPlayers.Remove(playerId);
                }

                //Add player to the new team
                if (team == Team.TeamA) {
                    _teamAPlayers.Add(playerId);
                    player.networkTeam.Value = Team.TeamA;
                    player.networkState.Value = PlayerState.PlayingDead;
                    player.selectedSpawnPoint.Value = SpawnArea.GetDefaultTeamASpawnArea();
                }
                else if (team == Team.TeamB) {
                    _teamBPlayers.Add(playerId);
                    player.networkTeam.Value = Team.TeamB;
                    player.networkState.Value = PlayerState.PlayingDead;
                    player.selectedSpawnPoint.Value = SpawnArea.GetDefaultTeamBSpawnArea();
                }
                else {
                    player.networkTeam.Value = Team.Spectator;
                    player.networkState.Value = PlayerState.MapCamera;
                }

                NetPlayerController controller = NetPlayerController.FindByOwnerId(playerId);
                if (controller) {
                    NetworkObject no = controller.GetComponent<NetworkObject>();
                    no.Despawn();
                }
            }
        }

        public int PlayersInGame() {
            return playersInGame.Value;
        }

        public float TimeElapsed() {
            return timeElapsed.Value;
        }

        public void ServerRequestJoinTeam(Team team, ulong playerId) {
            DoServerRequestJoinTeam(team, playerId);
        }

        public NetworkPlayer GetPlayer(ulong playerId) {
            return _allPlayers[playerId];
        }

        public Camera MapCamera() {
            return mapCamera;
        }

        public int TeamARespawn() {
            return teamARespawn;
        }

        public int TeamBRespawn() {
            return teamBRespawn;
        }
    }
}