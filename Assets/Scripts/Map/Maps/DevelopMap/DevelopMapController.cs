using System;
using Core;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Map.Maps.DevelopMap {
    public class DevelopMapController : BaseMapController<DevelopMapController>, DevelopMapControllerInterface {
        public GameObject flagAPosition;
        public GameObject flagBPosition;

        public ObjectivePickup flagAPrefab;
        public ObjectivePickup flagBPrefab;

        public ObjectiveDropZone flagADropZone;
        public ObjectiveDropZone flagBDropZone;

        private NetworkVariable<int> teamAScore = new NetworkVariable<int>();
        private NetworkVariable<int> teamBScore = new NetworkVariable<int>();

        //Server private values

        public void Awake() {
            MapMaster.Instance.instance = this;
        }

        new void Start() {
            base.Start();
            NetworkManager.Singleton.OnServerStarted += ServerInit;
        }

        private void ServerInit() {
            teamAScore.Value = 0;
            teamBScore.Value = 0;
            SpawnFlag(flagAPrefab, flagAPosition.transform);
            SpawnFlag(flagBPrefab, flagBPosition.transform);
        }

        private void SpawnFlag(ObjectivePickup prefab, Transform flagTransformPosition) {
            ObjectivePickup go = Instantiate(prefab, flagTransformPosition.position, flagTransformPosition.rotation);
            NetworkObject no = go.GetComponent<NetworkObject>();
            no.Spawn();
        }

        public void PlayerEnteredToDropZone(PlayableSoldier holder, ObjectiveDropZone objectiveDropZone) {
            throw new NotImplementedException();
        }
    }
}