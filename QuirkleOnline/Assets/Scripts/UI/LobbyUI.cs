using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{

    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinCodeButton;
    [SerializeField] private LobbyCreateUI lobbyCreateUI;
    [SerializeField] private TMP_InputField lobbyCodeInputField;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private Transform lobbyContainer;
    [SerializeField] private Transform lobbyTemplate;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });

        createLobbyButton.onClick.AddListener(() =>
        {
            lobbyCreateUI.Show();
        });

        quickJoinButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.QuickJoin();
        });

        joinCodeButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.JoinWithCode(lobbyCodeInputField.text);
        });

        lobbyTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerNameInputField.text = MultiplayerManager.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string playerName) =>
        {
            MultiplayerManager.Instance.SetPlayerName(playerName);
        });

        UpdateLobbyList(new List<Lobby>());
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
    }

    private void OnDestroy()
    {
        LobbyManager.Instance.OnLobbyListChanged -= LobbyManager_OnLobbyListChanged;
    }

    private void LobbyManager_OnLobbyListChanged(object sender, List<Lobby> lobbies)
    {
        UpdateLobbyList(lobbies);
    }

    private void UpdateLobbyList(List<Lobby> lobbies)
    {
        foreach(Transform child in lobbyContainer)
        {
            if (child == lobbyTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in lobbies)
        {
            Transform lobbyTransform = Instantiate(lobbyTemplate, lobbyContainer);
            lobbyTransform.gameObject.SetActive(true);
            lobbyTransform.GetComponent<LobbyUITemplate>().SetLobby(lobby);
        }
    }

}
