using Controller;
using Network.Shared;
using Player;
using UnityEngine;
using Util;

namespace Items {
    public abstract class EquipableItemLogic : NetController {
        public EquipableItemVisual visual;
        public Camera playerCamera;
        public string item_id;
        public Animator animator;
        public bool busy = false;
        public EquipableItemVisual spawnedVisual { get; set; }
        public PlayableSoldier soldierOwner;
        
        public AudioClip hitBodySound;
        public AudioClip hitHeadSound;
        public AudioClip hitTeamBodySound;
        public AudioClip hitTeamHeadSound;

        protected AudioSource hitBodySource;
        protected AudioSource hitHeadSource;
        protected AudioSource hitTeamBodySource;
        protected AudioSource hitTeamHeadSource;

        public new void Awake() {
            base.Awake();
            hitBodySource = AudioUtil.AddAudio(gameObject, false, false, 1f, hitBodySound);
            hitHeadSource = AudioUtil.AddAudio(gameObject, false, false, 1f, hitHeadSound);
            hitTeamBodySource = AudioUtil.AddAudio(gameObject, false, false, 1f, hitTeamBodySound);
            hitTeamHeadSource = AudioUtil.AddAudio(gameObject, false, false, 1f, hitTeamHeadSound);
        }
        
        public abstract EquipableItemNetworkData ToNetWorkData();
        protected abstract void InternalCallInitMetaData(NetworkString meta);

        public abstract string GetStatus();


        public void CallInitMetaData(NetworkString meta) {
            InternalCallInitMetaData(meta);
        }

        public abstract void AttachVisual(EquipableItemVisual visualItem);

        public abstract void ServerAmmoPickup();
    }
}