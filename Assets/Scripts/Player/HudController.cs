using System;
using Controller;
using Core;
using Enums;
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
            float time = MapController.TimeElapsed();

            double teamARemaining = 0;
            double teamBRemaining = 0;
            if (time > 0) {
                teamARemaining = Math.Round(MapController.Instance.teamARespawn -
                                            (time % MapController.Instance.teamARespawn));
                teamBRemaining = Math.Round(MapController.Instance.teamBRespawn -
                                            (time % MapController.Instance.teamBRespawn));
            }

            return teamARemaining + "/" + teamBRemaining;
        }

        private string getMapText() {
            return Math.Floor(MapController.TimeElapsed()) + "";
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
                NetFirstPersonController netFirstPersonController = Network.NetworkPlayer.networkPlayerOwner.fpsController;
                if (netFirstPersonController) {
                    PlayableSoldier playableSoldier = netFirstPersonController.soldier;
                    EquipableItem activeItem = playableSoldier.ActiveItem();
                    if (activeItem) {
                        return activeItem.GetStatus();
                    }
                }

            }

            return "??/???";
        }

        private string getHealthText() {
            if (Network.NetworkPlayer.networkPlayerOwner != null) {
                NetFirstPersonController netFirstPersonController = Network.NetworkPlayer.networkPlayerOwner.fpsController;
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