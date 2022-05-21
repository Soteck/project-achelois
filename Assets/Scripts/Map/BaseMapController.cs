using System;
using System.Collections.Generic;
using CharacterController;
using Config;
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
    public abstract class BaseMapController<T> : NetworkSingleton<T>, IBaseMapController where T : Component {
        //Init data
        public int warmUpDuration = 45;
        public int duration = 5 * 60;
        public int teamARespawn = 5;
        public int teamBRespawn = 6;

        //Input data
        public Camera mapCamera;
        public GameObject controlablePlayerPrefab;
        public GameObject spectatorPrefab;

        //Variables sync between clients and server
        private readonly NetworkVariable<int> _networkPlayersInGame = new NetworkVariable<int>();
        private readonly NetworkVariable<int> _networkMapDuration = new NetworkVariable<int>();
        private readonly NetworkVariable<int> _networkWarmUpDuration = new NetworkVariable<int>();
        private readonly NetworkVariable<float> _networkTimeElapsed = new NetworkVariable<float>();
        private readonly NetworkVariable<int> _networkTeamARespawnCount = new NetworkVariable<int>();
        private readonly NetworkVariable<int> _networkTeamBRespawnCount = new NetworkVariable<int>();
        
        private readonly NetworkVariable<MapState> _networkMapState = new NetworkVariable<MapState>();


        //This variables are on the server, but not on the client
        private Dictionary<ulong, NetworkPlayer> _allPlayers;
        private List<ulong> _teamAPlayers;
        private List<ulong> _teamBPlayers;
        protected bool serverInit = false;


        private void ServerInit() {
            if (!serverInit) {
                //Init server variables
                _networkMapDuration.Value = duration;
                _networkWarmUpDuration.Value = warmUpDuration;
                _networkTimeElapsed.Value = 0;
                _networkTeamARespawnCount.Value = 0;
                _networkTeamBRespawnCount.Value = 0;
                _allPlayers = new Dictionary<ulong, NetworkPlayer>();
                _teamAPlayers = new List<ulong>();
                _teamBPlayers = new List<ulong>();
                serverInit = true;
                _networkMapState.Value = MapState.Warmup;
            }
        }

        protected void Update() {
            if (IsServer && serverInit) {
                _networkTimeElapsed.Value += Time.deltaTime;
                if (_networkMapState.Value == MapState.Warmup) {
                    if (_networkTimeElapsed.Value > _networkWarmUpDuration.Value) {
                        _networkTimeElapsed.Value = 0;
                        _networkMapState.Value = MapState.Match;
                        _networkTeamARespawnCount.Value = 0;
                        _networkTeamBRespawnCount.Value = 0;
                        DespawnAllPlayers();
                        ServerRespawnTeamA();
                        ServerRespawnTeamB();
                    } else {
                        ServerRespawnTeamA();
                        ServerRespawnTeamB();
                    }
                }

                if (_networkMapState.Value == MapState.Match) {

                    if (_networkTimeElapsed.Value > _networkMapDuration.Value) {
                        int result = GetWinningTeam();
                        if (result == 0) {
                            _networkMapState.Value = MapState.Tie;
                        }else if (result < 0) {
                            _networkMapState.Value = MapState.WinA;
                        } else {
                            _networkMapState.Value = MapState.WinB;
                        }
                        
                    } else {
                        if (_networkTimeElapsed.Value / teamARespawn > _networkTeamARespawnCount.Value) {
                            ServerRespawnTeamA();
                        }

                        if (_networkTimeElapsed.Value / teamBRespawn > _networkTeamBRespawnCount.Value) {
                            ServerRespawnTeamB();
                        }
                    }

                }

            }
        }

        private void DespawnAllPlayers() {
            foreach (ulong teamAPlayerId in _teamAPlayers) {
                DespawnPlayer(teamAPlayerId);
            }

            foreach (ulong teamBPlayerId in _teamBPlayers) {
                DespawnPlayer(teamBPlayerId);
            }
        }

        private void DespawnPlayer(ulong playerId) {
            _allPlayers[playerId].networkState.Value = PlayerState.Spectating;
            _allPlayers[playerId].fpsController.soldier.networkObject.Despawn();
        }

        private void ServerRespawnTeamA() {
            Logger.Info("Respawning team A");
            _networkTeamARespawnCount.Value++;
            int teamSpawningNumber = 0;
            foreach (ulong playerId in _teamAPlayers) {
                NetworkPlayer player = _allPlayers[playerId];
                if (player.networkState.Value == PlayerState.PlayingDead) {
                    ServerSpawnControllablePlayer(playerId, player, teamSpawningNumber++);
                }
            }
        }

        private void ServerRespawnTeamB() {
            Logger.Info("Respawning team B");
            _networkTeamBRespawnCount.Value++;
            int teamSpawningNumber = 0;
            foreach (ulong playerId in _teamBPlayers) {
                NetworkPlayer player = _allPlayers[playerId];
                if (player.networkState.Value == PlayerState.PlayingDead) {
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
                    _networkPlayersInGame.Value--;
                    _allPlayers[id] = null;
                    _teamAPlayers.Remove(id);
                    _teamBPlayers.Remove(id);
                };
            };
        }

        private void ServerAddConnectedClient(ulong playerId) {
            _networkPlayersInGame.Value++;
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

                NetPlayerController controller = NetworkUtil.FindNetPlayerControllerByOwnerId(playerId);
                if (controller) {
                    NetworkObject no = controller.GetComponent<NetworkObject>();
                    no.Despawn();
                }
            }
        }

        public int PlayersInGame() {
            return _networkPlayersInGame.Value;
        }

        public float TimeElapsed() {
            return _networkTimeElapsed.Value;
        }

        public float MapDuration() {
            return _networkMapDuration.Value;
        }

        public float WarmupDuration() {
            return _networkWarmUpDuration.Value;
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

        public abstract int GetWinningTeam();
        
        public MapState GetMapState() {
            return _networkMapState.Value;
        }
    }
}