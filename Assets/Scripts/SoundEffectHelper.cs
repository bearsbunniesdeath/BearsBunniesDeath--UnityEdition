using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class SoundEffectHelper
    {
        static public void MakeNoise(AudioSource source, AudioClip clip) {
            source.loop = false;
            source.clip = clip;
            source.Play();
        }

        static public void LoopNoiseAtRandomPoint(AudioSource source, AudioClip clip)
        {
            if (!source.isPlaying)
            {
                source.clip = clip;

                float randomStartingTime = UnityEngine.Random.Range(0f, clip.length);
                source.loop = true;
                source.time = randomStartingTime;
                source.Play();
            }
        }
    }


}
    

