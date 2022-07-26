using System.Collections.Generic;
using Items;
using Player;

namespace Core {
    public class EquipmentPrefabFactory : Singleton<EquipmentPrefabFactory> {
        private Dictionary<string, EquipableItemLogic> itemsMap =
            new Dictionary<string, EquipableItemLogic>();

        private void DoRegister(EquipableItemLogic prefab) {
            itemsMap[prefab.item_id] = prefab;
        }

        public EquipableItemLogic DoGetPrefabByItemID(string item_id) {
            return itemsMap[item_id];
        }

        public static EquipableItemLogic GetPrefabByItemID(string item_id) {
            return Instance.DoGetPrefabByItemID(item_id);
        }

        public static void Register(EquipableItemLogic prefab) {
            Instance.DoRegister(prefab);
        }
    }
}