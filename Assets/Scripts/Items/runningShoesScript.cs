using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Items
{
    class runningShoesScript
        : MonoBehaviour, IWearableItem
    {
        public AudioClip PickUpSound;

        public float Magnitude
        {
            get
            {
                return 0.5f;
            }
        }

        public eWearableItemType TypeOfItem
        {
            get
            {
                return eWearableItemType.speed;
            }
        }

        public void MakePickUpNoise()
        {
            //Need to use a the static audio source, because this is gonna die now!
            AudioSource.PlayClipAtPoint(PickUpSound, transform.position);
        }
    }

}
