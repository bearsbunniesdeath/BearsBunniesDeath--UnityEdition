using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System;

public class TrapScript : MonoBehaviour, IHoldableObject
{

    private AudioSource myAudioSource;
    public AudioClip PickUpNoise;
    public AudioClip SetOffNoise;

    void Start() {
        ObjectTransform = this.transform;
        myAudioSource = GetComponent<AudioSource>();
    }

    public void MakePickUpNoise()
    {
        SoundEffectHelper.MakeNoise(myAudioSource, PickUpNoise);
    }

    //Matt: Probably some trap specific logic to insert here
    public bool IsHeld
    {
        get;
        set;
    }

    public bool IsHoldableInCurrentState
    {
        get
        {
            return true;
        }
    }

    public Transform ObjectTransform
    {
        private set;
        get;
    }

    public eItemType TypeOfItem
    {
        get
        {
            return eItemType.trap;
        }
    }
}
