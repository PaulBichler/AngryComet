using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Behaviors.FollowProjectile
{
    public class FpBaseBehavior : PlanetBehavior
    {
        [SerializeField] private GameObject projectile = null;
        [SerializeField] private float minIntervalSeconds = 1f;
        [SerializeField] private float maxIntervalSeconds = 3f;

        private float _currentCd;
        private Camera _camera;

        // Start is called before the first frame update
        public override void Start()
        {
            //call base Start
            base.Start();
            
            //set random cooldown time between range
            _currentCd = Random.Range(minIntervalSeconds, maxIntervalSeconds);
            _camera = FindObjectOfType<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameMode.instance.isGameOver)
            {
                Vector3 vpPos = _camera.WorldToViewportPoint(transform.position);
                
                //checks if obj is within camera bounds
                if (vpPos.x <= 1f && vpPos.x >= 0 && vpPos.y <= 1f && vpPos.y >= 0) 
                {
                    if (_currentCd <= 0)
                    {
                        //spawn projectile + set new cooldown
                        Instantiate(projectile, transform.position, Quaternion.identity);
                        _currentCd = Random.Range(minIntervalSeconds, maxIntervalSeconds);
                    }
                    else
                    {
                        //decrease cooldown time
                        _currentCd -= Time.deltaTime;
                    }
                }
            }
        }
        
        public override void Hit(PlayerController player)
        {
            //call base Hit
            base.Hit(player);
            
            //reward player + spawn death particle effect + destroy self
            player.AddHealth(rewardedHealth * (scaleMax / transform.localScale.x));
            player.AddPoints(rewardedPoints);
            Instantiate(GameMode.instance.enemyDeathEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
