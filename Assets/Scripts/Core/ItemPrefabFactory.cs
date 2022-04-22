using System.Collections.Generic;
using Player;

namespace Core {
    public class ItemPrefabFactory : Singleton<ItemPrefabFactory> {
        public EquipableItemLogic[] items;

        private Dictionary<string, EquipableItemLogic> itemsMap = new Dictionary<string, EquipableItemLogic>();

        private void Awake() {
            foreach (EquipableItemLogic item in items) {
                itemsMap[item.item_id] = item;
            }
        }

        public static EquipableItemLogic PrefabById(string id) {
            return Instance.itemsMap[id];
        }
    }
}