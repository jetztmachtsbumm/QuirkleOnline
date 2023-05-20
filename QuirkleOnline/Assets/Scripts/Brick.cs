using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Brick : MonoBehaviour
{

    private static BrickVisualCreator brickVisualCreator;

    [SerializeField] private Image brickImage;

    private BrickData brickData;

    private void Awake()
    {
        if(brickVisualCreator == null)
        {
            brickVisualCreator = Resources.Load<BrickVisualCreator>("BrickVisualCreator");
        }
    }

    public BrickData GetBrickData()
    {
        return brickData;
    }

    public void SetBrickData(BrickData brickData)
    {
        this.brickData = brickData;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        brickVisualCreator.UpdateVisual(brickImage, brickData.GetBrickShape(), brickData.GetBrickColor());
    }

}
