using System;
using Game;
using UnityEngine;
using UnityEngine.UI;

namespace Behaviors.Abilities
{
    public class QuickJumpAbility : MonoBehaviour
    {
        public static bool isActivated = false;
        [SerializeField] private Button activateButton = null;
        [SerializeField] private Image cooldownDisplay = null;
        [SerializeField] private float cooldown = 30f;
        [SerializeField] private float abilityDuration = 5f;
        [SerializeField] private float playerSpeed = 100f;

        private PlayerController _playerController;
        private Rigidbody2D _rb2d;
        private float _currentCd;
        private float _currentAd;
        private Transform _target;

        // Start is called before the first frame update
        void Start()
        {
            _playerController = GetComponent<PlayerController>();
            _rb2d = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _currentCd = cooldown;
            _currentAd = abilityDuration;
            
            activateButton.gameObject.SetActive(true);
            activateButton.onClick.AddListener(Activate);
            cooldownDisplay.fillAmount = _currentCd / cooldown;
        }

        private void OnDisable()
        {
            if(activateButton) {
                activateButton.gameObject.SetActive(false);
                activateButton.onClick.RemoveAllListeners();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_currentCd <= 0)
            {
                if (isActivated)
                {
                    _playerController.isInvincible = true;
                    _rb2d.isKinematic = true;
                    
                    if (_currentAd > 0)
                    {
                        if (!_target || MoveToTarget())
                        {
                            _target = GetClosestWithTag("Planet");
                        }

                        _currentAd -= Time.deltaTime;
                    }
                    else
                    {
                        if (!_target || MoveToTarget())
                        {
                            isActivated = false;
                            _playerController.isInvincible = false;
                            _rb2d.isKinematic = false;
                            _currentAd = abilityDuration;
                            _currentCd = cooldown;
                        }
                    }
                }
            }
            else
            {
                _currentCd -= Time.deltaTime;
                cooldownDisplay.fillAmount = _currentCd / cooldown;
            }
        }

        void Activate()
        {
            if (_currentCd <= 0)
            {
                isActivated = true;
                cooldownDisplay.fillAmount = 1f;
                Time.timeScale = 1f;
            }
        }

        bool MoveToTarget()
        {
            var tarPos = _target.position;
            var position = transform.position;

            position = Vector2.MoveTowards(position, tarPos, playerSpeed * Time.deltaTime);

            var transform1 = transform;
            transform1.position = position;

            if (position == tarPos) return true;
            return false;
        }

        Transform GetClosestWithTag(string entityTag)
        {
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

            if (bestTarget != null) return bestTarget.transform;
            return null;
        }
    }
}