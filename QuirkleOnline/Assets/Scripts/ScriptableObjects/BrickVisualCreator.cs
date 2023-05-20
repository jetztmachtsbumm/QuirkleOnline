using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "ScriptableObject/BrickVisualCerator")]
public class BrickVisualCreator : ScriptableObject
{

    [SerializeField] private Sprite circleImage;
    [SerializeField] private Sprite crossImage;
    [SerializeField] private Sprite lightningBoltImage;
    [SerializeField] private Sprite squareImage;
    [SerializeField] private Sprite starImage;
    [SerializeField] private Sprite diamondImage;

    [SerializeField] private Color purple;
    [SerializeField] private Color red;
    [SerializeField] private Color blue;
    [SerializeField] private Color yellow;
    [SerializeField] private Color green;
    [SerializeField] private Color orange;

    public void UpdateVisual(Image visual, BrickData.BrickShape brickShape, BrickData.BrickColor brickColor)
    {
        switch (brickShape)
        {
            case BrickData.BrickShape.CIRCLE:
                visual.sprite = circleImage;
                break;
            case BrickData.BrickShape.CROSS:
                visual.sprite = crossImage;
                break;
            case BrickData.BrickShape.LIGHTNING_BOLT:
                visual.sprite = lightningBoltImage;
                break;
            case BrickData.BrickShape.SQUARE:
                visual.sprite = squareImage;
                break;
            case BrickData.BrickShape.STAR:
                visual.sprite = starImage;
                break;
            case BrickData.BrickShape.DIAMOND:
                visual.sprite = diamondImage;
                break;
        }

        switch (brickColor)
        {
            case BrickData.BrickColor.PURPLE:
                visual.color = purple;
                break;
            case BrickData.BrickColor.RED:
                visual.color = red;
                break;
            case BrickData.BrickColor.BLUE:
                visual.color = blue;
                break;
            case BrickData.BrickColor.YELLOW:
                visual.color = yellow;
                break;
            case BrickData.BrickColor.GREEN:
                visual.color = green;
                break;
            case BrickData.BrickColor.ORANGE:
                visual.color = orange;
                break;
        }
    }

}
