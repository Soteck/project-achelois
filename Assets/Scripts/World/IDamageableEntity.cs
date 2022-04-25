namespace World {
    public interface IDamageableEntity {
        public void ServerTakeDamage(float amount);
        public void ServerDie();
    }
}