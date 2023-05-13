using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    public event EventHandler OnAvailableBricksChanged;

    private List<BrickData> availableBricks;
    private bool isBrickSelected;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("There is more than one GameManager object active in the scene!");
            Destroy(gameObject);
        }
        Instance = this;

        availableBricks = new List<BrickData>();

        MultiplayerManager.Instance.IncreaseClientsReadyServerRpc();
    }

    private void Update()
    {
        if (MultiplayerManager.Instance.IsClientInTurn() && !isBrickSelected)
        {
            BrickGhost.Instance.HideServerRpc();
        }
    }

    public void DrawBrick(BrickData brick)
    {
        availableBricks.Add(brick);
        OnAvailableBricksChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveBrick(BrickData brick)
    {
        availableBricks.Remove(brick);
        OnAvailableBricksChanged?.Invoke(this, EventArgs.Empty);
    }

    public List<BrickData> GetAvailableBricks()
    {
        return availableBricks;
    }

    public void SetIsBrickSelected(bool isBrickSelected)
    {
        this.isBrickSelected = isBrickSelected;
    }

}
