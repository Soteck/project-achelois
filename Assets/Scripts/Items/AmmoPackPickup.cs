using Player;
using UnityEngine;

namespace Items {
    public class AmmoPackPickup : MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            PlayableSoldier holder = other.gameObject.GetComponent<PlayableSoldier>();
            if (holder) {
                holder.AmmoPickUp();
            }
        }
    }
}