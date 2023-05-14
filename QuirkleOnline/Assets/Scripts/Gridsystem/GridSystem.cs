using System;
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
    private Transform gridCellVisual;

    public void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("There is more than one GridSystem object active in the scene!");
            Destroy(gameObject);
        }
        Instance = this;

        gridCellVisual = Resources.Load<Transform>("GridCellVisual");
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
    private void SetCellClientRpc(int x, int z, GridCell gridCell)
    {
        if (cells == null)
        {
            cells = new GridCell[width, height];
        }

        cells[x, z] = gridCell;
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

    public void UpdateGridCell(GridCell gridCell)
    {
        cells[gridCell.GetX(), gridCell.GetZ()] = gridCell;
    }

    public List<GridCell> GetValidCells(BrickData brickData)
    {
        List<GridCell> validCells = new List<GridCell>();

        foreach(GridCell cell in cells)
        {
            if (cell.IsOccupied()) continue;

            List<GridCell> neighbours = GetNeighbours(cell);
            List<GridCell> occupiedNeighbours = new List<GridCell>();

            foreach(GridCell neighbour in neighbours)
            {
                if (neighbour.IsOccupied())
                {
                    occupiedNeighbours.Add(neighbour);
                }
            }

            if (occupiedNeighbours.Count == 0) continue;

            List<GridCell> matchingNeighbours = new List<GridCell>();
            foreach(GridCell occupiedNeighbour in occupiedNeighbours)
            {
                if(BrickData.Match(brickData, occupiedNeighbour.GetBrickData()))
                {
                    matchingNeighbours.Add(occupiedNeighbour);
                }
            }

            if (matchingNeighbours.Count != occupiedNeighbours.Count) continue;
            
            if(!FitsInRow(cell, brickData)) continue;

            validCells.Add(cell);
        }

        return validCells;
    }

    private List<GridCell> GetNeighbours(GridCell gridCell)
    {
        List<GridCell> neighbours = new List<GridCell>();

        if(gridCell.GetX() < cells.GetLength(0) - 1)
        neighbours.Add(cells[gridCell.GetX() + 1, gridCell.GetZ()]);

        if(gridCell.GetX() > 0)
        neighbours.Add(cells[gridCell.GetX() - 1, gridCell.GetZ()]);

        if(gridCell.GetZ() < cells.GetLength(1) - 1)
        neighbours.Add(cells[gridCell.GetX(), gridCell.GetZ() + 1]);

        if(gridCell.GetZ() > 0)
        neighbours.Add(cells[gridCell.GetX(), gridCell.GetZ() - 1]);

        return neighbours;
    }

    private List<GridCell> GetHorizontalBrickRow(GridCell gridCell)
    {
        List<GridCell> brickRow = new List<GridCell>();

        brickRow.Add(gridCell);

        int z = gridCell.GetZ();
        int maximumRowLength = 5;

        for(int x = gridCell.GetX() + 1; x < x + maximumRowLength; x++)
        {
            if (cells[x, z].IsOccupied())
            {
                brickRow.Add(cells[x, z]);
            }
            else
            {
                break;
            }
        }

        for(int x = gridCell.GetX() - 1; x > x - maximumRowLength; x--)
        {
            if (cells[x, z].IsOccupied())
            {
                brickRow.Add(cells[x, z]);
            }
            else
            {
                break;
            }
        }

        return brickRow;
    }

    private List<GridCell> GetVerticalBrickRow(GridCell gridCell)
    {
        List<GridCell> brickRow = new List<GridCell>();

        brickRow.Add(gridCell);

        int x = gridCell.GetX();
        int maximumRowLength = 5;

        for (int z = gridCell.GetZ() + 1; z < z + maximumRowLength; z++)
        {
            if (cells[x, z].IsOccupied())
            {
                brickRow.Add(cells[x, z]);
            }
            else
            {
                break;
            }
        }

        for (int z = gridCell.GetZ() - 1; z > z - maximumRowLength; z--)
        {
            if (cells[x, z].IsOccupied())
            {
                brickRow.Add(cells[x, z]);
            }
            else
            {
                break;
            }
        }

        return brickRow;
    }

    private bool FitsInRow(GridCell gridCell, BrickData brickData)
    {
        List<GridCell> horizontalRow = GetHorizontalBrickRow(gridCell);
        List<GridCell> verticalRow = GetVerticalBrickRow(gridCell);

        bool horizontalEqualShape = true;
        bool horizontalEqualColor = true;
        foreach(GridCell cell in horizontalRow)
        {
            if (cell.GetBrickData().Equals(brickData)) return false;

            if (cell.GetBrickData().GetBrickShape() == BrickData.BrickShape.NONE || cell.GetBrickData().GetBrickColor() == BrickData.BrickColor.NONE) continue;

            horizontalEqualShape = cell.GetBrickData().GetBrickShape() == brickData.GetBrickShape();
            horizontalEqualColor = cell.GetBrickData().GetBrickColor() == brickData.GetBrickColor();
        }

        if (!(horizontalEqualShape || horizontalEqualColor)) return false;


        bool verticalEqualShape = true;
        bool verticalEqualColor = true;
        foreach(GridCell cell in verticalRow)
        {
            if (cell.GetBrickData().Equals(brickData)) return false;

            if (cell.GetBrickData().GetBrickShape() == BrickData.BrickShape.NONE || cell.GetBrickData().GetBrickColor() == BrickData.BrickColor.NONE) continue;

            verticalEqualShape = cell.GetBrickData().GetBrickShape() == brickData.GetBrickShape();
            verticalEqualColor = cell.GetBrickData().GetBrickColor() == brickData.GetBrickColor();
        }

        if (!(verticalEqualShape || verticalEqualColor)) return false;

        return true;
    }

}
