using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Player {
    public class HudController : MonoBehaviour {
        public TextMeshProUGUI timeTxt;
        public TextMeshProUGUI ammoTxt;

        private void Update() {
            timeTxt.SetText(getTextTeam());
        }

        private string getTextTeam() {
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
    }
}