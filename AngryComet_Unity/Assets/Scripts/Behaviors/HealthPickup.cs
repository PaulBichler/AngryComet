using Game;

namespace Behaviors
{
    public class HealthPickup : PlanetBehavior
    {
        public override void Hit(PlayerController player)
        {
            //call base Hit
            base.Hit(player);
            
            //reward health to player + spawn death particle effect + destroy self
            player.AddHealth(GameMode.instance.maxPlayerHealth);
            Instantiate(GameMode.instance.enemyDeathEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
