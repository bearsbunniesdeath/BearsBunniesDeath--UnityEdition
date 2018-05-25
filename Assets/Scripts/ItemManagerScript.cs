using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManagerScript : MonoBehaviour {
    private const int STARTING_CAPACITY = 4;
    private const int MAX_CAPACITY = 6;
    private int myCapacity = STARTING_CAPACITY;

    public int Capacity {
        get
        {
            return myCapacity;
        }
    }


    public List<IHoldableObject> HeldObjects { get; private set; }

    // Use this for initialization
    void Start () {
        HeldObjects = new List<IHoldableObject>();
    }
	
	// Update is called once per frame
	void Update () {
    }

    private void LateUpdate(){
        foreach (IHoldableObject obj in HeldObjects){
            obj.ObjectTransform.localPosition = Vector3.zero;
        }

    }

    private void OnTriggerEnter2D(Collider2D other) {
        //TODO: 1: Put this in a helper.
        MonoBehaviour[] list = other.gameObject.GetComponentsInParent<MonoBehaviour>();
        if (list.Length == 0) {
            list = other.gameObject.GetComponents<MonoBehaviour>();
        }
        foreach (MonoBehaviour mb in list)
        {
            if (mb is IHoldableObject)
            {
                IHoldableObject holdable = (IHoldableObject)mb;
                AttemptToPickUpItem(holdable);
            }
        }

    }

    private void AttemptToPickUpItem(IHoldableObject holdable)
    {
        //Logic to see what I can hold
        if (HeldObjects.Contains(holdable) || HeldObjects.Count >= myCapacity) { return; }

        //As of now, the rule is one of each type
        if (!(HeldObjects.Exists(o => o.TypeOfItem == eItemType.torch) && holdable.TypeOfItem == eItemType.torch) && holdable.IsHoldableInCurrentState)
        {
            if (!holdable.IsHeld)
            {
            holdable.MakePickUpNoise();
                holdable.ObjectTransform.parent = this.gameObject.transform;
                holdable.ObjectTransform.localPosition = Vector3.zero;
                holdable.ObjectTransform.GetComponent<Renderer>().enabled = false;
                foreach (Renderer r in holdable.ObjectTransform.GetComponentsInChildren<Renderer>())
                    r.enabled = false;
                holdable.IsHeld = true;
                HeldObjects.Add(holdable);
                CheckForMateableBunnies();
            }
        }

    }

    private void CheckForMateableBunnies()
    {
        for (int i = 0; i < HeldObjects.Count - 1; i++)
        { //Don't do the last one since we always look forward
            IHoldableObject currItem = HeldObjects[i];
            IHoldableObject nextItem = HeldObjects[i + 1];
            if (currItem.TypeOfItem == eItemType.bunny && nextItem.TypeOfItem == eItemType.bunny)
            {
                BunnyBehaviour currBunny = (BunnyBehaviour)currItem;
                BunnyBehaviour nextBunny = (BunnyBehaviour)nextItem;
                if (currBunny.Gender != BunnyBehaviour.eBunnyGender.baby && nextBunny.Gender != BunnyBehaviour.eBunnyGender.baby)
                {
                    if (currBunny.Gender != nextBunny.Gender && currBunny.Mate == null && nextBunny.Mate == null)
                    {
                        Debug.Log("  ;) ");
                        //Must go both ways, so both can check if the other's still there.
                        currBunny.StartMating(nextBunny);
                        nextBunny.StartMating(currBunny);
                    }
                }
            }
        }
    }

    internal void AttemptPopStack()
    {
        if (HeldObjects.Count > 0) {
            IHoldableObject removeMe = HeldObjects[HeldObjects.Count - 1];
            HeldObjects.Remove(removeMe);
            removeMe.ObjectTransform.parent = null;
            removeMe.ObjectTransform.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            removeMe.ObjectTransform.GetComponent<Renderer>().enabled = true;
            foreach (Renderer r in removeMe.ObjectTransform.GetComponentsInChildren<Renderer>())
                r.enabled = true;
            removeMe.IsHeld = false;
        }
    }

    internal void Reset()
    {
        HeldObjects.Clear();
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        myCapacity = STARTING_CAPACITY;
    }

    internal void AttemptToAddCapacity(int magnitude)
    {
        myCapacity = Math.Min(myCapacity + magnitude, MAX_CAPACITY);
        Debug.Log("Total Capacity:" + myCapacity);
    }
}

