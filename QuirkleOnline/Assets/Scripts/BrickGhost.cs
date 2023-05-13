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

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("There is more than one BrickGhost object active in the scene!");
            Destroy(gameObject);
        }
        Instance = this;
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
    public void PlaceBrickServerRpc(GridCell gridCell)
    {
        PlaceBrickClientRpc(gridCell);
    }

    [ClientRpc]
    private void PlaceBrickClientRpc(GridCell gridCell)
    {
        Transform brickPrefab = Resources.Load<Transform>("BrickTemplate");

        if (GridSystem.Instance.IsGridCellOccupied(gridCell))
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
            MultiplayerManager.Instance.NextPlayerTurnServerRpc();
        }
    }

    private void UpdateVisual()
    {
        shapeText.text = brickData.GetBrickShape().ToString();
        colorText.text = brickData.GetBrickColor().ToString();
    }

}
