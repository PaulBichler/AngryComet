using System;
using Game;
using UnityEngine;

namespace Behaviors
{
    [System.Serializable]
    public class SunBehavior : PlanetBehavior
    {
        [SerializeField] private SpriteRenderer face = null;
        [SerializeField] private float worryDistance = 20f;
        private bool _isClose;
        
        public override void Hit(PlayerController player)
        {
            base.Hit(player);
            
            if (player.CanKillSun)
            {
                ProjectileHit(player);
            }
            else
            {
                Time.timeScale = 1f;
                player.Die();
            }
        }

        private void Update()
        {
            PlayerController player = GameMode.instance.PlayerController;
            
            if (player && player.CanKillSun)
            {
                //Sun Killer Upgrade actived
                
                float distance = Vector2.Distance(transform.position, player.transform.position);
                
                if (!_isClose && distance < worryDistance)
                {
                    //puts on a worried face if the player gets too close :(
                    face.sprite = GameMode.instance.worriedFace;
                    _isClose = true;
                }
                else if (_isClose && distance > worryDistance)
                {
                    //and a happy face if not :D
                    face.sprite = GameMode.instance.happyFace; 
                    _isClose = false;
                }
            }
        }
    }
}
