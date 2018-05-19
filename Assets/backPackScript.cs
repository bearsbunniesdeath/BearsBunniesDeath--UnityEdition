using Assets.Scripts.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets.Scripts;

public class backPackScript : MonoBehaviour, IWearableItem{

    public AudioClip PickUpSound;

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

    public bool Worn
    {
        get;
        set;
    }

    public void MakePickUpNoise()
    {
        AudioSource.PlayClipAtPoint(PickUpSound, transform.position);
    }
}
