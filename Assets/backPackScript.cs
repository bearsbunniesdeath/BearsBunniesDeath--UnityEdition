using Assets.Scripts.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class backPackScript : MonoBehaviour, IWearableItem{
    public float Magnitude
    {
        get
        {
            return 1f;
        }
    }

    public eWearableItemType TypeOfItem
    {
        get
        {
            return eWearableItemType.capacity;
        }
    }

    public void MakePickUpNoise()
    {

    }
}
