using System.Collections.Generic;
using Config;
using Controller;
using Core;
using Enums;
using Map;
using Player;
using Unity.Netcode;
using UnityEngine;
using Logger = Core.Logger;
using NetworkPlayer = Network.NetworkPlayer;

public class MapController : NetworkSingleton<MapController> {
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
    private bool serverInit = false;


    private void ServerInit() {
        if (!serverInit) {
            //Init server variables
            mapDuration.Value = duration;
            timeElapsed.Value = 0;
            team_a_respawn_count.Value = 0;
            team_b_respawn_count.Value = 0;
            _allPlayers = new Dictionary<ulong, NetworkPlayer>();
            _teamAPlayers = new List<ulong>();
            _teamBPlayers = new List<ulong>();
            serverInit = true;
        }
    }

    private void Update() {
        if (IsServer && serverInit) {
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
            var player = _allPlayers[playerId];
            if (player.state.Value != PlayerSate.PlayingAlive) {
                ServerSpawnControlablePlayer(playerId, player, teamSpawningNumber++);
            }
        }
    }

    private void ServerRespawnTeamB() {
        Logger.Info("Respawning team B");
        team_b_respawn_count.Value++;
        int teamSpawningNumber = 0;
        foreach (ulong playerId in _teamBPlayers) {
            var player = _allPlayers[playerId];
            if (player.state.Value != PlayerSate.PlayingAlive) {
                ServerSpawnControlablePlayer(playerId, player, teamSpawningNumber++);
            }
        }
    }

    private void ServerSpawnControlablePlayer(ulong playerId, NetworkPlayer player, int number) {
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
        player.activeCamera = go.GetComponent<NetFirstPersonController>().playerCamera;
        PlayableSoldier po = go.GetComponent<PlayableSoldier>();
        player.currentSoldier = po;
        po.networkHealth.Value = 100f;
        DisableAllCameras(player.activeCamera);
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

    private void ServerAddConnectedClient(ulong id) {
        playersInGame.Value++;
        NetworkPlayer playerObject = NetworkManager.Singleton.ConnectedClients[id].PlayerObject
            .GetComponent<NetworkPlayer>();
        _allPlayers[id] = playerObject;
    }

    [ServerRpc]
    private void RequestJoinServerRpc(Team team, ulong playerId) {
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
            }
        }
    }

    public static int PlayersInGame() {
        return Instance.playersInGame.Value;
    }

    public static float TimeElapsed() {
        return Instance.timeElapsed.Value;
    }

    public static void RequestJoinTeam(Team team, ulong playerId) {
        Instance.RequestJoinServerRpc(team, playerId);
    }

    public static void RequestJoinTeam(Team team) {
        Instance.RequestJoinServerRpc(team, NetworkManager.Singleton.LocalClientId);
    }

    public static void FollowPlayer(ulong playerId) {
        NetworkPlayer player = Instance._allPlayers[playerId];
        if (player != null) {
            DisableAllCameras(player.activeCamera);
        }
        else {
            Logger.Warning("Error following player, player not found " + playerId);
        }
    }

    public static void MapCamera() {
        DisableAllCameras(Instance.mapCamera);
    }

    public static void DisableAllCameras(Camera dontDisable) {
        //Camera[] cameras = FindObjectsOfType(typeof(Camera)) as Camera[];
        Camera[] cameras = Camera.allCameras;
        foreach (Camera camera in cameras) {
            if (dontDisable != null) {
                if (camera == dontDisable) {
                    camera.enabled = true;
                }
                else {
                    camera.enabled = false;
                }
            }
            else {
                camera.enabled = false;
            }
        }
    }
}