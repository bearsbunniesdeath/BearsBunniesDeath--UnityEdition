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

        public void MakePickUpNoise()
        {
            //throw new NotImplementedException();
        }
    }

}
