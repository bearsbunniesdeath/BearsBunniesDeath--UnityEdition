using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerryBushBehavior : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Bear")
        {

        }
    }
}