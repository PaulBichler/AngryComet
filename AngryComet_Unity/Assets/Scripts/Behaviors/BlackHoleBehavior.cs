using Game;
using UnityEngine;

namespace Behaviors
{
    public class BlackHoleBehavior : PlanetBehavior
    {
        private const float G = 6f;
        [SerializeField] private float attractRadius = 10f;
    
        private Rigidbody2D _playerRb2d;
    
        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
            _playerRb2d = GameMode.instance.PlayerController.gameObject.GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_playerRb2d && !GameMode.instance.isGameOver)
            {
                Vector2 direction = (rb2d.position - _playerRb2d.position);
                float distance = direction.magnitude;

                if (distance < attractRadius)
                {
                    if (distance == 0) return;
                
                    float forceMagnitude = G * (rb2d.mass * _playerRb2d.mass) / Mathf.Pow(distance, 2);
                    Vector2 force = direction.normalized * forceMagnitude;
                
                    _playerRb2d.AddForce(force);
                }
            }
        }
    
        public override void Hit(PlayerController player)
        {
            base.Hit(player);
            Time.timeScale = 1f;
            GameMode.instance.PlayerController.Die();
        }
    }
}
