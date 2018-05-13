using Assets.Scripts.Environment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerBehaviour_1 : MonoBehaviour {

    private const string GROUND_LEVEL_COLLIDER_STRING = "GroundLevelCollider";
    private const string ITEM_MANAGER_STRING = "ItemManager";

    private enum PlayerMovementState {
        eNormal,
        eDashing,
        eDashCooldown,
        eTired
    }

    public bool IsInvincible = false;
    public bool IsDead = false;

    //My components and child
    private Rigidbody2D myRigidBody;
    private ItemManagerScript myItemManager;

    private PlayerMovementState myState = PlayerMovementState.eNormal;
    private  List<ISpeedInhibitor> mySpeedInhibitors;
    private const float NORMAL_DRAG = 6f;
    private const float SPEED_TO_FORCE = 10.0f;
    private const float MAX_SPEED = 4.0f;
    private const float DASH_FORCE = 1000;

    private const float NORMAL_SPEED = 4.0f;
    private const float DASH_SPEED = 16.0f;
    private const float DASH_DISTANCE = 1.5f;
    private float myCurrentDashTimer = 0f;
    private const float DASH_COOLDOWN = 1.0f;
    private float myCurrentDashCooldown = 0f;
    /// <summary>
    /// Number of seconds without dashing, before another dash is made available
    /// </summary>
    private const float DASH_RESET_TIME = 2.0f;
    private float myCurrentDashResetTimer = DASH_RESET_TIME;
    private const int MAX_NUMBER_OF_DASHES = 4;
    private int myCurrentNumberOfDashes = MAX_NUMBER_OF_DASHES;

    private const float JOYSTICK_THRESHOLD = 0.1f;

    // Use this for initialization
    void Start () {
        myRigidBody = GetComponent<Rigidbody2D>();
        myItemManager = this.transform.Find(ITEM_MANAGER_STRING).GetComponent<ItemManagerScript>();
        mySpeedInhibitors = new List<ISpeedInhibitor>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!IsDead) {
            UpdateMovementFromInput();
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown("joystick button 0"))
            {
                myItemManager.AttemptPopStack();
            }
        }
    }

    /// <summary>
    /// Do all movement logic for one Update
    /// </summary>
    private void UpdateMovementFromInput()
    {
        Vector2 direction = GetMovementDirectionFromInput();


        RecoveryDashesOverTime();

        switch (myState)
        {
            case PlayerMovementState.eNormal:
                if (myCurrentNumberOfDashes > 0 && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown("joystick button 4")))
                {
                    myState = PlayerMovementState.eDashing;
                    Debug.Log("Start Dash.");
                    //Turn on ground level jump collider so we can "jump" over short objects
                    this.transform.Find(GROUND_LEVEL_COLLIDER_STRING).gameObject.SetActive(false);
                    myRigidBody.AddForce(direction.normalized * DASH_FORCE);
                    myCurrentNumberOfDashes -= 1;
                    myCurrentDashTimer = DASH_DISTANCE / DASH_SPEED;
                    break;
                }

                float speed = NORMAL_SPEED;

                if (myRigidBody.velocity.magnitude < MAX_SPEED) {
                    myRigidBody.AddForce(direction.normalized * speed * SPEED_TO_FORCE);
                }

                break;

            case PlayerMovementState.eDashing:
                myCurrentDashTimer -= Time.deltaTime;
                if (myCurrentDashTimer < 0.0f) {
                    myState = PlayerMovementState.eDashCooldown;
                    myCurrentDashCooldown = DASH_COOLDOWN;
                    this.transform.Find(GROUND_LEVEL_COLLIDER_STRING).gameObject.SetActive(true);
                    break;
                }

                if (myRigidBody.velocity.magnitude < MAX_SPEED)
                {
      
                }

                break;

            case PlayerMovementState.eDashCooldown:
                myCurrentDashCooldown -= Time.deltaTime;
                if (myCurrentDashTimer < 0.0f)
                {
                    myState = PlayerMovementState.eNormal;
                    break;
                }
                if (myRigidBody.velocity.magnitude < MAX_SPEED)
                {
                    myRigidBody.AddForce(direction.normalized * NORMAL_SPEED * 0.2f * SPEED_TO_FORCE);
                }

                break;
        }


    }

    private void RecoveryDashesOverTime()
    {
        //Debug.Log(myCurrentNumberOfDashes);
        if (myCurrentNumberOfDashes >= MAX_NUMBER_OF_DASHES) {
            return;
            //Early Exit: Nothing to recover
        }

        if (myCurrentDashResetTimer > 0.0f)
        {
            myCurrentDashResetTimer -= Time.deltaTime;
        }
        else {
            myCurrentNumberOfDashes += 1;
            myCurrentDashResetTimer = DASH_RESET_TIME;
        }
    }

    /// <summary>
    /// Checks keyboard and joystick inputs to get a direction for the player to move. Called every update.
    /// </summary>
    /// <returns></returns>
    private Vector2 GetMovementDirectionFromInput()
    {
        Vector2 movementDirection = new Vector2(0, 0);

        float horVal = Input.GetAxis("Horizontal");
        float vertVal = Input.GetAxis("Vertical");

        if (horVal < -JOYSTICK_THRESHOLD || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            movementDirection.x = -1;
        }
        else if (horVal > JOYSTICK_THRESHOLD || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            movementDirection.x = 1;
        }

        if (vertVal < -JOYSTICK_THRESHOLD || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            movementDirection.y = -1;
        }

        else if (vertVal > JOYSTICK_THRESHOLD || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            movementDirection.y = 1;
        }

        return movementDirection;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: 1: Put this in a helper.
        MonoBehaviour[] list = other.gameObject.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour mb in list)
        {
            if (mb is ISpeedInhibitor)
            {
                ISpeedInhibitor breakable = (ISpeedInhibitor)mb;
                if (!mySpeedInhibitors.Contains(breakable)) {
                    mySpeedInhibitors.Add(breakable);
                    Debug.Log(mySpeedInhibitors.Count);
                    //TODO: Find the minimum SlowFactor and apply it to the drag
                    this.myRigidBody.drag = (1 / mySpeedInhibitors.FirstOrDefault().SlowFactor) * NORMAL_DRAG;
                }
            }
        }

        //Check if the tag of the trigger collided with is Exit.
        if (other.tag == "Bear")
        {
            if (!IsInvincible)
            {
                Kill();
            }
        }
        
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Exit Collider");
        if (mySpeedInhibitors.Count > 0) {
            MonoBehaviour[] list = other.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour mb in list)
            {
                if (mb is ISpeedInhibitor)
                {
                    mySpeedInhibitors.Remove((ISpeedInhibitor)mb);
                    if (mySpeedInhibitors.Count == 0) {
                        this.myRigidBody.drag = NORMAL_DRAG;
                    }
                }
            }
        }
    }

    public void Kill()
    {
        IsDead = true;
        myRigidBody.freezeRotation = false;
        //myRenderer.material.SetColor("_Color", Color.black);
    }

}



