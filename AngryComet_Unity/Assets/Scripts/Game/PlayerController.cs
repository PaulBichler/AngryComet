using System;
using System.Collections;
using System.Collections.Generic;
using Behaviors;
using Behaviors.Abilities;
using Generation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace Game
{
    //PlayerController --> controls the state of the player (updates health, score, multiplier, etc..)
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D _rb2d;
        private Vector3 _startPosition;
        private float _depleteRatePerSec;
        private float _depleteRateMultiplier;
        private float _maxHealth;
        private int _scoreMultiplier;
        
        //Coroutine references to prevent a routine running multiple times at the same time
        private Coroutine _multiplierDurationCoroutine;
        private Coroutine _scoreAnimCoroutine;
        private Coroutine _multiplierAnimCoroutine;
        
        //Current player stats
        public float CurrentHealth { get; private set; }
        public int CurrentScore { get; private set; }
        public int CurrentCoins { get; private set; }
        public float CurrentXp { get; private set; }
        public bool CanKillSun { get; private set; }
        
        [HideInInspector] public bool isInvincible;
        
        //references to UI elements that need to be updates
        [SerializeField] private Image healthBar = null;
        [SerializeField] private TextMeshProUGUI pointsTextField = null;
        [SerializeField] private TextMeshProUGUI multiplierTextField = null;
        private Vector3 _originalScoreScale;
        
        //projectile upgrade
        [SerializeField] private GameObject playerProjectile = null;
        private float _playerProjectileCooldown = -1f;

        // Start is called before the first frame update
        void Start()
        {
            _rb2d = GetComponent<Rigidbody2D>();
            _startPosition = transform.position;
            _originalScoreScale = pointsTextField.transform.localScale;
            ResetPlayer();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateHealth();
        }

        
        private void UpdatePlayerValues()
        {
            //gets all the player information from the GameMode and updates the player state
            //this is used to update the player stats after an upgrade is bought
            GameMode gm = GameMode.instance;
            PlayerMovement mm = GetComponent<PlayerMovement>();
            
            _depleteRatePerSec = gm.depleteRatePerSec;
            _depleteRateMultiplier = gm.slowMoDepleteRateMultiplier;
            _maxHealth = gm.maxPlayerHealth;
            
            _scoreMultiplier = gm.startMultiplier;
            mm.launchForce = gm.launchForce * 10f;
            mm.slowMotionTimeScale = gm.slowMotionTimeScale;
            
            _playerProjectileCooldown = (gm.upgrades[0].enabled) ? 12f : -1;
            GetComponent<QuickJumpAbility>().enabled = gm.upgrades[1].enabled;
            CanKillSun = gm.upgrades[6].enabled;
        }

        public void ResetPlayer()
        {
            //resets the player state to start a new round
            UpdatePlayerValues();
            StartCoroutine(DelayPlayerControl(0.3f));
            transform.position = _startPosition;
            CurrentHealth = _maxHealth;
            healthBar.fillAmount = CurrentHealth / _maxHealth;
            CurrentScore = 0;
            CurrentCoins = 0;
            CurrentXp = 0;
            pointsTextField.text = CurrentScore.ToString();
            
            //Projectile Upgrade
            StartCoroutine(SpawnProjectiles());
        }

        public void Die()
        {
            if (!isInvincible)
            {
                //Disbales player + plays death effects + tells GameMode to end the game
                var transform1 = transform;
                Instantiate(GameMode.instance.playerDeathEffect, transform1.position, transform1.rotation);
                GameMode.instance.EndGame(CurrentScore, CurrentCoins);
                GetComponent<QuickJumpAbility>().enabled = false;
                gameObject.SetActive(false);
            }
        }

        private void UpdateHealth()
        {
            if (healthBar && !GameMode.instance.isPaused && !_rb2d.isKinematic)
            {
                //Calculates the new health based on the depletion-rate and displays it on the health-bar
                float multiplier = (Time.timeScale != 1f) ? _depleteRateMultiplier : 1f;
                CurrentHealth -= _depleteRatePerSec * Time.unscaledDeltaTime * multiplier;
                healthBar.fillAmount = CurrentHealth / _maxHealth;

                if (CurrentHealth <= 0)
                {
                    Die();
                }
            }
        }

        private void UpdateScoreText()
        {
            //Updates the score display + plays the score animation (coroutine)
            if (pointsTextField)
            {
                pointsTextField.text = CurrentScore.ToString();
                
                if (_scoreAnimCoroutine != null)
                {
                    StopCoroutine(_scoreAnimCoroutine);
                    multiplierTextField.transform.localScale = Vector3.one;
                }
                _scoreAnimCoroutine = StartCoroutine(ScoreAnim(0.3f, 2f, pointsTextField));
            }
        }

        private void UpdateMultiplierText()
        {
            //Updates the Multiplier display + plays the multiplier animation (coroutine)
            if (multiplierTextField)
            {
                if (_scoreMultiplier <= 1)
                {
                    multiplierTextField.text = "";
                }
                else
                {
                    multiplierTextField.text = "x" + _scoreMultiplier;
                    
                    if (_multiplierAnimCoroutine != null)
                    {
                        StopCoroutine(_multiplierAnimCoroutine);
                        multiplierTextField.transform.localScale = Vector3.one;
                    }
                    _multiplierAnimCoroutine = StartCoroutine(ScoreAnim(0.3f, 2f, multiplierTextField));
                }
            }
        }

        public void AddHealth(float amount)
        {
            CurrentHealth += amount;
            if (CurrentHealth > _maxHealth) CurrentHealth = _maxHealth;
        }

        public void AddPoints(int amount, bool silent = false)
        {
            //Add points to the score + calculates the multiplier + displays the points pop-up
            int multiplier = (_scoreMultiplier == 0) ? _scoreMultiplier + 1 : _scoreMultiplier;
            CurrentScore += amount * multiplier;
            if(!silent) PointsPopup.Create(transform.position, amount * multiplier);
            if (CurrentScore < 0) CurrentScore = 0;
            
            //Increments the score multiplier if the duration coroutine, is this running 
            if (_scoreMultiplier == 0)
            {
                _scoreMultiplier++;
            } 
            else if (_scoreMultiplier == GameMode.instance.maxMultiplier)
            {
                if(_multiplierDurationCoroutine != null) StopCoroutine(_multiplierDurationCoroutine);
            }
            else
            {
                _scoreMultiplier++;
                if(_multiplierDurationCoroutine != null) StopCoroutine(_multiplierDurationCoroutine);
            }
            
            UpdateScoreText();
            UpdateMultiplierText();
            _multiplierDurationCoroutine = StartCoroutine(MultiplierCountdown());
        }

        public void AddCoins(int amount, bool silent = false)
        {
            //Adds coins + displays coins pop-up if not silent
            CurrentCoins += amount;
            if (!silent)
            {
                var position = transform.position;
                Vector3 popupPos = new Vector3(position.x, position.y - 3f, position.z);
                PointsPopup.Create(popupPos, amount, true);
            }

            if (CurrentCoins < 0) CurrentCoins = 0;
        }

        public void AddXp(float amount)
        {
            CurrentXp += amount;
            LevelGeneration.instance.UpdateLevel(CurrentXp);
        }

        IEnumerator ScoreAnim(float duration, float endScale, TextMeshProUGUI element)
        {
            //Produces a scaling animation of the score and multiplier text
            element.transform.localScale = _originalScoreScale;
            Vector3 originalScale = element.transform.localScale;

            float elapsed = 0.0f;
            float xy = originalScale.x;

            while (elapsed < duration)
            {
                xy += endScale * Time.deltaTime;
            
                element.transform.localScale = new Vector3(xy, xy, originalScale.z);
                elapsed += Time.deltaTime;
            
                yield return null;
            }

            element.transform.localScale = originalScale;
        }

        IEnumerator MultiplierCountdown()
        {
            //resets the multiplier after a duration (is stopped and restarted if another planet is hit within this duration)
            yield return new WaitForSecondsRealtime(GameMode.instance.multiplierDuration);
            _scoreMultiplier = 0;
            UpdateMultiplierText();
        }

        IEnumerator SpawnProjectiles()
        {
            //continuously spawn projectile after cooldown
            while (!GameMode.instance.isGameOver && _playerProjectileCooldown != -1f)
            {
                yield return new WaitUntil(CheckIfNotPaused);
                
                if (!GameMode.instance.isPaused)
                {
                    yield return new WaitForSecondsRealtime(_playerProjectileCooldown);
                    Instantiate(playerProjectile, transform.position, Quaternion.identity);
                }
                else
                {
                    yield return new WaitUntil(CheckIfNotPaused);
                }
            }
        }

        private bool CheckIfNotPaused()
        {
            //used as delegate in Enumerators
            return !GameMode.instance.isPaused;
        }

        IEnumerator DelayPlayerControl(float duration)
        {
            //player can not control the comet for <duration> seconds
            //used when the player is respawned
            _rb2d.isKinematic = true;
            yield return new WaitForSecondsRealtime(duration);
            _rb2d.isKinematic = false;
        }
    }
}
