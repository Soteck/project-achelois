using System;
using System.Collections.Generic;
using Config;
using Controller;
using Core;
using Network.Shared;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Logger = Core.Logger;
using NetworkPlayer = Network.NetworkPlayer;

public class MapController : NetworkSingleton<MapController> {
    //Init data
    public int duration = 15 * 60;
    public int teamARespawn = 5;
    public int teamBRespawn = 6;
    
    //Input data
    public Camera mapCamera;

    //Variables sync between clients and server
    private NetworkVariable<int> playersInGame = new NetworkVariable<int>();
    private NetworkVariable<int> mapDuration = new NetworkVariable<int>();
    private NetworkVariable<float> timeElapsed = new NetworkVariable<float>();
    private NetworkVariable<int> team_a_respawn_count = new NetworkVariable<int>();
    private NetworkVariable<int> team_b_respawn_count = new NetworkVariable<int>();


    //This variables are on the server, but not on the client
    private Dictionary<ulong, Network.NetworkPlayer> all_players;
    private List<ulong> team_a_players;
    private List<ulong> team_b_players;


    private void Awake() {
        if (IsServer) {
            mapDuration.Value = duration;
            timeElapsed.Value = 0;
            team_a_respawn_count.Value = 0;
            team_b_respawn_count.Value = 0;
            //Init server variables
            all_players = new Dictionary<ulong, Network.NetworkPlayer>();
            team_a_players = new List<ulong>();
            team_b_players = new List<ulong>();
        }
    }

    private void Update() {
        if (IsServer) {
            timeElapsed.Value += Time.deltaTime;
            if (timeElapsed.Value / teamARespawn > team_a_respawn_count.Value) {
                RespawnTeamA();
            }

            if (timeElapsed.Value / teamBRespawn > team_b_respawn_count.Value) {
                RespawnTeamB();
            }
        }
    }

    private void RespawnTeamA() {
        Logger.Info("Respawning team A");
        team_a_respawn_count.Value++;
    }

    private void RespawnTeamB() {
        Logger.Info("Respawning team B");
        team_b_respawn_count.Value++;
    }

    void Start() {
        if (IsServer) {
            NetworkManager.Singleton.OnClientConnectedCallback += (id) => {
                playersInGame.Value++;
                NetworkPlayer playerObject = NetworkManager.Singleton.ConnectedClients[id].PlayerObject
                    .GetComponent<NetworkPlayer>();
                all_players[id] = playerObject;
            };

            NetworkManager.Singleton.OnClientDisconnectCallback += (id) => {
                playersInGame.Value--;
                all_players[id] = null;
                team_a_players.Remove(id);
                team_b_players.Remove(id);
            };
            
            NetworkManager.Singleton.OnServerStarted += () =>
            {
                NetworkObjectPool.Instance.InitializePool();
            };
        }
    }

    [ServerRpc]
    private void RequestJoinServerRpc(Team team, ulong playerId) {
        NetworkPlayer player = this.all_players[playerId];
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
                        canJoin = team_a_players.Count <= team_b_players.Count;
                    }
                    else if (team == Team.TeamB) {
                        canJoin = team_b_players.Count <= team_a_players.Count;
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
                team_a_players.Remove(playerId);
            }
            else if (from == Team.TeamB) {
                team_b_players.Remove(playerId);
            }
            //Add player to the new team
            if (team == Team.TeamA) {
                team_a_players.Add(playerId);
                player.team.Value = Team.TeamA;
                player.state.Value = PlayerSate.PlayingDead;
            }
            else if (team == Team.TeamB) {
                team_b_players.Add(playerId);
                player.team.Value = Team.TeamB;
                player.state.Value = PlayerSate.PlayingDead;
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
}