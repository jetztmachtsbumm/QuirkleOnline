using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCap : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Destroy(gameObject);
    }
}
