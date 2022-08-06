using Controller;
using Network.Shared;
using Player;
using ScriptableObjects;
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


        protected SoundsScriptableObject _soundsScriptableObject;


        protected AudioSource hitBodySource;
        protected AudioSource hitHeadSource;
        protected AudioSource hitTeamBodySource;
        protected AudioSource hitTeamHeadSource;

        public new void Awake() {
            base.Awake();
            _soundsScriptableObject = ResourceLoaderUtil.Instance.soundsScriptableObject;
            hitBodySource = AudioUtil.AddAudio("hit-sounds", "hit-body",
                                               false, false, 1f, _soundsScriptableObject.hitBodySound);
            hitHeadSource = AudioUtil.AddAudio("hit-sounds", "hit-head",
                                               false, false, 1f, _soundsScriptableObject.hitHeadSound);
            hitTeamBodySource = AudioUtil.AddAudio("hit-sounds", "team-body",
                                                   false, false, 1f, _soundsScriptableObject.hitTeamBodySound);
            hitTeamHeadSource = AudioUtil.AddAudio("hit-sounds", "team-head", 
                                                   false, false, 1f, _soundsScriptableObject.hitTeamHeadSound);
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