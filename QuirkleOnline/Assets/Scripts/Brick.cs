using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct BrickData
{
    
    public enum BrickShape
    {
        CIRCLE,
        CROOS,
        FLOWER,
        SQUARE,
        STAR,
        DIAMOND
    }

    public enum BrickColor
    {
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

    public BrickShape GetBrickShape()
    {
        return brickShape;
    }

    public BrickColor GetBrickColor()
    {
        return brickColor;
    }

}
