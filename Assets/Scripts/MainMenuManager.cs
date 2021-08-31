using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public BambooStickSpawner Spawner;
    public int lo_newStick = 50;

    private void Start()
    {
        var newStick = Spawner.SpawnBambooStick(.25f);
        newStick.UpdateOrderInLayer(lo_newStick);
    }
}
