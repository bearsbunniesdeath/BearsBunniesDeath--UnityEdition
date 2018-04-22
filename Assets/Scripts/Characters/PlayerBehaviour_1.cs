using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour_1 : MonoBehaviour {

    private Rigidbody2D myRigidBody;
    private const float NORMAL_SPEED = 4.0f;
    private const float JOYSTICK_THRESHOLD = 0.2f;

    // Use this for initialization
    void Start () {
        myRigidBody = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update () {
        UpdateMovementFromInput();
    }

    private void UpdateMovementFromInput()
    {
        Vector2 velocity = new Vector2(0, 0);


        float horVal = Input.GetAxis("Horizontal");
        float vertVal = Input.GetAxis("Vertical");

        if (horVal < -JOYSTICK_THRESHOLD)
        {
            velocity.x = -1;
        }
        else if (horVal > JOYSTICK_THRESHOLD)
        {
            velocity.x = 1;
        }

        if (vertVal < -JOYSTICK_THRESHOLD)
        {
            velocity.y = -1;
        }

        else if (vertVal > JOYSTICK_THRESHOLD)
        {
            velocity.y = 1;
        }



        myRigidBody.MovePosition(myRigidBody.position + velocity.normalized * NORMAL_SPEED * Time.fixedDeltaTime);
    }
}
