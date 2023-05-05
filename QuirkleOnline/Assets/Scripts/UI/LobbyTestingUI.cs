using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTestingUI : MonoBehaviour
{

    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startClientButton;

    private void Awake()
    {
        startHostButton.onClick.AddListener(() => MultiplayerManager.Instance.StartHost());
        startClientButton.onClick.AddListener(() => MultiplayerManager.Instance.StartClient());
    }

}
