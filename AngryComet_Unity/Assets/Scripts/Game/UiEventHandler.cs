using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace Game
{
    [System.Serializable]
    public struct UiUpgrade
    {
        public string refName;
        public int upgradeIndex;
        public TextMeshProUGUI nameField;
        public Button upgradeButton;
        public TextMeshProUGUI priceField;
        public Transform progressPanel;
        public GameObject completedMark;
        public GameObject failedMark;
    }

    public class UiEventHandler : MonoBehaviour
    {
        public static UiEventHandler instance = null;

        void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
                //DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                instance = this;
            }
        }
        
        [Header("User Info Panel")]
        [SerializeField] private GameObject uipUsername = null;
        [SerializeField] private GameObject uipCoins = null;

        [Space] [Header("Loading Bar + Notification")] 
        [SerializeField] private GameObject dimPanel = null;
        [SerializeField] private GameObject loadingCircleBar = null;
        [SerializeField] private TextMeshProUGUI loadingCircleBarText = null;
        private bool _isLoading;
        [SerializeField] private GameObject notificationPopup = null;
        private float _notificationHeight;
        private Coroutine _currentNotificationCoroutine = null;
        
        [Space] [Header("HUD")] 
        [SerializeField] private GameObject hudPanel = null;
        [SerializeField] private Button pauseButton = null;

        [Space] [Header("Main Menu")] 
        [SerializeField] private GameObject mainMenuPanel = null;
        [SerializeField] private Button playButton, upgradesButton, loginMenuButton, creditsButton;
        
        [Space] [Header("Credits Screen")] 
        [SerializeField] private GameObject creditsPanel = null;
        [SerializeField] private Button creditsBackButton;

        [Space] [Header("Pause Menu")] 
        [SerializeField] private GameObject pauseMenuPanel = null;
        [SerializeField] private TextMeshProUGUI pauseCurrentScore, pauseCurrentCash;
        [SerializeField] private Button resumeButton, homeButton;

        [Space] [Header("Death Screen")] 
        [SerializeField] private GameObject deathScreenPanel = null;
        [SerializeField] private Button restartButton, homeButton2;
        [SerializeField] private TextMeshProUGUI scoreTextField, coinsTextField, highscoreTextField, notificationTextField;

        [Space] [Header("Upgrades Menu")] 
        [SerializeField] private GameObject upgradesMenuPanel = null;
        [SerializeField] private UiUpgrade[] uiUpgrades;
        [SerializeField] private Button upgradesBackButton, upgradesPlayButton;

        [Space] [Header("Login Menu")] 
        [SerializeField] private GameObject loginMenuPanel = null;
        [SerializeField] private TMP_InputField usrInputField, pwInputField;
        [SerializeField] private Button goToRegisterButton, loginBackButton, loginButton;

        [Space] [Header("Register Menu")] 
        [SerializeField] private GameObject registerMenuPanel = null;
        [SerializeField] private TMP_InputField usrInputFieldReg, pwInputFieldReg, confirmPwInputFieldReg;
        [SerializeField] private Button goToLoginButton, registerBackButton, registerButton;


        // Start is called before the first frame update
        void Start()
        {
            //Notification Popup Setup
            var rectTransform = notificationPopup.GetComponent<RectTransform>();
            _notificationHeight = rectTransform.rect.height;
            rectTransform.offsetMin = new Vector2(0, _notificationHeight);
            rectTransform.offsetMax = new Vector2(0, _notificationHeight);

            //HUD
            pauseButton.onClick.AddListener(PauseGame);

            //Main Menu
            playButton.onClick.AddListener(StartGame);
            upgradesButton.onClick.AddListener(ShowUpgrades);
            loginMenuButton.onClick.AddListener(ShowLogin);
            creditsButton.onClick.AddListener(ShowCredits);
            
            //Credits Screen
            creditsBackButton.onClick.AddListener(BackToMenu);
            
            //Pause Menu
            resumeButton.onClick.AddListener(ResumeGame);
            homeButton.onClick.AddListener(BackToMenu);

            //Death Screen
            restartButton.onClick.AddListener(RestartGame);
            homeButton2.onClick.AddListener(BackToMenu);

            //Upgrades Menu
            upgradesBackButton.onClick.AddListener(BackToMenu);
            upgradesPlayButton.onClick.AddListener(StartGame);

            foreach (UiUpgrade uiUpgrade in uiUpgrades)
            {
                //Add click listener to all upgrade buttons
                uiUpgrade.upgradeButton.onClick.AddListener(() => Upgrade(uiUpgrade));
            }

            //Login Menu
            goToRegisterButton.onClick.AddListener(ShowRegister);
            loginBackButton.onClick.AddListener(BackToMenu);
            loginButton.onClick.AddListener(AttemptLogin);

            //Register Menu
            goToLoginButton.onClick.AddListener(ShowLogin);
            registerBackButton.onClick.AddListener(BackToMenu);
            registerButton.onClick.AddListener(AttemptRegister);
            
            //get and show user information 
            SetUserInfoVisibility(true, true);
            UpdateUserInfoPanel();
        }

        void PauseGame()
        {
            //Freeze the game and display the pause menu
            GameMode.instance.PauseGame();
            pauseMenuPanel.SetActive(true);
            
            pauseCurrentScore.SetText(GameMode.instance.PlayerController.CurrentScore.ToString());
            pauseCurrentCash.SetText(GameMode.instance.PlayerController.CurrentCoins + " $");
        }

        void StartGame()
        {
            //Initialise the game and hide the main and upgrades menu
            mainMenuPanel.SetActive(false);
            upgradesMenuPanel.SetActive(false);
            hudPanel.SetActive(true);
            GameMode.instance.InitGame();
        }

        void ShowUpgrades()
        {
            //upgrades are only available if player is logged in (removed for git version)
            //if (Player.instance.IsLoggedIn)
            //{
                //Display the Upgrades menu and hide the main menu
                mainMenuPanel.SetActive(false);
                upgradesMenuPanel.SetActive(true);
                SetUserInfoVisibility(false, true);
                
                //Reload upgrade icons and progress
                ReloadUiUpgrades();
            /*}
            else
            {
                ShowNotificationPopup("Please log-in to access the upgrades!", Color.red);
            }*/
        }

        void ShowLogin()
        {
            //Display the Login Form and hide the main menu
            mainMenuPanel.SetActive(false);
            registerMenuPanel.SetActive(false);
            loginMenuPanel.SetActive(true);
            
            //hide top user info panel
            SetUserInfoVisibility(false, false);
        }

        void ShowCredits()
        {
            //Display the Credits-Screen and hide the Main Menu
            mainMenuPanel.SetActive(false);
            creditsPanel.SetActive(true);
        }

        void ShowRegister()
        {
            //Display the Register form and hide the previous panels
            mainMenuPanel.SetActive(false);
            loginMenuPanel.SetActive(false);
            registerMenuPanel.SetActive(true);
            
            //hide top user info panel
            SetUserInfoVisibility(false, false);
        }

        void ResumeGame()
        {
            //Unfreeze the game and hide the pause menu
            GameMode.instance.UnpauseGame();
            pauseMenuPanel.SetActive(false);
        }

        public void BackToMenu()
        {
            //Reset the game and display the Main Menu
            GameMode.instance.PauseGame();
            //hide every panel
            pauseMenuPanel.SetActive(false);
            upgradesMenuPanel.SetActive(false);
            loginMenuPanel.SetActive(false);
            creditsPanel.SetActive(false);
            registerMenuPanel.SetActive(false);
            deathScreenPanel.SetActive(false);
            
            //display main menu + display top user info panel
            mainMenuPanel.SetActive(true);
            SetUserInfoVisibility(true, true);
        }

        public void ShowDeathScreen(int score, int coins, int highscore, string notification)
        {
            //display death screen + top user info panel
            hudPanel.SetActive(false);
            deathScreenPanel.SetActive(true);
            SetUserInfoVisibility(true, true);
            
            //set coins text color
            coinsTextField.color = Color.yellow;
            
            //set death screen texts
            scoreTextField.text = score.ToString();
            coinsTextField.text = "+" + coins + " $";
            highscoreTextField.text = highscore.ToString();
            notificationTextField.text = notification;
        }

        void RestartGame()
        {
            //hide death screen + show hud + initialize a new game
            deathScreenPanel.SetActive(false);
            hudPanel.SetActive(true);
            GameMode.instance.InitGame(true);
        }

        void Upgrade(UiUpgrade upgrade)
        {
            //Upgrade
            GameMode gm = GameMode.instance;
            int price;
            
            if(gm.upgrades[upgrade.upgradeIndex].enabled)
                price = gm.upgrades[upgrade.upgradeIndex].prices[gm.upgrades[upgrade.upgradeIndex].currentValueIndex + 1];
            else
                price = gm.upgrades[upgrade.upgradeIndex].prices[gm.upgrades[upgrade.upgradeIndex].currentValueIndex];

            
            if (Player.instance.coins >= price)
            {
                Player.instance.coins -= price;
                GameMode.instance.upgrades[upgrade.upgradeIndex].Upgrade(); //this does the actual upgrade
                UpdateUiUpgrade(upgrade);
                UpdateUserInfoPanel();
                Player.instance.StartCoroutine(Player.instance.Save()); //Save Player
            }
            else
            {
                UpdateUiUpgrade(upgrade, false);   
            }
        }

        void UpdateUiUpgrade(UiUpgrade upgrade, bool success = true)
        {
            if (!success)
            {
                StartCoroutine(UpgradeFailedFeedback(upgrade));
                ShowNotificationPopup("Not enough Cash to upgrade!", Color.red);
            }

            UpgradeDetails upgradeDetails = GameMode.instance.upgrades[upgrade.upgradeIndex];

            upgrade.nameField.text = upgradeDetails.name;

            if (upgradeDetails.enabled)
            {
                if(upgradeDetails.oneTime)
                    upgrade.priceField.text = upgradeDetails.prices[upgradeDetails.currentValueIndex].ToString();
                else
                    upgrade.priceField.text = upgradeDetails.prices[upgradeDetails.currentValueIndex + 1].ToString();
                
                if (upgradeDetails.oneTime)
                {
                    //show completed
                    upgrade.priceField.text = "Unlocked!";
                    upgrade.upgradeButton.onClick.RemoveAllListeners();
                    upgrade.upgradeButton.interactable = false;
                    upgrade.completedMark.SetActive(true);
                }
                else
                {
                    //show progress
                    for (int i = 0; i <= upgradeDetails.currentValueIndex; i++)
                    {
                        upgrade.progressPanel.GetChild(i).GetComponent<Image>().color = Color.green;
                    }

                    if (upgradeDetails.currentValueIndex == upgradeDetails.values.Length - 1)
                    {
                        //show completed
                        upgrade.priceField.text = "Unlocked!";
                        upgrade.upgradeButton.onClick.RemoveAllListeners();
                        upgrade.upgradeButton.interactable = false;
                        upgrade.completedMark.SetActive(true);
                    }
                }
            }
            else
            {
                upgrade.priceField.text = upgradeDetails.prices[upgradeDetails.currentValueIndex].ToString();
            }
        }

        void ReloadUiUpgrades()
        {
            foreach (UiUpgrade uiUpgrade in uiUpgrades)
            {
                //reset progress bar
                if (uiUpgrade.progressPanel)
                {
                    foreach (Transform child in uiUpgrade.progressPanel)
                    {
                        child.gameObject.GetComponent<Image>().color = Color.white;
                    }
                }

                uiUpgrade.completedMark.SetActive(false);
                UpdateUiUpgrade(uiUpgrade);
            }
        }

        void AttemptLogin()
        {
            string user = usrInputField.text;
            string password = pwInputField.text;

            if (user.Length == 0 || password.Length == 0)
            {
                ShowNotificationPopup("Please enter a username and a password!", Color.red);
            }
            else
            {
                Player.instance.StartCoroutine(Player.instance.Login(user, password));
            }
        }

        void AttemptRegister()
        {
            string user = usrInputFieldReg.text;
            string password = pwInputFieldReg.text;
            string confPw = confirmPwInputFieldReg.text;

            if (user.Length == 0 || password.Length == 0)
            {
                ShowNotificationPopup("Please enter a username and a password!", Color.red, 2f);
            }
            else if (confPw.Length == 0)
            {
                ShowNotificationPopup("Please confirm your password!", Color.red, 2f);
            }
            else if (password != confPw)
            {
                ShowNotificationPopup("Password Confirmation is wrong!", Color.red, 2f);
                pwInputFieldReg.text = "";
                confirmPwInputFieldReg.text = "";
            }
            else
            {
                Player.instance.StartCoroutine(Player.instance.Register(user, password));
            }
        }

        public void SwitchLogInUi(bool loggedIn)
        {
            if (loggedIn)
            {
                //show logout button
                loginMenuButton.onClick.RemoveAllListeners();
                loginMenuButton.onClick.AddListener(Player.instance.Logout);
                loginMenuButton.transform.GetChild(0).GetComponent<Text>().text = "LOGOUT";
            }
            else
            {
                //show login button
                loginMenuButton.onClick.RemoveAllListeners();
                loginMenuButton.onClick.AddListener(ShowLogin);
                loginMenuButton.transform.GetChild(0).GetComponent<Text>().text = "LOGIN";
            }
        }
        
        public void SetUserInfoVisibility(bool loginInfo, bool coins)
        {
            uipUsername.SetActive(loginInfo);
            uipCoins.SetActive(coins);
        }

        public void UpdateUserInfoPanel()
        {
            string loginText;

            if (Player.instance.username == "")
            {
                loginText = "You are not logged-in!";
                
                if (Player.instance.coins > 0)
                    loginText += "\nPlease log-in to claim your cash!";
            }
            else
            {
                loginText = "Logged-In as:\n" + Player.instance.username;
            }
                
            uipUsername.GetComponent<TextMeshProUGUI>().SetText(loginText);
            uipCoins.GetComponent<TextMeshProUGUI>().SetText(Player.instance.coins + " $");
        }

        public void ShowNotificationPopup(string message, Color color, float duration = 1f)
        {
            if (_currentNotificationCoroutine == null)
            {
                notificationPopup.SetActive(true);
                TextMeshProUGUI text = notificationPopup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                text.SetText(message);
                text.color = color;

                _currentNotificationCoroutine = 
                    StartCoroutine(NotificationAnim(0.3f, 1f, notificationPopup.GetComponent<RectTransform>()));
            }
        }

        public IEnumerator SetLoadingBar(bool isEnabled, string text = "")
        {
            _isLoading = true;
            if (isEnabled)
            {
                yield return new WaitForSecondsRealtime(.5f);
                if (_isLoading)
                {
                    dimPanel.SetActive(true);
                    loadingCircleBarText.SetText(text);
                    loadingCircleBar.SetActive(true);
                }
                else
                {
                    dimPanel.SetActive(false);
                    loadingCircleBar.SetActive(false);
                }
            }
            else
            {
                _isLoading = false;
                dimPanel.SetActive(false);
                loadingCircleBar.SetActive(false);
            }
        }

        IEnumerator UpgradeFailedFeedback(UiUpgrade upgrade)
        {
            upgrade.failedMark.SetActive(true);
            yield return new WaitForSecondsRealtime(.25f);
            upgrade.failedMark.SetActive(false);
        }

        private IEnumerator NotificationAnim(float animDuration, float duration, RectTransform rectTransform, bool up = false)
        {
            rectTransform.offsetMin = new Vector2(0, _notificationHeight);
            rectTransform.offsetMax = new Vector2(0, _notificationHeight);

            float elapsed = 0.0f;

            while (elapsed < animDuration)
            {
                float height;
                
                if (up)
                    height = Mathf.SmoothStep(0, _notificationHeight, elapsed / animDuration);
                else
                    height = Mathf.SmoothStep(_notificationHeight, 0, elapsed / animDuration);
                
                rectTransform.offsetMin = new Vector2(0, height);
                rectTransform.offsetMax = new Vector2(0, height);
                elapsed += Time.unscaledDeltaTime;
            
                yield return null;
            }

            if (!up)
            {
                yield return new WaitForSecondsRealtime(duration);
                StartCoroutine(NotificationAnim(animDuration, 0, rectTransform, true));
            }
            else
            {
                rectTransform.offsetMin = new Vector2(0, _notificationHeight);
                rectTransform.offsetMax = new Vector2(0, _notificationHeight);
                notificationPopup.SetActive(false);
                _currentNotificationCoroutine = null;
            }
        }
    }
}