using Network.Shared;

namespace Items {
    public class PackLogic : EquipableItemLogic{
        protected override void ServerCalculations() {
            throw new System.NotImplementedException();
        }

        protected override void ClientBeforeInput() {
            throw new System.NotImplementedException();
        }

        protected override void ClientInput() {
            throw new System.NotImplementedException();
        }

        protected override void ClientMovement() {
            throw new System.NotImplementedException();
        }

        protected override void ClientVisuals() {
            throw new System.NotImplementedException();
        }

        public override EquipableItemNetworkData ToNetWorkData() {
            throw new System.NotImplementedException();
        }

        protected override void InternalCallInitMetaData(NetworkString meta) {
            throw new System.NotImplementedException();
        }

        public override string GetStatus() {
            throw new System.NotImplementedException();
        }

        public override void AttachVisual(EquipableItemVisual visualItem) {
            throw new System.NotImplementedException();
        }

        public override void ServerAmmoPickup() {
            //N/A
        }
    }
}