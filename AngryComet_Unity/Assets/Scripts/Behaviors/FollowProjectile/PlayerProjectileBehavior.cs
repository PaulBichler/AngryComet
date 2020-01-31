using System;
using Game;
using UnityEngine;

namespace Behaviors.FollowProjectile
{
    public class PlayerProjectileBehavior : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        private PlayerController _playerController;
        private float _lifetime = 3f;
        private Transform _target;
        private bool _isQuitting = false;

        private void Start()
        {
            _playerController = FindObjectOfType<PlayerController>();
        }

        void Update()
        {
            //check lifetime and player state
            if (_lifetime > 0 && _playerController.gameObject.activeSelf)
            {
                //is target still alive
                if (_target)
                {
                    //follow target
                    var tarPos = _target.position;
                    var position = transform.position;

                    position = Vector2.MoveTowards(position, tarPos, speed * Time.deltaTime);

                    var transform1 = transform;
                    transform1.position = position;
                    transform1.up = tarPos - transform1.position;
                }
                else
                {
                    //get next closest target (preferably enemy projectiles)
                    _target = GetClosestWithTag("Projectile");
                    if (!_target) _target = GetClosestWithTag("Planet");
                }
                
                //decrase lifetime
                _lifetime -= Time.deltaTime;
            }
            else
            {
                //Kill projectile
                Destroy(gameObject);
            }
        }

        Transform GetClosestWithTag(string entityTag)
        {
            //get closest transform with the tag <entityTag>
            GameObject[] entities = GameObject.FindGameObjectsWithTag(entityTag);
            GameObject bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = transform.position;

            foreach (GameObject entity in entities)
            {
                Vector3 directionToTarget = entity.transform.position - currentPosition;
                float disSqrToTarget = directionToTarget.sqrMagnitude;
                if (disSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = disSqrToTarget;
                    bestTarget = entity;
                }
            }
            
            //return closest transform
            if (bestTarget != null) return bestTarget.transform;
            return null;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //destroy planets and projectiles on touch
            if (other.gameObject.CompareTag("Planet"))
            {
                other.gameObject.GetComponent<PlanetBehavior>().ProjectileHit(_playerController);
            }
            else if (other.gameObject.CompareTag("Projectile"))
            {
                Destroy(other.gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            //destroy iron planets on touch (they dont use triggers)
            if (other.gameObject.CompareTag("Planet"))
            {
                other.gameObject.GetComponent<PlanetBehavior>().ProjectileHit(_playerController);
            }
        }

        private void OnApplicationQuit()
        {
            //to prevent OnDestroy exception
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            //spawn death particle effect
            if(!_isQuitting)
                Instantiate(GameMode.instance.playerDeathEffect, transform.position, Quaternion.identity);
        }
    }
}
