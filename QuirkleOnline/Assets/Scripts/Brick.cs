using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Brick : MonoBehaviour
{

    [SerializeField] private TextMeshPro shapeText;
    [SerializeField] private TextMeshPro colorText;

    private BrickData brickData;

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
        shapeText.text = brickData.GetBrickShape().ToString();
        colorText.text = brickData.GetBrickColor().ToString();
    }

}
