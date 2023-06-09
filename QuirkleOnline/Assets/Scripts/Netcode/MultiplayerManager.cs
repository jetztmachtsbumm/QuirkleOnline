using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : NetworkBehaviour
{

    public static MultiplayerManager Instance { get; private set; }

    private List<BrickData> drawableBricks;
    private Dictionary<string, int> playerScores;
    private ulong clientInTurn;
    private int clientsReady;
    private string playerName;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("There is more than one MultiplayerManager object active in the scene!");
            Destroy(gameObject);
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        drawableBricks = new List<BrickData>();
        playerName = PlayerPrefs.GetString("PlayerName", "Player#" + Random.Range(1000, 10000));
    }

    public void StartHost()
    {
        NetworkManager.StartHost();
        playerScores = new Dictionary<string, int>();
    }

    public void StartClient()
    {
        NetworkManager.StartClient();
    }

    public void StartGame()
    {
        if (!IsHost) return;

        LobbyManager.Instance.DeleteLobby();

        FillDrawableBricksClientRpc();
        DrawBricks();

        GridSystem.Instance.InitializeGrid();

        SetClientInTurnClientRpc(NetworkManager.LocalClientId);
        PlaceFirstBrick();
    }

    private void DrawBricks()
    {
        for (int i = 0; i < NetworkManager.ConnectedClientsIds.Count; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                DrawBrickClientRpc(NetworkManager.ConnectedClientsIds[i]);
            }
        }
    }

    private void PlaceFirstBrick()
    {
        GridCell gridCell = GridSystem.Instance.GetGridCellAtWorldPosition(new Vector3(50, 0, 50));
        BrickGhost.Instance.SetBrickDataServerRpc(GameManager.Instance.GetAvailableBricks()[0]);
        BrickGhost.Instance.PlaceBrickServerRpc(gridCell, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DrawBrickServerRpc(ulong clientId)
    {
        if (drawableBricks.Count > 0)
        {
            DrawBrickClientRpc(clientId);
        }
        else
        {
            NetworkManager.SceneManager.LoadScene("GameOverScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    [ClientRpc]
    private void DrawBrickClientRpc(ulong clientId)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            BrickData brick = drawableBricks[Random.Range(0, drawableBricks.Count)];
            GameManager.Instance.DrawBrick(brick);
            RemoveDrawableBrickServerRpc(drawableBricks.IndexOf(brick));
        }
        Debug.Log(drawableBricks.Count);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveDrawableBrickServerRpc(int index)
    {
        RemoveDrawableBrickClientRpc(index);
    }

    [ClientRpc]
    private void RemoveDrawableBrickClientRpc(int index)
    {
        drawableBricks.RemoveAt(index);
    }

    [ClientRpc]
    private void FillDrawableBricksClientRpc()
    {
        for(int brickQuantity = 0; brickQuantity < 1; brickQuantity++)
        {
            for(int shape = 1; shape < 7; shape++)
            {
                BrickData.BrickShape brickShape = (BrickData.BrickShape)shape;
                for (int color = 1; color < 7; color++)
                {
                    BrickData.BrickColor brickColor = (BrickData.BrickColor)color;

                    drawableBricks.Add(new BrickData(brickShape, brickColor));
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncreaseClientsReadyServerRpc()
    {
        clientsReady++;

        if(clientsReady == NetworkManager.ConnectedClients.Count)
        {
            StartGame();
        }
    }

    [ClientRpc]
    private void SetClientInTurnClientRpc(ulong clientId)
    {
        clientInTurn = clientId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void NextPlayerTurnServerRpc()
    {
        ulong nextClientInTurn = 0;

        for(int index = 0; index < NetworkManager.ConnectedClientsIds.Count; index++)
        {
            if (NetworkManager.ConnectedClientsIds[index] == clientInTurn)
            {
                if(index == NetworkManager.ConnectedClientsIds.Count - 1)
                {
                    nextClientInTurn = NetworkManager.ConnectedClientsIds[0];
                }
                else
                {
                    nextClientInTurn = NetworkManager.ConnectedClientsIds[index + 1];
                }
            }
        }

        SetClientInTurnClientRpc(nextClientInTurn);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerScoreServerRpc(string playerName, int score)
    {
        playerScores[playerName] = score;
    }

    public bool IsClientInTurn()
    {
        return clientInTurn == NetworkManager.LocalClientId;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string playerName)
    {
        this.playerName = playerName;
        PlayerPrefs.SetString("PlayerName", playerName);
    }

    public Dictionary<string, int> GetPlayerScores()
    {
        return playerScores;
    }

}
