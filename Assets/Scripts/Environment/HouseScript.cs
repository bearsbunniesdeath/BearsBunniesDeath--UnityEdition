using Assets.Scripts.Environment;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseScript : MonoBehaviour, IPlayerLightDimmer{

    private Transform myRoofTransform;
    private Vector3 myOriginalRoofLocation;

    public float DimmingTime
    {
        private set;
        get;
    }

    public float DimmingFactor
    {
        private set;
        get;
    }

    // Use this for initialization
    void Start () {
        myRoofTransform = transform.Find("Roof");
        myOriginalRoofLocation = myRoofTransform.localScale;
        DimmingTime = 0.5f;
        DimmingFactor = 0.5f;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            //Hide the roof and reveal the inside
            myRoofTransform.localScale = new Vector3(0, 0, 0);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            //Hide the roof and reveal the inside
            myRoofTransform.localScale = myOriginalRoofLocation;
        }
    }

}
