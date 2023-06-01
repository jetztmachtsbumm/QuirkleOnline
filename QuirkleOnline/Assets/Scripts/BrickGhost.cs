using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrickGhost : NetworkBehaviour
{

    public static BrickGhost Instance { get; private set; }

    [SerializeField] private Image brickImage;

    private Transform brickPrefab;
    private BrickData brickData;
    private BrickVisualCreator brickVisualCreator;
    private GridCell lastBrickPlacedThisTurn;
    private GridSystem.PlacementDirection currentPlacementDirection;
    private bool isFirstBrickPlacedThisTurn = true;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("There is more than one BrickGhost object active in the scene!");
            Destroy(gameObject);
        }
        Instance = this;

        brickPrefab = Resources.Load<Transform>("BrickTemplate");
        brickVisualCreator = Resources.Load<BrickVisualCreator>("BrickVisualCreator");
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
        if (MultiplayerManager.Instance.IsClientInTurn())
        {
            if (GridSystem.Instance.IsGridCellOccupied(gridCell))
            {
                return;
            }

            if (!GridSystem.Instance.GetValidCells(brickData, isFirstBrickPlacedThisTurn, gridCell, lastBrickPlacedThisTurn, currentPlacementDirection).Contains(gridCell) && !firstTurn)
            {
                return;
            }

            Vector3 placePosition = GridSystem.Instance.GetWorldPositionOfGridCell(gridCell);

            if (currentPlacementDirection == GridSystem.PlacementDirection.NONE)
            {
                currentPlacementDirection = GridSystem.Instance.GetPlacementDirection(lastBrickPlacedThisTurn, gridCell);
            }

            SpawnBrickVisualServerRpc(placePosition);

            UpdateGridCellServerRpc(gridCell);

            if (!firstTurn)
            {
                isFirstBrickPlacedThisTurn = false;
            }

            GameManager.Instance.RemoveBrick(brickData);
            GameManager.Instance.IncreaseScore(GridSystem.Instance.CalculateScore(gridCell));
            GameManager.Instance.SetIsBrickSelected(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBrickVisualServerRpc(Vector3 spawnPosition)
    {
        SpawnBrickVisualClientRpc(spawnPosition);
    }

    [ClientRpc]
    private void SpawnBrickVisualClientRpc(Vector3 spawnPosition)
    {
        Transform spawnedBrick = Instantiate(brickPrefab, spawnPosition, Quaternion.identity);
        spawnedBrick.GetComponent<Brick>().SetBrickData(brickData);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateGridCellServerRpc(GridCell gridCell)
    {
        UpdateGridCellClientRpc(gridCell);
    }

    [ClientRpc]
    private void UpdateGridCellClientRpc(GridCell gridCell)
    {
        gridCell.SetBrickData(brickData);
        gridCell.SetIsOccupied(true);

        GridSystem.Instance.UpdateGridCell(gridCell);

        if (MultiplayerManager.Instance.IsClientInTurn())
        {
            lastBrickPlacedThisTurn = gridCell;
        }
    }

    private void UpdateVisual()
    {
        brickVisualCreator.UpdateVisual(brickImage, brickData.GetBrickShape(), brickData.GetBrickColor());
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

    public void SetupNextTurn()
    {
        currentPlacementDirection = GridSystem.PlacementDirection.NONE;
        isFirstBrickPlacedThisTurn = true;
    }

}
