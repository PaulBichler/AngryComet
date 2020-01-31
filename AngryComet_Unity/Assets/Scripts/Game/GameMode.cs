using System.Collections;
using System.Collections.Generic;
using Behaviors;
using Boomlagoon.JSON;
using Generation;
using UnityEngine;

namespace Game
{
    [System.Serializable]
    public struct UpgradeDetails
    {
        //struct that stores information about one upgrade
        public string name;
        public bool enabled;
        public bool oneTime;

        public float[] values;
        public int[] prices;
        public int currentValueIndex;

        public void Upgrade()
        {
            if (!enabled)
            {
                enabled = true;
            }
            else if (!oneTime && currentValueIndex + 1 != values.Length)
            {
                currentValueIndex++;
            }
        }
    }
    
    //GameMode class controls the state of the game + keeps player and enemy information
    public class GameMode : MonoBehaviour
    {
        //Singleton Class
        public static GameMode instance = null;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
            }
        }

        [Space] [Header("Player Settings")] 
        public float maxPlayerHealth = 100f;
        public float depleteRatePerSec = 20f;
        [Range(0, 2)] public float slowMoDepleteRateMultiplier = 1.5f;
        public float launchForce = 250f;
        public float slowMotionTimeScale = 0.3f;
        public int startMultiplier = 0;
        public int maxMultiplier = 10;
        public float multiplierDuration = 4f;
        public GameObject playerDeathEffect;
        public AudioClip playerDeathSound;

        [Space] [Header("Enemy Settings")] 
        public GameObject pointsPopup;
        public GameObject enemyDeathEffect;
        public AudioClip planetDeathSound;
        public Sprite happyFace;
        public Sprite worriedFace;
        
        [Space] public UpgradeDetails[] upgrades;
        public AudioClip[] backgroundMusic;
        [Range(500, 20000)] public float menuLowPassValue = 1000;

        [HideInInspector] public bool isPaused = true;
        [HideInInspector] public bool isGameOver = false;
        
        public PlayerController PlayerController { get; private set; }
        private AudioSource _audioSource; 
        private AudioLowPassFilter _audioLowPassFilter;

        // Start is called before the first frame update
        void Start()
        {
            PlayerController = FindObjectOfType<PlayerController>();
            _audioSource = GetComponent<AudioSource>();
            _audioLowPassFilter = GetComponent<AudioLowPassFilter>();
            
            PlayRandomBackgroundMusic();
            
            //pause game in UI mode
            PauseGame();
        }

        public void InitGame(bool isRestart = false)
        {
            UiEventHandler.instance.SetUserInfoVisibility(false, false); //hides user info in top corners
            isGameOver = false;
            UpdateStats();
            PlayerController.gameObject.SetActive(true);
            UnpauseGame();

            if (isRestart)
            {
                PlayRandomBackgroundMusic();
            }
            
            PlayerController.ResetPlayer();
            LevelGeneration.instance.RestartGeneration();
        }

        public void PauseGame()
        {
            Time.timeScale = 0f;
            Physics2D.autoSimulation = false;
            isPaused = true;
            
            //muffles the audio if not in game (paused)
            _audioLowPassFilter.enabled = true;
            _audioLowPassFilter.cutoffFrequency = menuLowPassValue;
        }

        public void UnpauseGame()
        {
            Time.timeScale = 1f;
            Physics2D.autoSimulation = true;
            isPaused = false;
            
            //un-muffles the audio
            _audioLowPassFilter.enabled = false;
        }

        public void EndGame(int score, int coins)
        {
            //muffles the audio if not in game (paused)
            _audioLowPassFilter.enabled = true;
            _audioLowPassFilter.cutoffFrequency = menuLowPassValue;
            
            //Sets the death screen information
            string notification = "";
            isGameOver = true;
            Player.instance.coins += coins;

            if (score > Player.instance.highscore)
            {
                Player.instance.highscore = score;
                notification = "New Highscore! Hurray!";
            }

            UiEventHandler.instance.ShowDeathScreen(score, coins, Player.instance.highscore, notification);
            UiEventHandler.instance.UpdateUserInfoPanel();
            Player.instance.StartCoroutine(Player.instance.Save());
        }

        private void UpdateStats()
        {
            maxPlayerHealth = (upgrades[4].enabled)
                ? upgrades[4].values[upgrades[4].currentValueIndex]
                : maxPlayerHealth;

            launchForce = (upgrades[2].enabled)
                ? upgrades[2].values[upgrades[2].currentValueIndex]
                : launchForce;

            slowMotionTimeScale = (upgrades[3].enabled)
                ? upgrades[3].values[upgrades[3].currentValueIndex]
                : slowMotionTimeScale;

            maxMultiplier = (upgrades[5].enabled)
                ? (int) upgrades[5].values[upgrades[5].currentValueIndex]
                : maxMultiplier;
        }

        public void SetUpgrades(JSONArray jsonUpgrades)
        {
            if (Player.instance.IsLoggedIn)
            {
                foreach (var val in jsonUpgrades)
                {
                    bool isEnabled = val.Obj.GetBoolean("enabled");
                    if (!isEnabled) continue;

                    int upgradeIndex = (int) val.Obj.GetNumber("upgradeIndex");
                    int valueIndex = (int) val.Obj.GetNumber("valueIndex");

                    upgrades[upgradeIndex].enabled = true;
                    upgrades[upgradeIndex].currentValueIndex = valueIndex;
                }
            }
        }

        public void ResetUpgrades()
        {
            for (int i = 0; i < upgrades.Length; i++)
            {
                upgrades[i].enabled = false;
                upgrades[i].currentValueIndex = 0;
            }
        }

        public void PlayRandomBackgroundMusic()
        {
            if (backgroundMusic.Length > 0) 
                _audioSource.clip = backgroundMusic[Random.Range(0, backgroundMusic.Length - 1)];
            _audioSource.Play();
        }
    }
}