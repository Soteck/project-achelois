using Enums;

namespace World {
    public interface IDamageableEntity {
        public void ServerTakeDamage(float amount);
        public bool ServerCanTakeDamage();
        public Team ServerGetTeam();
    }
}