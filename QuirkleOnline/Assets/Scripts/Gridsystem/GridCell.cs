using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct GridCell : IEquatable<GridCell>, INetworkSerializable
{

    private int x;
    private int z;
    private BrickData brickData;
    private bool isOccupied;

    public GridCell(int x, int z)
    {
        this.x = x;
        this.z = z;
        brickData = new BrickData();
        isOccupied = false;
    }

    public GridCell(int x, int z, BrickData brickData)
    {
        this.x = x;
        this.z = z;
        this.brickData = brickData;
        isOccupied = false;
    }

    public bool Equals(GridCell other)
    {
        return x == other.x && z == other.z;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref z);
        serializer.SerializeValue(ref brickData);
        serializer.SerializeValue(ref isOccupied);
    }

    public int GetX()
    {
        return x;
    }

    public int GetZ()
    {
        return z;
    }

    public BrickData GetBrickData()
    {
        return brickData;
    }

    public bool IsOccupied()
    {
        return isOccupied;
    }

    public void SetBrickData(BrickData brickData)
    {
        this.brickData = brickData;
    }

    public void SetIsOccupied(bool isOccupied)
    {
        this.isOccupied = isOccupied;
    }

}
