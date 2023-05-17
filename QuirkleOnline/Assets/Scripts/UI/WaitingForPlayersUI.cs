using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitingForPlayersUI : NetworkBehaviour 
{

    public static bool IsWaitingForPlayers { get; private set; }

    [SerializeField] private Button leaveButton;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI lobbyCodeText;
    [SerializeField] private TextMeshProUGUI joinedPlayersText;
    [SerializeField] private TextMeshProUGUI joinMessageText;

    private void Awake()
    {
        leaveButton.onClick.AddListener(() =>
        {
            LobbyManager.Instance.LeaveLobby();
            NetworkManager.Shutdown();
            SetJoinedPlayersTextServerRpc(true);
            AddJoinMessageServerRpc(MultiplayerManager.Instance.GetPlayerName(), false);
            IsWaitingForPlayers = false;
            SceneManager.LoadScene("MainMenu");
        });
        IsWaitingForPlayers = true;
    }

    private void Start()
    {
        if (!MultiplayerManager.Instance.NetworkManager.IsHost)
        {
            Destroy(startButton.gameObject);
        }
        else
        {
            startButton.onClick.AddListener(() =>
            {
                MultiplayerManager.Instance.NetworkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
            });
        }

        lobbyNameText.text = LobbyManager.Instance.GetJoinedLobby().Name;
        lobbyCodeText.text = LobbyManager.Instance.GetJoinedLobby().LobbyCode;

        SetJoinedPlayersTextServerRpc(false);
        AddJoinMessageServerRpc(MultiplayerManager.Instance.GetPlayerName(), true);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetJoinedPlayersTextServerRpc(bool leftGame)
    {
        if (leftGame)
        {
            SetJoinedPlayersTextClientRpc(NetworkManager.ConnectedClients.Count - 1);
        }
        else
        {
            SetJoinedPlayersTextClientRpc(NetworkManager.ConnectedClients.Count);
        }
    }

    [ClientRpc]
    private void SetJoinedPlayersTextClientRpc(int playerCount)
    {
        joinedPlayersText.text = "Players: " + playerCount + "/" + LobbyManager.Instance.GetJoinedLobby().MaxPlayers;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddJoinMessageServerRpc(string playerName, bool joined)
    {
        AddJoinMessageClientRpc(playerName, joined);
    }

    [ClientRpc]
    private void AddJoinMessageClientRpc(string playerName, bool joined)
    {
        if (joined)
        {
            if(joinMessageText.text == "")
            {
                joinMessageText.text += playerName + " joined the game!";
            }
            else
            {
                joinMessageText.text += "\n" + playerName + " joined the game!";
            }
        }
        else
        {
            joinMessageText.text += "\n" + playerName + " left the game!";
        }
    }

}
