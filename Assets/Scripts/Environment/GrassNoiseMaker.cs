using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class GrassNoiseMaker : NoiseMaker
{

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //Duplicated from ThickGrassScript, but I don't know if there's a cleaner way to do this...
        MonoBehaviour[] list = other.gameObject.GetComponentsInParent<MonoBehaviour>();
        if (list.Length == 0)
        {
            list = other.gameObject.GetComponents<MonoBehaviour>();
        }
        foreach (MonoBehaviour mb in list)
        {
            if (mb is IHoldableObject)
            {
                IHoldableObject holdable = (IHoldableObject)mb;
                if (holdable.IsHeld)
                {
                    return;
                }
            }
        }

        if (!myNoiseMakingBodies.Contains(other.attachedRigidbody))
        {
            myNoiseMakingBodies.Add(other.attachedRigidbody);
        }
    }
}
