using System;
using System.Collections;
using Boomlagoon.JSON;
using UnityEngine;
using UnityEngine.Networking;

namespace Game
{
    public class Player : MonoBehaviour
    {
        //Singleton Class
        public static Player instance = null;

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
        
        [Header("Server Info")]
        [SerializeField] private string serverIp = null;

        [Space] [Header("Player Info")]
        public string username = "";
        public int coins = 0;
        public int highscore = 0;
        public string PlayerId { get; private set; }
        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            private set { 
                //when the value changes, update UI Elements to display the user information
                _isLoggedIn = value;
                Game.UiEventHandler.instance.UpdateUserInfoPanel();
                Game.UiEventHandler.instance.SwitchLogInUi(_isLoggedIn);
            }
        }
        
        [HideInInspector] public string token = "";
        private UiEventHandler _uiEvent;

        private void Start()
        {
            _uiEvent = UiEventHandler.instance;
        }

        public IEnumerator Register(string user, string password)
        {
            //show loading bar
            _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(true, "Registering you..."));
            
            //prepare request body
            string url = serverIp + "/register";
            WWWForm form = new WWWForm();
            form.AddField("name", user);
            form.AddField("password", password);
            form.AddField("highscore", 0);
            form.AddField("coins", 0);
            
            //set request + timeout duration
            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.timeout = 20;
            
            //send request
            yield return request.SendWebRequest();
            
            //check for request errors
            if (request.isNetworkError)
            {
                //disable loading bar + display error pop-up
                _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(false));
                _uiEvent.ShowNotificationPopup("Connection Failed!", Color.red);
            }
            else
            {
                //no connection error
                //disable loading bar
                _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(false));
                
                //retrieve response, parse it to a json-object and retrieve the response code
                string response = request.downloadHandler.text;
                JSONObject json = JSONObject.Parse(response);
                int code = (int)json.GetNumber("code");
                
                //check response code
                if (code == 0)
                {
                    _uiEvent.ShowNotificationPopup("Successfully Registered!", Color.green);
                    //Logs the player in immediately after registering
                    StartCoroutine(Login(user, password));
                } 
                else if (code == 11000)
                    _uiEvent.ShowNotificationPopup("Username already exists!", Color.red);
                else
                    _uiEvent.ShowNotificationPopup("Unexpected error occurred!", Color.red);
            }
        }

        public IEnumerator Login(string user, string password)
        {
            //Display loading bar
            _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(true, "Logging you in..."));
            
            //prepare request body
            string url = serverIp + "/login";
            WWWForm form = new WWWForm();
            form.AddField("name", user);
            form.AddField("password", password);
            
            //set request + timeout duration
            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.timeout = 20;
            
            //send request
            yield return request.SendWebRequest();
            
            //check for request errors
            if (request.isNetworkError)
            {
                //disable loading bar + display error pop-up
                _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(false));
                _uiEvent.ShowNotificationPopup("Connection Failed!", Color.red);
            }
            else
            {
                //retrieve response, parse it to a json-object and check if successful
                string response = request.downloadHandler.text;
                JSONObject json = JSONObject.Parse(response);
                bool success = json.GetBoolean("success");

                if (success)
                {
                    //on success --> retrieve Authentication Bearer Token + user ID
                    token = json.GetString("token");
                    PlayerId = json.GetString("_id");
                    
                    //Retrieve the user information (using the user ID)
                    StartCoroutine(GetPlayerInfo());
                }
                else
                {
                    //on error --> disable loading bar + show error pop-up
                    _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(false));
                    _uiEvent.ShowNotificationPopup("Authentication Failed!", Color.red);
                }
            }
        }

        private IEnumerator GetPlayerInfo()
        {
            //create request + set authentication bearer token
            UnityWebRequest request = UnityWebRequest.Get(serverIp + "/player/" + PlayerId);
            request.SetRequestHeader("Authorization", token);
            
            //send request
            yield return request.SendWebRequest();
            
            //check for request errors
            if (request.isNetworkError)
            {
                //disable loading bar + show error pop-up
                _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(false));
                _uiEvent.ShowNotificationPopup("Connection Failed!", Color.red);
            }
            else
            {
                //on success --> retrieve user information
                _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(false));
                string response = request.downloadHandler.text;
                JSONObject json = JSONObject.Parse(response);
                username = json.GetString("name");
                coins += (int)json.GetNumber("coins");
                highscore = (int)json.GetNumber("highscore");
                IsLoggedIn = true;
                
                //Display success pop-up + change UI back to main menu
                _uiEvent.ShowNotificationPopup("Welcome, " + username, Color.green);
                _uiEvent.BackToMenu();
                
                //Set the user's upgrades
                GameMode.instance.SetUpgrades(json.GetArray("upgrades"));
            }
        }

        public IEnumerator Save()
        {
            //save player information on the player's account
            if (IsLoggedIn)
            {
                //display loading bar
                _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(true, "Saving..."));
                
                //Set json request body
                var json = new JSONObject();
                json.Add("coins", coins);
                json.Add("highscore", highscore);
                
                //create json array 
                var upgradesArray = new JSONArray();
                int index = 0;
                //add each upgrade to the array
                foreach (var upgrade in GameMode.instance.upgrades)
                {
                    var jsonA = new JSONObject();
                    jsonA.Add("upgradeIndex", index);
                    jsonA.Add("enabled", upgrade.enabled);
                    jsonA.Add("valueIndex", upgrade.currentValueIndex);

                    upgradesArray.Add(jsonA);
                    index++;
                }
                //add created json array to json request body
                json.Add("upgrades", upgradesArray);
                
                //create request + set authentication bearer token + set content-type to json
                UnityWebRequest request = UnityWebRequest.Put(serverIp + "/player/" + PlayerId, json.ToString());
                request.SetRequestHeader("Authorization", token);
                request.SetRequestHeader("Content-Type", "application/json");
                
                //send request
                yield return request.SendWebRequest();
                
                //check for request errors
                if (request.isNetworkError)
                {
                    //disable loading bar + show error pop-up
                    _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(false));
                    _uiEvent.ShowNotificationPopup("Could not save! Connection Failed!", Color.red);
                }
                else
                {
                    //disable loading bar
                    _uiEvent.StartCoroutine(_uiEvent.SetLoadingBar(false));
                }
            }
        }

        public void Logout()
        {
            //save before logout
            StartCoroutine(Save());
            
            //reset player info
            username = "";
            coins = 0;
            highscore = 0;
            PlayerId = "";
            token = "";
            IsLoggedIn = false;
            GameMode.instance.ResetUpgrades();
            _uiEvent.ShowNotificationPopup("Logout successful!", Color.green);
        }

        private void OnApplicationQuit()
        {
            //save before quitting application
            StartCoroutine(Save());
        }
    }
}
