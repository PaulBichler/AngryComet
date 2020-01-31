using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Behaviors
{
    [System.Serializable]
    public class PlanetBehavior : MonoBehaviour
    {
        [SerializeField] protected int rewardedPoints;
        [SerializeField] protected int rewardedCoins;
        [SerializeField] protected float rewardedHealth;
        [SerializeField] protected float rewardedXp;
        [SerializeField] protected float scaleMin = 1f;
        [SerializeField] protected float scaleMax = 2f;
        [SerializeField] protected float forceMin = 1f;
        [SerializeField] protected float forceMax = 4f;

        protected Rigidbody2D rb2d;
        private CameraController _camScript;
        
        public virtual void Start()
        {
            rb2d = GetComponent<Rigidbody2D>();
            _camScript = FindObjectOfType<CameraController>();
        
            Vector2 randDirection = Random.insideUnitCircle.normalized;
            float randForce = Random.Range(forceMin, forceMax);
            rb2d.velocity = randDirection * randForce;
        
            float randScale = Random.Range(scaleMin, scaleMax);
            transform.localScale = new Vector3(randScale, randScale, randScale);

            float randStartRotation = Random.Range(0f, 359f);
            transform.Rotate(0, 0, randStartRotation);
            float randAngularVelocity = Random.Range(-50f, 50f);
            rb2d.angularVelocity = randAngularVelocity;
        }

        public virtual void Hit(PlayerController player)
        {
            player.AddXp(rewardedXp);

            if (GameMode.instance.planetDeathSound)
                AudioSource.PlayClipAtPoint(GameMode.instance.planetDeathSound, transform.position);
            
            StartCoroutine(_camScript.Shake(.3f, 2f));
        }

        public void ProjectileHit(PlayerController player)
        {
            player.AddXp(rewardedXp);
            player.AddHealth(rewardedHealth);
            player.AddCoins(rewardedCoins, true);
            player.AddPoints(rewardedPoints);
            
            if (player.isInvincible)
            {
                if (GameMode.instance.planetDeathSound)
                    AudioSource.PlayClipAtPoint(GameMode.instance.planetDeathSound, transform.position);
            }
                
            Instantiate(GameMode.instance.enemyDeathEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}