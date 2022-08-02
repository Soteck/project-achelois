using Player;
using UnityEngine;

namespace Items {
    public class HealthPackPickup : MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            PlayableSoldier holder = other.gameObject.GetComponent<PlayableSoldier>();
            if (holder) {
                holder.HealthPickUp();
            }
        }
    }
}