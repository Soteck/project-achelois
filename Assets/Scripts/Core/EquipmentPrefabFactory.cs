using System.Collections.Generic;
using Player;

namespace Core {
    public class EquipmentPrefabFactory : Singleton<EquipmentPrefabFactory> {
        private Dictionary<string, EquipableItem> itemsMap =
            new Dictionary<string, EquipableItem>();

        private void DoRegister(EquipableItem prefab) {
            itemsMap[prefab.item_id] = prefab;
        }

        public EquipableItem DoGetPrefabByItemID(string item_id) {
            return itemsMap[item_id];
        }

        public static EquipableItem GetPrefabByItemID(string item_id) {
            return Instance.DoGetPrefabByItemID(item_id);
        }

        public static void Register(EquipableItem prefab) {
            Instance.DoRegister(prefab);
        }
    }
}