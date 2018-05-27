using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;

public abstract class NoiseMaker : MonoBehaviour
{
    protected List<Rigidbody2D> myNoiseMakingBodies = new List<Rigidbody2D>();
    public AudioClip DefaultAudioLoop;
    public float NoiseThreshold = 0f;
    protected AudioSource myAudioSource;

    // Use this for initialization
    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    /// <summary>
    /// Unless the child class specifies extra logic for the noise making on Update, just check for rigidBodyVelocity.
    /// </summary>
    protected virtual void Update()
    {
        if (myNoiseMakingBodies.Count > 0)
        {
            if (myNoiseMakingBodies.Exists(x => x.velocity.magnitude > NoiseThreshold))
            {
                SoundEffectHelper.LoopNoiseAtRandomPoint(myAudioSource, DefaultAudioLoop);
            }
            else if (myAudioSource.isPlaying)
            {
                myAudioSource.Stop();
            }
        }
        else
        {
            if (myAudioSource.isPlaying)
            {
                myAudioSource.Stop();
            }
        }
    }

    /// <summary>
    /// Here the child must set rules for which rigidbodies to add to myNoiseMakingBodies
    /// </summary>
    /// <param name="collision"></param>
    protected abstract void OnTriggerEnter2D(Collider2D collision);

    protected void OnTriggerExit2D(Collider2D other) {
        if (myNoiseMakingBodies.Contains(other.attachedRigidbody))
        {
            myNoiseMakingBodies.Remove(other.attachedRigidbody);
        }
    }

}
