using Controller;
using Network.Shared;
using Player;
using UnityEngine;

namespace Items {
    public abstract class EquipableItemLogic : NetController {
        public EquipableItemVisual visual;
        public Camera playerCamera;
        public string item_id;
        public Animator animator;
        public bool busy = false;
        public EquipableItemVisual spawnedVisual { get; set; }
        public PlayableSoldier soldierOwner;

        public abstract EquipableItemNetworkData ToNetWorkData();
        protected abstract void InternalCallInitMetaData(NetworkString meta);

        public abstract string GetStatus();


        public void CallInitMetaData(NetworkString meta) {
            InternalCallInitMetaData(meta);
        }

        public abstract void AttachVisual(EquipableItemVisual visualItem);
    }
}