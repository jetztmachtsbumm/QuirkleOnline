using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AvailableBricksUI : MonoBehaviour
{

    [SerializeField] private Transform brickUITemplate;

    private void Start()
    {
        GameManager.Instance.OnAvailableBricksChanged += GameManager_OnAvailableBricksChanged;
    }

    private void GameManager_OnAvailableBricksChanged(object sender, System.EventArgs e)
    {
        UpdateBricks();
    }

    private void UpdateBricks()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach(BrickData availableBrick in GameManager.Instance.GetAvailableBricks())
        {
            Transform template = Instantiate(brickUITemplate, transform);

            template.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (MultiplayerManager.Instance.IsClientInTurn())
                {
                    BrickGhost.Instance.SetBrickDataServerRpc(availableBrick);
                    GameManager.Instance.SetIsBrickSelected(true);
                }
            });

            template.Find("ShapeText").GetComponent<TextMeshProUGUI>().text = availableBrick.GetBrickShape().ToString();
            template.Find("ColorText").GetComponent<TextMeshProUGUI>().text = availableBrick.GetBrickColor().ToString();
        }
    }

}
