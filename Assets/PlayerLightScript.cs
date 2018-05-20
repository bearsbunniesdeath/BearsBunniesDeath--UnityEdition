using Assets.Scripts.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLightScript : MonoBehaviour {

    private Light myLightControl;
    private Rigidbody2D myPlayerBody;

    private float myNormalLightRange = 16f;
    private const float LOWEST_FACTOR_WHILE_RUNNING = 5.5f/16f;
    private const float HIGHEST_VELOCITY = 4.5f;
    //Might be useful to have a public setpoint
    public float mySetPoint;
    private List<ILightInhibitor> myCurrentDimmers;

    // Use this for initialization
    void Start () {             
        myLightControl = GetComponent<Light>();
        myNormalLightRange = myLightControl.range;
        mySetPoint = myNormalLightRange;

        myCurrentDimmers = new List<ILightInhibitor>();
    }
	
	// Update is called once per frame
	void Update () {
        if (myPlayerBody == null)
        {
            PlayerBehaviour_1 pScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBehaviour_1>();
            myPlayerBody = pScript.RigidBody;
        }

        Debug.Log("Dimmer Count: " + myCurrentDimmers.Count);
        if (myCurrentDimmers.Count == 0) {
            //Now we adjust based on player speed
            float lightScale =  Mathf.Max(0, (HIGHEST_VELOCITY - myPlayerBody.velocity.magnitude)) / HIGHEST_VELOCITY;
            // 1 for most light, 0 for least light

            //Debug.Log("This chunk: " + (LOWEST_FACTOR_WHILE_RUNNING + (1 - LOWEST_FACTOR_WHILE_RUNNING) * lightScale).ToString());
            mySetPoint = (LOWEST_FACTOR_WHILE_RUNNING + (1 - LOWEST_FACTOR_WHILE_RUNNING) * lightScale) *myNormalLightRange;
        }

        //Debug.Log("SP: " + mySetPoint);
        myLightControl.range = Mathf.Lerp(myLightControl.range, mySetPoint, Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: 1: Put this in a helper.
        MonoBehaviour[] list = other.gameObject.GetComponentsInParent<MonoBehaviour>();
        if (list.Length == 0)
        {
            list = other.gameObject.GetComponents<MonoBehaviour>();
        }
        foreach (MonoBehaviour mb in list)
        {
            if (mb is ILightInhibitor)
            {
                ILightInhibitor dimmer = (ILightInhibitor)mb;
                if (!myCurrentDimmers.Contains(dimmer)) {
                    myCurrentDimmers.Add(dimmer);
                    mySetPoint = myNormalLightRange * dimmer.DimmingFactor;
                }
            }
        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Exit Collider");
        if (myCurrentDimmers.Count > 0)
        {
            MonoBehaviour[] list = other.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour mb in list)
            {
                if (mb is ILightInhibitor)
                {
                    myCurrentDimmers.Remove((ILightInhibitor)mb);
                    if (myCurrentDimmers.Count == 0)
                    {
                        mySetPoint = myNormalLightRange;
                    }
                }
            }
        }
    }

    internal void Reset()
    {
        myCurrentDimmers.Clear();
    }
}
