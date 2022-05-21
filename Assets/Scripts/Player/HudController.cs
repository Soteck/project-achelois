using System;
using System.Collections.Generic;
using System.Linq;
using CharacterController;
using Controller;
using Core;
using Enums;
using Map;
using Map.Maps;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using NetworkPlayer = Network.NetworkPlayer;

namespace Player {
    public class HudController : Singleton<HudController> {
        public TextMeshProUGUI teamTxt;
        public TextMeshProUGUI spawnTimeTxt;
        public TextMeshProUGUI mapTimeTxt;

        [Space(15)]
        public TextMeshProUGUI ammoTxt;

        public RectTransform ammoInfo;


        [Space(15)]
        public TextMeshProUGUI healthTxt;

        public RectTransform healthInfo;

        [Space(15)]
        public TextMeshProUGUI statusInfoTxt;

        public RectTransform statusInfo;


        [Space(15)]
        public TextMeshProUGUI teamAPlayers;

        public TextMeshProUGUI teamBPlayers;
        public TextMeshProUGUI spectatorPlayers;

        private Team _drawingTeam;

        private void Update() {
            teamTxt.SetText(GetTeamTxt());
            mapTimeTxt.SetText(getMapText());
            spawnTimeTxt.SetText(getRespawnsText());

            string ammoText = getAmmoText();
            if (ammoText != null) {
                ammoInfo.gameObject.SetActive(true);
                ammoTxt.SetText(ammoText);
            } else {
                ammoInfo.gameObject.SetActive(false);
            }

            string healthText = getHealthText();
            if (healthText != null) {
                healthInfo.gameObject.SetActive(true);
                healthTxt.SetText(healthText);
            } else {
                healthInfo.gameObject.SetActive(false);
            }

            string statusTxt = getStatusTxt();
            if (statusTxt != null) {
                statusInfo.gameObject.SetActive(true);
                statusInfoTxt.SetText(statusTxt);
            } else {
                statusInfo.gameObject.SetActive(false);
            }
        }

        private string getRespawnsText() {
            float time = MapMaster.MapInstance().TimeElapsed();

            TimeSpan teamARespawnSpan;
            TimeSpan teamBRespawnSpan;

            int teamARespawn = MapMaster.MapInstance().TeamARespawn();
            int teamBRespawn = MapMaster.MapInstance().TeamBRespawn();

            if (time > 0) {
                teamARespawnSpan = TimeSpan.FromSeconds(teamARespawn - (time % teamARespawn));
                teamBRespawnSpan = TimeSpan.FromSeconds(teamBRespawn - (time % teamBRespawn));
            }

            return teamARespawnSpan.Seconds + "/" + teamBRespawnSpan.Seconds;
        }

        private string getMapText() {
            IBaseMapController mapInstance = MapMaster.MapInstance();
            MapState mapState = mapInstance.GetMapState();
            float totalDuration = mapInstance.MapDuration();
            float warmupDuration = mapInstance.WarmupDuration();
            float timeElapsed = MapMaster.MapInstance().TimeElapsed();
            float remaining;
            if (mapState == MapState.Warmup) {
                remaining = warmupDuration - timeElapsed;
            }else if (mapState == MapState.Match) {
                remaining = totalDuration - timeElapsed;
            } else {
                return null;
            }
            TimeSpan timeSpan = TimeSpan.FromSeconds(remaining);
            return timeSpan.Minutes + ":" + timeSpan.Seconds;
        }

        private string GetTeamTxt() {
            switch (_drawingTeam) {
                case Team.Spectator:
                    return "Spectator";
                case Team.TeamA:
                    return "Team A";
                case Team.TeamB:
                    return "Team B";
            }

            return "Unknown";
        }


        private string getAmmoText() {
            if (Network.NetworkPlayer.networkPlayerOwner != null) {
                NetPlayerController netFirstPersonController = Network.NetworkPlayer.networkPlayerOwner.fpsController;
                if (netFirstPersonController) {
                    PlayableSoldier playableSoldier = netFirstPersonController.soldier;
                    EquipableItemLogic activeItem = playableSoldier.ActiveItem();
                    if (activeItem) {
                        return activeItem.GetStatus();
                    }
                }
            }

            return null;
        }


        private string getStatusTxt() {
            List<string> data = new List<string>();
            if (NetworkPlayer.networkPlayerOwner != null) {
                NetworkPlayer networkPlayer = NetworkPlayer.networkPlayerOwner;
                if (networkPlayer.networkTeam.Value == Team.Spectator) {
                    data.Add("You're an spectator, press [L] to select a team to Join.");
                }

                if (networkPlayer.fpsController != null) {
                    NetPlayerController fpsController = networkPlayer.fpsController;
                    if (fpsController.soldier != null) {
                        PlayableSoldier playableSoldier = fpsController.soldier;
                        if (playableSoldier.IsKnockedDown()) {
                            data.Add( "You're knocked down. You can wait to be revived or press [Space] to respawn.");
                        }
                    }
                }

                if (
                    networkPlayer.networkState.Value == PlayerState.Following ||
                    networkPlayer.networkState.Value == PlayerState.PlayingDead
                ) {
                    //TODO: Add relevant info like player name instead of it's ID
                    data.Add( "You're spectating a team mate (with id " + networkPlayer.networkFollowing.Value + ")");
                }
            }

            IBaseMapController mapInstance = MapMaster.MapInstance();
            if (mapInstance != null) {
                MapState mapState = mapInstance.GetMapState();
                if(mapState == MapState.Warmup) {
                    float remaining = mapInstance.WarmupDuration() - mapInstance.TimeElapsed();
                    TimeSpan timeSpan = TimeSpan.FromSeconds(remaining);
                    data.Add("WarmUP!!! Waiting " + timeSpan.Seconds + "s to start the map");
                }else if (mapState == MapState.Tie) {
                    data.Add("Map ended with a TIE!");
                }else if (mapState == MapState.WinA) {
                    data.Add("Map ended, winning team A!");
                }else if (mapState == MapState.WinB) {
                    data.Add("Map ended, winning team B!");
                }
            }

            if (data.Count > 0) {
                return string.Join("\n", data);
            }
            return null;
        }

        private string getHealthText() {
            if (NetworkPlayer.networkPlayerOwner != null) {
                NetPlayerController netFirstPersonController = NetworkPlayer.networkPlayerOwner.fpsController;
                if (netFirstPersonController) {
                    PlayableSoldier playableSoldier = netFirstPersonController.soldier;
                    return playableSoldier.networkHealth.Value + "";
                }
            }

            return null;
        }

        public static void ChangeTeam(Team team) {
            Instance._drawingTeam = team;
        }
    }
}