using System.Collections.Generic;
using Config;
using Controller;
using Core;
using Enums;
using Player;
using Unity.Netcode;
using UnityEngine;
using Logger = Core.Logger;
using NetworkPlayer = Network.NetworkPlayer;

namespace Map {
    public class MapController<T> : NetworkSingleton<T> where T : Component {
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
        private bool _serverInit = false;


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
                if (player.state.Value != PlayerSate.PlayingAlive) {
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
                if (player.state.Value != PlayerSate.PlayingAlive) {
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
            player.state.Value = PlayerSate.PlayingAlive;
            PlayableSoldier po = go.GetComponent<PlayableSoldier>();
            po.networkHealth.Value = 100f;
            // DisableAllCameras(player.activeCamera);
        }

        void Start() {
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

            GameObject spectator = Instantiate(spectatorPrefab, mapCamera.transform.position, Quaternion.identity);
            NetworkObject no = spectator.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(playerId);
            playerObject.spectatorController = spectator.GetComponent<NetSpectatorController>();

            playerObject.state.Value = PlayerSate.MapCamera;
        }

        private void DoServerRequestJoinTeam(Team team, ulong playerId) {
            NetworkPlayer player = this._allPlayers[playerId];
            Team from = Team.Spectator;
            bool canJoin = false;
            //Check if the change can be done
            if (player != null) {
                from = player.team.Value;
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
                    player.team.Value = Team.TeamA;
                    player.state.Value = PlayerSate.PlayingDead;
                    player.selectedSpawnPoint.Value = SpawnArea.GetDefaultTeamASpawnArea();
                }
                else if (team == Team.TeamB) {
                    _teamBPlayers.Add(playerId);
                    player.team.Value = Team.TeamB;
                    player.state.Value = PlayerSate.PlayingDead;
                    player.selectedSpawnPoint.Value = SpawnArea.GetDefaultTeamBSpawnArea();
                }
                else {
                    player.team.Value = Team.Spectator;
                    player.state.Value = PlayerSate.MapCamera;
                }

                NetPlayerController controller = NetPlayerController.FindByOwnerId(playerId);
                if (controller) {
                    NetworkObject no = controller.GetComponent<NetworkObject>();
                    no.Despawn();
                }
            }
        }


    }
}