using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerManager : NetworkBehaviour
{
    
    public static MultiplayerManager Instance { get; private set; }

    private List<BrickData> drawableBricks;
    private ulong clientInTurn;

    private int clientsReady;

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
    }

    public void StartHost()
    {
        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        NetworkManager.StartHost();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if(NetworkManager.ConnectedClientsList.Count > 2)
        {
            NetworkManager.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    public void StartClient()
    {
        NetworkManager.StartClient();
    }

    public void StartGame()
    {
        if (!IsHost) return;

        FillDrawableBricksClientRpc();
        for(int i = 0; i < NetworkManager.ConnectedClientsIds.Count; i++)
        {
            for(int j = 0; j < 6; j++)
            {
                DrawBrickClientRpc(NetworkManager.ConnectedClientsIds[i]);
            }
        }

        SetClientInTurnClientRpc(NetworkManager.LocalClientId);
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
        for(int brickQuantity = 0; brickQuantity < 2; brickQuantity++)
        {
            for(int shape = 0; shape < 6; shape++)
            {
                BrickData.BrickShape brickShape = (BrickData.BrickShape)shape;
                for (int color = 0; color < 6; color++)
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

    public bool IsClientInTurn()
    {
        return clientInTurn == NetworkManager.LocalClientId;
    }

}
