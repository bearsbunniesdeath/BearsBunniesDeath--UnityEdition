using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowBehaviour : MonoBehaviour {

    private void Update()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        //Check if the arrow is "on the floor"
        if (rb.velocity.magnitude == 0.0f)
        {
            //Arrow is "on the floor", we should disable collisions by moving the arrows on a layer
            //where the rigidbodies will not collide with characters
            gameObject.layer = LayerMask.NameToLayer("Dead Arrows");  //TODO: Do we want a more general layer for this?

            //Don't need this behaviour anymore
            enabled = false;
        }    
    }

           
}
