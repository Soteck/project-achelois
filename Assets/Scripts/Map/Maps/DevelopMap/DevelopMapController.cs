
using System.Collections.Generic;
using Enums;
using Network.Shared;
using Player;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Util;

namespace Map.Maps.DevelopMap {
    public class DevelopMapController : BaseMapController<DevelopMapController>, DevelopMapControllerInterface {
        
        public TextMeshProUGUI[] scoreShowers;
        
        public GameObject flagAPosition;
        public GameObject flagBPosition;
        
        public ObjectivePickup flagAPrefab;
        public ObjectivePickup flagBPrefab;

        public ObjectiveDropZone flagADropZone;
        public ObjectiveDropZone flagBDropZone;

        private NetworkVariable<int> teamAScore = new NetworkVariable<int>();
        private NetworkVariable<int> teamBScore = new NetworkVariable<int>();

        //Server private values
        private List<string> teamAObjectiveIdentifiers;
        private List<string> teamBObjectiveIdentifiers;

        private List<string> teamAObjectiveTaken;
        private List<string> teamBObjectiveTaken;

        private List<string> teamAObjectivesDelivered;
        private List<string> teamBObjectivesDelivered;

        private int aCounter = 0;
        private int bCounter = 0;

        public void Awake() {
            MapMaster.Instance.instance = this;
        }

        new void Start() {
            base.Start();
            NetworkManager.Singleton.OnServerStarted += ServerInit;
        }

        public override int GetWinningTeam() {
            return teamAScore.Value - teamBScore.Value;
        }

        public new void Update() {
            base.Update();
            UpdateScorePanels();
        }

        private void UpdateScorePanels() {
            if (scoreShowers != null) {
                foreach (TextMeshProUGUI scoreShower in scoreShowers) {
                    scoreShower.SetText($"<color=\"red\">[{teamAScore.Value}</color><color=\"black\"> - </color><color=\"blue\">{teamBScore.Value}]</color>");
                }
            }
        }

        private void ServerInit() {
            teamAScore.Value = 0;
            teamBScore.Value = 0;
            teamAObjectiveIdentifiers = new List<string>();
            teamBObjectiveIdentifiers = new List<string>();
            teamAObjectivesDelivered = new List<string>();
            teamBObjectivesDelivered = new List<string>();
            teamAObjectiveTaken = new List<string>();
            teamBObjectiveTaken = new List<string>();
            SpawnTeamAFlag();
            SpawnTeamBFlag();
        }

        private void SpawnTeamAFlag() {
            //spawn red flag
            string code = aCounter++ + "_a_flag";
            teamAObjectiveIdentifiers.Add(code);
            SpawnFlag(flagAPrefab, flagAPosition.transform, code);
        }

        private void SpawnTeamBFlag() {
            //spawn blue flag
            string code = bCounter++ + "_b_flag";
            teamBObjectiveIdentifiers.Add(code);
            SpawnFlag(flagBPrefab, flagBPosition.transform, code);
        }


        private void SpawnFlag(ObjectivePickup prefab, Transform flagTransformPosition, string code) {
            ObjectivePickup go = Instantiate(prefab, flagTransformPosition.position, flagTransformPosition.rotation);
            NetworkObject no = go.GetComponent<NetworkObject>();
            go.objectiveCode.Value = code;
            no.Spawn();
        }

        [ServerRpc]
        private void OnPlayerPickedUpObjectiveServerRpc(ulong playerId, ulong objectiveId) {
            NetworkObject playerInstance = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            NetworkObject objectiveInstance = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectiveId];
            PlayableSoldier ps = playerInstance.GetComponent<PlayableSoldier>();
            Network.NetworkPlayer player = Network.NetworkPlayer.NetworkPlayerByControllerId(playerInstance.OwnerClientId);
            ObjectivePickup op = objectiveInstance.GetComponent<ObjectivePickup>();
            NetworkString objectiveCodeValue = op.objectiveCode.Value;
            
            //Check if can be Picked Up
            if (ps.HasObjective()) {
                return;
            }
            if (player.GetNetworkTeam() == Team.TeamA) {
                if (!teamBObjectiveIdentifiers.Contains(objectiveCodeValue)) {
                    return;
                }
            }else if (player.GetNetworkTeam() == Team.TeamB) {
                if (!teamAObjectiveIdentifiers.Contains(objectiveCodeValue)) {
                    return;
                }
            }
            
            //Do the actual Pick Up
            if (player.GetNetworkTeam() == Team.TeamA) {
                teamBObjectiveIdentifiers.Remove(objectiveCodeValue);
                teamBObjectiveTaken.Add(objectiveCodeValue);
            }else if (player.GetNetworkTeam() == Team.TeamB) {
                teamAObjectiveIdentifiers.Remove(objectiveCodeValue);
                teamAObjectiveTaken.Add(objectiveCodeValue);
            }
            ps.networkObjective.Value = objectiveCodeValue;
            objectiveInstance.Despawn();
        }

        [ServerRpc]
        private void OnPlayerEnteredToDropZoneServerRpc(ulong playerId, ulong dropZoneId) {
            NetworkObject playerInstance = NetworkManager.Singleton.SpawnManager.SpawnedObjects[playerId];
            NetworkObject dropZoneInstance = NetworkManager.Singleton.SpawnManager.SpawnedObjects[dropZoneId];
            PlayableSoldier ps = playerInstance.GetComponent<PlayableSoldier>();
            Network.NetworkPlayer player = Network.NetworkPlayer.NetworkPlayerByControllerId(playerInstance.OwnerClientId);
            ObjectiveDropZone dropZone = dropZoneInstance.GetComponent<ObjectiveDropZone>();

            if (flagADropZone.drop_zone_id.Equals(dropZone.drop_zone_id) && player.GetNetworkTeam() == Team.TeamA) {
                string objectiveId = ps.networkObjective.Value;
                teamBObjectiveTaken.Remove(objectiveId);
                teamAObjectivesDelivered.Add(objectiveId);
                teamAScore.Value++;
                ps.networkObjective.Value = Constants.OBJECTIVE_NONE;
                SpawnTeamBFlag();
            }else if (flagBDropZone.drop_zone_id.Equals(dropZone.drop_zone_id) && player.GetNetworkTeam() == Team.TeamB) {
                string objectiveId = ps.networkObjective.Value;
                teamAObjectiveTaken.Remove(objectiveId);
                teamBObjectivesDelivered.Add(objectiveId);
                teamBScore.Value++;
                ps.networkObjective.Value = Constants.OBJECTIVE_NONE;
                SpawnTeamAFlag();
            }

        }
        
        

        //Client-Side function calls
        public void PlayerEnteredToDropZone(PlayableSoldier holder, ObjectiveDropZone objectiveDropZone) {
            OnPlayerEnteredToDropZoneServerRpc(
                holder.NetworkObjectId,
                objectiveDropZone.NetworkObjectId);
        }

        public void PlayerPickedUpObjective(PlayableSoldier holder, ObjectivePickup objectivePickup) {
            OnPlayerPickedUpObjectiveServerRpc(
                holder.NetworkObjectId,
                objectivePickup.NetworkObjectId);
        }
    }
}