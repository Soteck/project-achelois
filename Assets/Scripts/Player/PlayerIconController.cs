using UnityEngine;
using NetworkPlayer = Network.NetworkPlayer;

namespace Player {
    public class PlayerIconController : MonoBehaviour {
        public PlayableSoldier soldier;
        public RectTransform inMenu;
        public RectTransform withObjective;
        public RectTransform texting;

        void LateUpdate() {
            if (NetworkPlayer.networkPlayerOwner != null) {
                NetworkPlayer networkPlayer = NetworkPlayer.networkPlayerOwner;
                Camera cameraToLookAt = networkPlayer.activeCamera;
                if (cameraToLookAt != null) {
                    transform.LookAt(cameraToLookAt.transform);
                    transform.rotation = Quaternion.LookRotation(cameraToLookAt.transform.forward);
                }
            }

            if (soldier != null && soldier.IsSpawned) {
                inMenu.gameObject.SetActive(soldier.InMenu());
                withObjective.gameObject.SetActive(soldier.HasObjective());
                texting.gameObject.SetActive(soldier.IsTexting());
            }
        }
    }
}