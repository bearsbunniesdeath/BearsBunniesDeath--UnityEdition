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
                return eWearableItemType.speed;
            }
        }

        public void MakePickUpNoise()
        {
            //throw new NotImplementedException();
        }
    }

}
