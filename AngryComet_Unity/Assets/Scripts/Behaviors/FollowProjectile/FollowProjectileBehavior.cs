using System;
using Game;
using UnityEngine;

namespace Behaviors.FollowProjectile
{
    public class FollowProjectileBehavior : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 4f;
        private float _currentLifetime;
        private Transform _target;
        private bool _isQuitting;

        // Start is called before the first frame update
        void Start()
        {
            _currentLifetime = lifetime;
            _target = FindObjectOfType<PlayerController>().transform;
        }
    
        void Update()
        {
            if (_target && _currentLifetime > 0)
            {
                var tarPos = _target.position;
                var position = transform.position;
            
                position = Vector2.MoveTowards(position, tarPos, speed * Time.deltaTime);
            
                var transform1 = transform;
                transform1.position = position;
                transform1.up = tarPos - transform1.position;

                _currentLifetime -= Time.deltaTime;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            if(!_isQuitting)
                Instantiate(GameMode.instance.enemyDeathEffect, transform.position, Quaternion.identity);
        }
    }
}
