using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class BrickGhost : NetworkBehaviour
{

    public static BrickGhost Instance { get; private set; }

    [SerializeField] private TextMeshPro shapeText;
    [SerializeField] private TextMeshPro colorText;

    private BrickData brickData;

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogWarning("There is more than one BrickGhost object active in the scene!");
            Destroy(gameObject);
        }
        Instance = this;
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                transform.position = MouseWorld.Instance.GetMouseWorldPosition() + new Vector3(0, 3, 0);
            }
        }
    }

    public BrickData GetBrickData()
    {
        return brickData;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetBrickDataServerRpc(BrickData brickData)
    {
        SetBrickDataClientRpc(brickData);
    }

    [ClientRpc]
    private void SetBrickDataClientRpc(BrickData brickData)
    {
        this.brickData = brickData;
        UpdateVisual();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlaceBrickServerRpc()
    {
        PlaceBrickClientRpc();
    }

    [ClientRpc]
    private void PlaceBrickClientRpc()
    {
        Transform brickPrefab = Resources.Load<Transform>("BrickTemplate");

        Vector3 placePosition = new Vector3(BrickGhost.Instance.transform.position.x, 0, BrickGhost.Instance.transform.position.z);

        Transform spawnedBrick = Instantiate(brickPrefab, placePosition, Quaternion.identity);

        spawnedBrick.GetComponent<Brick>().SetBrickData(BrickGhost.Instance.GetBrickData());
    }

    private void UpdateVisual()
    {
        shapeText.text = brickData.GetBrickShape().ToString();
        colorText.text = brickData.GetBrickColor().ToString();
    }

}
