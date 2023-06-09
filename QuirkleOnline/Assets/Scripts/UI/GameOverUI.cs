using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : NetworkBehaviour
{

    [SerializeField] private Transform playerScoreTemplate;
    [SerializeField] private Transform scoreboardContainer;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("MainMenu");
        });
    }

    private void Start()
    {
        if (NetworkManager.IsHost)
        {
            for(int i = 0; i < MultiplayerManager.Instance.GetPlayerScores().Count; i++)
            {
                KeyValuePair<string, int> entry = MultiplayerManager.Instance.GetPlayerScores().ElementAt(i);
                CreateScoreboardEntryClientRpc(i + 1, entry.Key, entry.Value);
            }
        }
    }

    [ClientRpc]
    private void CreateScoreboardEntryClientRpc(int place, string playerName, int score)
    {
        Transform playerScoreTransform = Instantiate(playerScoreTemplate, scoreboardContainer);
        playerScoreTransform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text = place + ". " + playerName;
        playerScoreTransform.Find("PlayerScoreText").GetComponent<TextMeshProUGUI>().text = "Score: " + score;
    }

}
