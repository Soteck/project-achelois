using System;
using System.Collections.Generic;
using CharacterController;
using Core;
using Enums;
using Items;
using Map;
using Map.Maps;
using TMPro;
using UnityEngine;
using NetworkPlayer = Network.NetworkPlayer;

namespace Player {
    public class HudController : Singleton<HudController> {
        public TextMeshProUGUI mapTimeTxt;

        [Space(15)]
        public TextMeshProUGUI teamTxt;

        public RectTransform teamInfo;

        [Space(15)]
        public TextMeshProUGUI spawnTimeTxt;

        public RectTransform spawnTimeInfo;

        [Space(15)]
        public TextMeshProUGUI ammoTxt;

        public RectTransform ammoInfo;


        [Space(15)]
        public TextMeshProUGUI healthTxt;

        public RectTransform healthInfo;


        [Space(15)]
        public TextMeshProUGUI energyTxt;

        public RectTransform energyInfo;

        [Space(15)]
        public TextMeshProUGUI statusInfoTxt;

        public RectTransform statusInfo;


        [Space(15)]
        public TextMeshProUGUI teamAPlayers;

        public TextMeshProUGUI teamBPlayers;
        public TextMeshProUGUI spectatorPlayers;

        private void Update() {
            mapTimeTxt.SetText(getMapText());

            string teamText = GetTeamTxt();
            if (teamText != null) {
                teamInfo.gameObject.SetActive(true);
                teamTxt.SetText(teamText);
            } else {
                teamInfo.gameObject.SetActive(false);
            }

            string spawnText = getRespawnsText();
            if (spawnText != null) {
                spawnTimeInfo.gameObject.SetActive(true);
                spawnTimeTxt.SetText(spawnText);
            } else {
                spawnTimeInfo.gameObject.SetActive(false);
            }

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

            string energyText = getEnergyTxt();
            if (energyText != null) {
                energyInfo.gameObject.SetActive(true);
                energyTxt.SetText(energyText);
            } else {
                energyInfo.gameObject.SetActive(false);
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

            TimeSpan teamARespawnSpawn;
            TimeSpan teamBRespawnSpawn;

            int teamARespawn = MapMaster.MapInstance().TeamARespawn();
            int teamBRespawn = MapMaster.MapInstance().TeamBRespawn();

            if (time > 0) {
                teamARespawnSpawn = TimeSpan.FromSeconds(teamARespawn - (time % teamARespawn));
                teamBRespawnSpawn = TimeSpan.FromSeconds(teamBRespawn - (time % teamBRespawn));
            } else {
                return "??/??";
            }

            return teamARespawnSpawn.Seconds + "/" + teamBRespawnSpawn.Seconds;
        }

        private string getMapText() {
            IBaseMapController mapInstance = MapMaster.MapInstance();
            MapState mapState = mapInstance.GetMapState();
            float totalDuration = mapInstance.MapDuration();
            float warmupDuration = mapInstance.WarmupDuration();
            float timeElapsed = mapInstance.TimeElapsed();
            float remaining;
            if (mapState == MapState.Warmup) {
                remaining = warmupDuration - timeElapsed;
            } else if (mapState == MapState.Match) {
                remaining = totalDuration - timeElapsed;
            } else {
                return null;
            } 

            TimeSpan timeSpan = TimeSpan.FromSeconds(remaining);
            int minutes = timeSpan.Minutes;
            int seconds = timeSpan.Seconds;

            return (minutes < 10 ? "0" + minutes : "" + minutes) 
                   + ":" 
                   + (seconds < 10 ? "0" + seconds : "" + seconds);
        }

        private string GetTeamTxt() {
            if (NetworkPlayer.networkPlayerOwner != null) {
                NetworkPlayer owner = NetworkPlayer.networkPlayerOwner;
                GameTeam gameTeam = owner.GetNetworkTeam();
                GameRole gameRole = owner.GetNetworkClassRole();
                string ret;
                switch (gameTeam) {
                    case GameTeam.Spectator:
                        ret = "Spectator";
                        break;
                    case GameTeam.TeamA:
                        ret = "Team A";
                        break;
                    case GameTeam.TeamB:
                        ret = "Team B";
                        break;
                    default:
                        ret = "N/A";
                        break;
                }

                return ret + ": " + gameRole;
            }

            return null;
        }


        private string getAmmoText() {
            if (NetworkPlayer.networkPlayerOwner != null) {
                NetPlayerController netFirstPersonController = NetworkPlayer.networkPlayerOwner.fpsController;
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
                if (networkPlayer.GetNetworkTeam() == GameTeam.Spectator) {
                    data.Add("You're an spectator, press [L] to select a team to Join.");
                }

                if (networkPlayer.fpsController != null) {
                    NetPlayerController fpsController = networkPlayer.fpsController;
                    if (fpsController.soldier != null) {
                        PlayableSoldier playableSoldier = fpsController.soldier;
                        if (playableSoldier.IsKnockedDown()) {
                            data.Add("You're knocked down. You can wait to be revived or press [Space] to respawn.");
                        }
                    }
                }

                PlayerState networkPlayerState = networkPlayer.GetPlayerState();
                if (
                    networkPlayerState == PlayerState.Following ||
                    networkPlayerState == PlayerState.PlayingDead
                ) {
                    //TODO: Add relevant info like player name instead of it's ID
                    data.Add("You're spectating a team mate (with id " + networkPlayer.GetNetworkFollowing() + ")");
                }
            }

            IBaseMapController mapInstance = MapMaster.MapInstance();
            if (mapInstance != null) {
                MapState mapState = mapInstance.GetMapState();
                if (mapState == MapState.Warmup) {
                    //float remaining = mapInstance.WarmupDuration() - mapInstance.TimeElapsed();
                    //TimeSpan timeSpan = TimeSpan.FromSeconds(remaining);
                    data.Add("WarmUP!!! Waiting " + getMapText() + "s to start the map");
                } else if (mapState == MapState.Tie) {
                    data.Add("Map ended with a TIE!");
                } else if (mapState == MapState.WinA) {
                    data.Add("Map ended, winning team A!");
                } else if (mapState == MapState.WinB) {
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
                    return Math.Ceiling(playableSoldier.networkHealth.Value) + "";
                }
            }

            return null;
        }

        private string getEnergyTxt() {
            if (NetworkPlayer.networkPlayerOwner != null) {
                NetPlayerController netFirstPersonController = NetworkPlayer.networkPlayerOwner.fpsController;
                if (netFirstPersonController) {
                    PlayableSoldier playableSoldier = netFirstPersonController.soldier;
                    return Math.Ceiling(playableSoldier.networkEnergy.Value) + " / 100";
                }
            }

            return null;
        }
    }
}