using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class SoundEffectHelper
    {
        static public void MakeNoise(AudioSource source , AudioClip clip) {
            source.clip = clip;
            source.Play();
            }
        }
    }

