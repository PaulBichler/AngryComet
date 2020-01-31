using Behaviors.Abilities;
using Game;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Behaviors
{
    public class PlayerMovement : MonoBehaviour
    {
        private Rigidbody2D _rb2d;
        private Camera _camera;
        private PlayerController _playerController;
        private LineRenderer _trajLine;
        private Vector3 _mousePos;
        private bool _launch;
        public float launchForce = 200f;
        public float slowMotionTimeScale = 0.3f;
    
        [Space]
        public float trajLineLength = 30f;
        [SerializeField] private int segmentCount = 20;

        public bool useSling;

        // Start is called before the first frame update
        void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();
            _trajLine = GetComponent<LineRenderer>();
            _trajLine.enabled = false;
            _camera = FindObjectOfType<Camera>();
            _playerController = GetComponent<PlayerController>();
        
        
            launchForce = GameMode.instance.launchForce * 10f;
            slowMotionTimeScale = GameMode.instance.slowMotionTimeScale;
        }

        // Update is called once per frame
        void Update()
        {
            if (!GameMode.instance.isPaused && !QuickJumpAbility.isActivated)
            {
                if (Input.touchSupported)
                {
                    //Touch mode
                    var finger = Input.GetTouch(0);
                    if (Input.touchCount == 1 && (finger.phase == TouchPhase.Began || finger.phase == TouchPhase.Moved))
                    {
                        //on touch begin --> display trajectory
                        Time.timeScale = slowMotionTimeScale;
                        ChooseTrajectory(true);
                    }
                    else if (finger.phase == TouchPhase.Ended)
                    {
                        //on touch end --> launch
                        _launch = true;
                        Time.timeScale = 1f;
                        _trajLine.enabled = false;
                    }
                }
                else
                {
                    //mouse mode
                    if (_playerController.CurrentHealth > 0)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            //On click --> display trajectory
                            Time.timeScale = slowMotionTimeScale;
                            ChooseTrajectory(false);
                        }
                        else if (Input.GetMouseButtonUp(0))
                        {
                            //on release --> launch
                            _launch = true;
                            Time.timeScale = 1f;
                        }
                        else
                        {
                            if (_trajLine.enabled) _trajLine.enabled = false;
                        }
                    }
                    else
                    {
                        //hide trajectory
                        if (_trajLine.enabled) _trajLine.enabled = false;
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            //launch in fixed-update, due to physics manipulation
            if (_launch)
            {
                LaunchMeteor();
                _launch = false;
            }
        }

        void ChooseTrajectory(bool isTouch)
        {
            var transform1 = transform;
            
            //enable trajectory line renderer
            if (!_trajLine.enabled) _trajLine.enabled = true;
            
            //get touch or mouse position
            if (isTouch)
                _mousePos = _camera.ScreenToWorldPoint(Input.touches[0].position);
            else
                _mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            
            //calculate the direction from the player to the mouse/touch position
            Vector2 difference = _mousePos - transform.position;
            Vector2 direction = difference.normalized;
            if (useSling) direction = -direction;
            
            //divide line length into segments
            Vector2[] segments = new Vector2[segmentCount];
            float segmentScale = trajLineLength / segmentCount;
 
            // The first line point is on the player
            segments[0] = transform1.position;
        
            // The initial velocity
            Vector2 segVelocity = launchForce * Time.fixedDeltaTime * direction;

            for (int i = 1; i < segmentCount; i++)
            {
                // Time it takes to traverse one segment of length segScale (careful if velocity is zero)
                float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;

                // Add velocity from gravity for this segment's timestep
                segVelocity = segVelocity + segTime * _rb2d.gravityScale * Physics2D.gravity;
                //set the next position to the last one plus v*t
                segments[i] = segments[i - 1] + segVelocity * segTime;
            }
            
            //set position of line segments
            _trajLine.positionCount = segmentCount;
            for (int i = 0; i < segmentCount; i++)
                _trajLine.SetPosition(i, segments[i]);
        }

        void LaunchMeteor()
        {
            //launch in direction with speed
            _rb2d.velocity = Vector2.zero;
            Vector2 direction = _mousePos - transform.position;
            _rb2d.AddForce(launchForce * direction.normalized);
            _playerController.AddHealth(-5f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Planet"))
            {
                //slow the player down + give upwards force
                _rb2d.velocity = new Vector2(_rb2d.velocity.x, Mathf.Abs(_rb2d.velocity.y)) * 0.5f;
                _rb2d.AddForce(Vector2.up * 1000f);
                
                //Hit Event
                if(_playerController.isInvincible)
                    other.gameObject.GetComponent<PlanetBehavior>().ProjectileHit(_playerController);
                else
                    other.gameObject.GetComponent<PlanetBehavior>().Hit(_playerController);
            } 
            else if (other.gameObject.CompareTag("Projectile"))
            {
                //die when hit by enemy projectile
                Destroy(other.gameObject);
                _playerController.Die();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Planet") && _playerController.isInvincible)
            {
                collision.gameObject.GetComponent<PlanetBehavior>().ProjectileHit(_playerController);
            }
        }
    }
}
