using System;
using CharacterController;
using Controller;
using Core;
using Enums;
using Map;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Player {
    public class HudController : Singleton<HudController> {
        public TextMeshProUGUI teamTxt;
        public TextMeshProUGUI spawnTimeTxt;
        public TextMeshProUGUI mapTimeTxt;
        public TextMeshProUGUI ammoTxt;
        public TextMeshProUGUI healthTxt;
        
        
        public TextMeshProUGUI teamAPlayers;
        public TextMeshProUGUI teamBPlayers;
        public TextMeshProUGUI spectatorPlayers;

        private Team _drawingTeam;
        private void Update() {
            teamTxt.SetText(GetTeamTxt());
            mapTimeTxt.SetText(getMapText());
            spawnTimeTxt.SetText(getRespawnsText());
            ammoTxt.SetText(getAmmoText());
            healthTxt.SetText(getHealthText());
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
            float totalDuration = MapMaster.MapInstance().MapDuration();
            float remaining = totalDuration - MapMaster.MapInstance().TimeElapsed();
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

            return "??/???";
        }

        private string getHealthText() {
            if (Network.NetworkPlayer.networkPlayerOwner != null) {
                NetPlayerController netFirstPersonController = Network.NetworkPlayer.networkPlayerOwner.fpsController;
                if (netFirstPersonController) {
                    PlayableSoldier playableSoldier = netFirstPersonController.soldier;
                    return playableSoldier.networkHealth.Value + "";
                }
            }

            return "???";
        }

        public static void ChangeTeam(Team team) {
            Instance._drawingTeam = team;
        }
        
        

    }
}