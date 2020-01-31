using System;
using Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Behaviors
{
    [System.Serializable]
    public class NormalPlanetBehavior : PlanetBehavior
    {
        [SerializeField] private SpriteRenderer face = null;
        [SerializeField] private float worryDistance = 20f;
        private bool _isClose;
        private Transform _player;
        
        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            _player = GameMode.instance.PlayerController.transform;
            
            //put on a happy face
            face.sprite = GameMode.instance.happyFace;
        }

        private void Update()
        {
            if (_player && face)
            {
                float distance = Vector2.Distance(transform.position, _player.position);
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

        public override void Hit(PlayerController player)
        {
            base.Hit(player);
            player.AddHealth(rewardedHealth * (scaleMax / transform.localScale.x));
            player.AddPoints(rewardedPoints);
            player.AddCoins(rewardedCoins, true);
            Instantiate(GameMode.instance.enemyDeathEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
