using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System;

public class TorchBehaviour : MonoBehaviour, IHoldableObject {

    private Light myLight;
    const float ON_INTENSITY = 0.85f;
    private AudioSource myAudioSource;
    public AudioClip IgniteAudioClip; 

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
            return eItemType.torch;
        }
    }

    // Use this for initialization
    void Start () {
        ObjectTransform = this.transform;
        myLight = transform.GetComponentInChildren<Light>(true);
        myLight.intensity = 0f;
        myAudioSource = GetComponent<AudioSource>();
    }

    //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
     private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (myLight.intensity != ON_INTENSITY) {
                SoundEffectHelper.MakeNoise(myAudioSource, IgniteAudioClip);
                myLight.intensity = ON_INTENSITY;
            }
        }
    }

    public void ForceChangeLight(bool toOn) {
        if (toOn)
        {
            SoundEffectHelper.MakeNoise(myAudioSource, IgniteAudioClip);
            myLight.intensity = ON_INTENSITY;
        }
        else {
            myLight.intensity = 0f;
        }
    }

    public void MakePickUpNoise()
    {
        //SoundEffectHelper.MakeNoise(myAudioSource, )
    }
}
