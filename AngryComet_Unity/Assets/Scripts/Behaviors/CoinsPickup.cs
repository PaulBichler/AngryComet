using System.Collections;
using System.Collections.Generic;
using Behaviors;
using Game;
using UnityEngine;

public class CoinsPickup : PlanetBehavior
{
    public override void Hit(PlayerController player)
    {
        //call base Hit
        base.Hit(player);
        
        //reward player + spawn death particle effect + destroy self
        player.AddHealth(rewardedHealth);
        player.AddCoins(rewardedCoins);
        Instantiate(GameMode.instance.enemyDeathEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
