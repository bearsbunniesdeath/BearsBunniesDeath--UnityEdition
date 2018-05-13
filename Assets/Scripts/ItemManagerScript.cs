using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManagerScript : MonoBehaviour {

    private List<IHoldableObject> myHeldObjects;
    private const int STARTING_CAPACITY = 4;
    public int Capacity = STARTING_CAPACITY;

    // Use this for initialization
    void Start () {
        myHeldObjects = new List<IHoldableObject>();
    }
	
	// Update is called once per frame
	void Update () {
    }

    private void LateUpdate(){
        foreach (IHoldableObject obj in myHeldObjects){
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
        if (myHeldObjects.Contains(holdable) || myHeldObjects.Count >= Capacity) { return; }

        //As of now, the rule is one of each type
        if (!(myHeldObjects.Exists(o => o.TypeOfItem == eItemType.torch) && holdable.TypeOfItem == eItemType.torch) && holdable.IsHoldableInCurrentState)
        {
            if (!holdable.IsHeld)
            {
            holdable.MakePickUpNoise();
                holdable.ObjectTransform.parent = this.gameObject.transform;
                holdable.ObjectTransform.localPosition = Vector3.zero;
                holdable.IsHeld = true;
                myHeldObjects.Add(holdable);
            }
        }

    }

    internal void AttemptPopStack()
    {
        if (myHeldObjects.Count > 0) {
            IHoldableObject removeMe = myHeldObjects[myHeldObjects.Count - 1];
            myHeldObjects.Remove(removeMe);
            removeMe.ObjectTransform.parent = null;
            removeMe.IsHeld = false;
        }
    }
}

