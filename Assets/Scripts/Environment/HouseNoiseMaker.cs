using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class HouseNoiseMaker : NoiseMaker
{

    protected override void Update()
    {
        if (myNoiseMakingBodies.Count > 0)
        {
            foreach (Rigidbody2D body in myNoiseMakingBodies) {
                //Player must be alive for footsteps to happen
                PlayerBehaviour_1 maybePlayer = body.GetComponent<PlayerBehaviour_1>();
                if (maybePlayer != null)
                {
                    if (!maybePlayer.IsDead && body.velocity.magnitude > NoiseThreshold)
                    {
                        SoundEffectHelper.LoopNoiseAtRandomPoint(myAudioSource, DefaultAudioLoop);
                    }
                    else if(myNoiseMakingBodies.Count == 1) {
                        //Only thing is the player, turn off noise when stopped
                        myAudioSource.Stop();
                    }
                }
                else {
                    //Just do it for all the time for bunnies and bears (for now)
                    SoundEffectHelper.LoopNoiseAtRandomPoint(myAudioSource, DefaultAudioLoop);
                }
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


    protected override void OnTriggerEnter2D(Collider2D other)
    {

        if (other.tag == "Player" || other.tag == "Bear" || other.tag == "Bunny") {

            if (!myNoiseMakingBodies.Contains(other.attachedRigidbody))
            {
                myNoiseMakingBodies.Add(other.attachedRigidbody);
            }
        }
    }
}
