using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridSystem : NetworkBehaviour
{

    public static GridSystem Instance { get; private set; }

    [SerializeField] private int width;
    [SerializeField] private int height;

    private GridCell[,] cells;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("There is more than one GridSystem object active in the scene!");
            Destroy(gameObject);
        }
        Instance = this;
    }

    public void InitializeGrid()
    {
        if (!IsHost) return;

        for(int x = 0; x < width; x++)
        {
            for(int z = 0; z < height; z++)
            {
                SetCellClientRpc(x, z, new GridCell(x, z));
            }
        }
    }

    [ClientRpc]
    private void SetCellClientRpc(int x, int z, GridCell cell)
    {
        if (cells == null)
        {
            cells = new GridCell[width, height];
        }

        cells[x, z] = cell;
    }

    public GridCell GetGridCellAtWorldPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x);
        int z = Mathf.RoundToInt(worldPos.z);
        
        if(cells == null)
        {
            return new GridCell();
        }

        return cells[x, z];
    }

    public Vector3 GetWorldPositionOfGridCell(GridCell gridCell)
    {
        return new Vector3(gridCell.GetX(), 0, gridCell.GetZ());
    }

    public Vector3 SnapWorldPositionToGrid(Vector3 worldPos)
    {
        GridCell gridCell = GetGridCellAtWorldPosition(worldPos);
        return GetWorldPositionOfGridCell(gridCell);
    }

    public bool IsGridCellOccupied(GridCell gridCell)
    {
        return gridCell.IsOccupied();
    }

    public void UpdateGridCell(GridCell cell)
    {
        for(int x = 0; x < cells.GetLength(0); x++)
        {
            for(int z = 0; z < cells.GetLength(1); z++)
            {
                if (cells[x, z].Equals(cell))
                {
                    cells[x, z] = cell;
                }
            }
        }
    }

}
