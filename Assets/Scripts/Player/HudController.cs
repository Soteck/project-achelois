using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Player {
    public class HudController : MonoBehaviour {
        public TextMeshProUGUI timeTxt;
        public TextMeshProUGUI ammoTxt;
        public TextMeshProUGUI healthTxt;

        private void Update() {
            timeTxt.SetText(getTeamText());
            ammoTxt.SetText(getAmmoText());
            healthTxt.SetText(getHealthText());
        }

        private string getTeamText() {
            float time = MapController.TimeElapsed();

            double teamARemaining = 0;
            double teamBRemaining = 0;
            if (time > 0) {
                teamARemaining = Math.Round(MapController.Instance.teamARespawn -
                                            (time % MapController.Instance.teamARespawn));
                teamBRemaining = Math.Round(MapController.Instance.teamBRespawn -
                                            (time % MapController.Instance.teamBRespawn));
            }

            return teamARemaining + "/" + teamBRemaining + " - " + time;
        }

        private string getAmmoText() {
            if (Network.NetworkPlayer.networkPlayerOwner != null) {
                PlayableSoldier ps = Network.NetworkPlayer.networkPlayerOwner.currentSoldier;
                if (ps) {
                    EquipableItem activeItem = ps.ActiveItem();
                    if (activeItem) {
                        return activeItem.GetStatus();
                    }
                }

            }

            return "??/???";
        }

        private string getHealthText() {
            if (Network.NetworkPlayer.networkPlayerOwner != null) {
                PlayableSoldier ps = Network.NetworkPlayer.networkPlayerOwner.currentSoldier;
                if (ps) {
                    EquipableItem activeItem = ps.ActiveItem();
                    return ps.networkHealth.Value + "";
                }
            }

            return "???";
        }
    }
}