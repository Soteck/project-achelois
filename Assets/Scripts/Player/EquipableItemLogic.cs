using Controller;
using Network.Shared;
using UnityEngine;

namespace Player {
    public abstract class EquipableItemLogic : NetController {
        public EquipableItemVisual visual;
        public Camera playerCamera;
        public string item_id;
        public Animator animator;
        public bool busy = false;
        public EquipableItemVisual spawnedVisual { get; set; }

        public abstract EquipableItemNetworkData ToNetWorkData();
        protected abstract void InternalCallInitMetaData(EquipableItemNetworkData meta);

        public abstract string GetStatus();


        public void CallInitMetaData(EquipableItemNetworkData meta) {
            InternalCallInitMetaData(meta);
        }

        public abstract void AttachVisual(EquipableItemVisual visualItem);
    }
}