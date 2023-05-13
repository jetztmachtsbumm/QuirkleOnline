using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerControlls : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (!MultiplayerManager.Instance.IsClientInTurn()) return;

                BrickGhost.Instance.PlaceBrickServerRpc(GridSystem.Instance.GetGridCellAtWorldPosition(BrickGhost.Instance.transform.position));
            }
        }
    }

}
