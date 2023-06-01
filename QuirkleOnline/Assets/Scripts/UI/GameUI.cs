using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    [SerializeField] private Button endTurnButton;
    [SerializeField] private Button drawBrickButton;

    private void Awake()
    {
        endTurnButton.onClick.AddListener(() =>
        {
            MultiplayerManager.Instance.NextPlayerTurnServerRpc();
            BrickGhost.Instance.SetupNextTurn();
        });

        drawBrickButton.onClick.AddListener(() =>
        {
            if (!MultiplayerManager.Instance.IsClientInTurn())
            {
                if(GameManager.Instance.GetAvailableBricks().Count < 6)
                {
                    MultiplayerManager.Instance.DrawBrickServerRpc(MultiplayerManager.Instance.NetworkManager.LocalClientId);
                }
            }
        });
    }

}
