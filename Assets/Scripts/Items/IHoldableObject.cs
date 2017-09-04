using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    interface IHoldableObject
    {
         Transform ObjectTransform {
            get;
        }
         Boolean IsHeld
        {
            get;
            set;
        }

        Boolean IsHoldableInCurrentState
        {
            get;
        }

        eItemType TypeOfItem {
            get;
        }

        void MakePickUpNoise();

    }

    public enum eItemType
    {
        bunny,
        torch,
        trap,
        bomb
    }
}
