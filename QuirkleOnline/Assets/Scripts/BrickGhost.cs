using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class BrickGhost : NetworkBehaviour
{

    public static BrickGhost Instance { get; private set; }

    [SerializeField] private TextMeshPro shapeText;
    [SerializeField] private TextMeshPro colorText;

    private BrickData brickData;
    private List<Transform> validGridCellVisuals;
    private Transform validGridCellVisual;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("There is more than one BrickGhost object active in the scene!");
            Destroy(gameObject);
        }
        Instance = this;

        validGridCellVisuals = new List<Transform>();
        validGridCellVisual = Resources.Load<Transform>("ValidGridCellVisual");
        HideServerRpc();
    }

    private void Update()
    {
        if (MultiplayerManager.Instance.IsClientInTurn())
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                ChangePositionServerRpc(GridSystem.Instance.GetGridCellAtWorldPosition(MouseWorld.Instance.GetMouseWorldPosition()));
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangePositionServerRpc(GridCell gridCell)
    {
        transform.position = GridSystem.Instance.GetWorldPositionOfGridCell(gridCell) + new Vector3(0, 3, 0);
    }

    public BrickData GetBrickData()
    {
        return brickData;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetBrickDataServerRpc(BrickData brickData)
    {
        SetBrickDataClientRpc(brickData);
        ShowValidGridCellsClientRpc();
    }

    [ClientRpc]
    private void SetBrickDataClientRpc(BrickData brickData)
    {
        this.brickData = brickData;
        UpdateVisual();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlaceBrickServerRpc(GridCell gridCell, bool firstTurn = false)
    {
        PlaceBrickClientRpc(gridCell, firstTurn);
    }

    [ClientRpc]
    private void PlaceBrickClientRpc(GridCell gridCell, bool firstTurn)
    {
        Transform brickPrefab = Resources.Load<Transform>("BrickTemplate");

        if (GridSystem.Instance.IsGridCellOccupied(gridCell))
        {
            return;
        }

        if (!GridSystem.Instance.GetValidCells(brickData).Contains(gridCell) && !firstTurn)
        {
            return;
        }

        Vector3 placePosition = GridSystem.Instance.GetWorldPositionOfGridCell(gridCell);

        Transform spawnedBrick = Instantiate(brickPrefab, placePosition, Quaternion.identity);

        spawnedBrick.GetComponent<Brick>().SetBrickData(brickData);

        gridCell.SetBrickData(brickData);
        gridCell.SetIsOccupied(true);

        GridSystem.Instance.UpdateGridCell(gridCell);

        if (MultiplayerManager.Instance.IsClientInTurn())
        {
            GameManager.Instance.RemoveBrick(brickData);
            GameManager.Instance.IncreaseScore(GridSystem.Instance.CalculateScore(gridCell));
            MultiplayerManager.Instance.DrawBrickServerRpc(NetworkManager.LocalClientId);
            MultiplayerManager.Instance.NextPlayerTurnServerRpc();
            GameManager.Instance.SetIsBrickSelected(false);
            HideValidGridCells();
        }
    }

    private void UpdateVisual()
    {
        shapeText.text = brickData.GetBrickShape().ToString();
        colorText.text = brickData.GetBrickColor().ToString();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShowServerRpc()
    {
        ShowClientRpc();
    }

    [ClientRpc]
    private void ShowClientRpc()
    {
        gameObject.SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HideServerRpc()
    {
        HideClientRpc();
    }

    [ClientRpc]
    private void HideClientRpc()
    {
        gameObject.SetActive(false);
    }

    [ClientRpc]
    public void ShowValidGridCellsClientRpc()
    {
        if(!MultiplayerManager.Instance.IsClientInTurn()) return;

        HideValidGridCells();
        validGridCellVisuals = new List<Transform>();
        foreach (GridCell validCell in GridSystem.Instance.GetValidCells(brickData))
        {
            validGridCellVisuals.Add(Instantiate(validGridCellVisual, GridSystem.Instance.GetWorldPositionOfGridCell(validCell), Quaternion.identity, GridSystem.Instance.transform));
        }
    }

    public void HideValidGridCells()
    {
        foreach (Transform visual in validGridCellVisuals)
        {
            Destroy(visual != null ? visual.gameObject : null);
        }
    }

}
