using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public enum eWearableItemType
{
    speed,
    lives,
    capacity
}

namespace Assets.Scripts.Items
{
    interface IWearableItem
    {
        eWearableItemType TypeOfItem
        {
            get;
        }

        float Magnitude
        {
            get;
        }

        void MakePickUpNoise();
    }
}
