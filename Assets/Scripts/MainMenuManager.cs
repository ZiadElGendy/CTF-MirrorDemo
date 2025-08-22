using System;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CTF
{
    public class MainMenuManager : MonoBehaviour
    {
        public static MainMenuManager Instance { get; private set; }
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TextMeshProUGUI statusText;

        public static string PlayerName { get; private set; }
        private const string PlayerPrefsNameKey = "PlayerName";

        public NetworkManager networkManager;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        private void Start()
        {
            GetInfoFromPlayerPrefs();
            hostButton.onClick.AddListener(OnHostClicked);
            joinButton.onClick.AddListener(OnJoinClicked);
            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }

        private void GetInfoFromPlayerPrefs()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsNameKey)) return;
            nameInputField.text = PlayerPrefs.GetString(PlayerPrefsNameKey);
        }

        private void OnNameChanged(string newName)
        {
            PlayerName = newName.Trim();
            PlayerPrefs.SetString(PlayerPrefsNameKey, PlayerName);
            PlayerPrefs.Save();
        }

        public void OnHostClicked()
        {
            statusText.text = "Starting host...";
            networkManager.StartHost();
        }

        public void OnJoinClicked()
        {
            statusText.text = "Connecting...";
            networkManager.StartClient();
        }
    }
}