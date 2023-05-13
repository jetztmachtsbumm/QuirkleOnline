using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControlls : MonoBehaviour
{

    private void Update()
    {
        if (!MultiplayerManager.Instance.IsClientInTurn()) return;

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            BrickGhost.Instance.ShowServerRpc();

            if (Input.GetMouseButtonDown(0))
            {
                BrickGhost.Instance.PlaceBrickServerRpc(GridSystem.Instance.GetGridCellAtWorldPosition(BrickGhost.Instance.transform.position));
            }
        }
        else
        {
            BrickGhost.Instance.HideServerRpc();
        }
    }

}
