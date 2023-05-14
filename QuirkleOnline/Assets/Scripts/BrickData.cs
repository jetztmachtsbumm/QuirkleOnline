using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct BrickData : IEquatable<BrickData>, INetworkSerializable
{
    
    public enum BrickShape
    {
        NONE,
        CIRCLE,
        CROSS,
        FLOWER,
        SQUARE,
        STAR,
        DIAMOND
    }

    public enum BrickColor
    {
        NONE,
        PURPLE,
        RED,
        BLUE,
        YELLOW,
        GREEN,
        ORANGE
    }

    private BrickShape brickShape;
    private BrickColor brickColor;

    public BrickData(BrickShape brickShape, BrickColor brickColor)
    {
        this.brickShape = brickShape;
        this.brickColor = brickColor;
    }

    public bool Equals(BrickData other)
    {
        return brickShape == other.brickShape && brickColor == other.brickColor;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref brickShape);
        serializer.SerializeValue(ref brickColor);
    }

    public BrickShape GetBrickShape()
    {
        return brickShape;
    }

    public BrickColor GetBrickColor()
    {
        return brickColor;
    }

    public static bool Match(BrickData brickData1, BrickData brickData2)
    {
        return (brickData1.brickShape == brickData2.brickShape || brickData1.brickColor == brickData2.brickColor) &&
                !(brickData1.brickShape == brickData2.brickShape && brickData1.brickColor == brickData2.brickColor);
    }

}
