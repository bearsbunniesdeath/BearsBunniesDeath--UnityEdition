using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IHoldableObject
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

    /// <summary>
    /// The HUD needs to know some more details of the items, (ex. Bunny gender)
    /// </summary>
    public enum eHUDItemType
    {
        bunny,
        bunnyFemale,
        bunnyMale,
        torch,
        trap,
        bomb
    }
}
