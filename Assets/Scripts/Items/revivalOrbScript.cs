using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Items
{
    class revivalOrbScript
        : MonoBehaviour, IWearableItem
    {
        public AudioClip PickUpSoundClip;

        public float Magnitude
        {
            get
            {
                return 1;
            }
        }

        public eWearableItemType TypeOfItem
        {
            get
            {
                return eWearableItemType.lives;
            }
        }

        public bool Worn
        {
            get;
            set;
        }

        public void MakePickUpNoise()
        {
            //Need to use a the static audio source, because this is gonna die now!
            AudioSource.PlayClipAtPoint(PickUpSoundClip, transform.position);
        }
    }

}
