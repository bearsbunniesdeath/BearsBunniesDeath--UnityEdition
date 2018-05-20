using Assets.Scripts;
using Assets.Scripts.Environment;
using Assets.Scripts.Items;
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

    public AudioClip DashNoise;
    public AudioClip ReviveNoise;

    //My components and child
    public Rigidbody2D RigidBody;
    private AudioSource myAudioSource;
    private ItemManagerScript myItemManager;
    private PlayerLightScript myLightScript;

    private PlayerMovementState myState = PlayerMovementState.eNormal;
    private  List<ISpeedInhibitor> mySpeedInhibitors;
    private const float NORMAL_DRAG = 8.5f;
    private const float SPEED_TO_FORCE = 12.0f;
    private const float MAX_SPEED = 4.0f;
    private const float DASH_FORCE = 12.5f;

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

    private int myAdditionalLives = 0;
    private Timer myRevivalTimer;
    private const float REVIVAL_TIME = 2.5f;
    public int AdditionalLives {
    get { return myAdditionalLives; }
    }
    public bool IsReviving {
        get { return myRevivalTimer.IsRunning; }
    }

    public int AvailableNumberOfDashes
    {
    get {
            return myCurrentNumberOfDashes;
        }
    }

    private const float JOYSTICK_THRESHOLD = 0.1f;

    // Use this for initialization
    void Start () {
        myRevivalTimer = gameObject.AddComponent<Timer>();
        myRevivalTimer.ResetTime = REVIVAL_TIME;

        RigidBody = GetComponent<Rigidbody2D>();
        myItemManager = this.transform.Find(ITEM_MANAGER_STRING).GetComponent<ItemManagerScript>();
        myLightScript = this.transform.Find("Point light").GetComponent<PlayerLightScript>();
        mySpeedInhibitors = new List<ISpeedInhibitor>();
        myAudioSource = transform.GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!IsDead)
        {
            UpdateMovementFromInput();
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown("joystick button 0"))
            {
                myItemManager.AttemptPopStack();
            }
        }
        else
        {
            if (myRevivalTimer.CheckForDone())
            {
                Revive();
            }
            else if (myAdditionalLives > 0 && !myRevivalTimer.IsRunning)
            {
                //Start Reviving!
                myRevivalTimer.StartTimer();
                Debug.Log("Start Reviving!");
            }

        }
        
    }

    internal void Reset()
    {
        IsDead = false;
        transform.rotation = new Quaternion(0, 0, 0, 0);
        if (myItemManager != null) {
            myItemManager.Reset();
        }
        if (myLightScript != null)
        {
            myLightScript.Reset();
        }
        GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        GetComponent<Rigidbody2D>().angularVelocity = 0;
        if (RigidBody != null)
        {
            RigidBody.freezeRotation = true;
        }
        myCurrentNumberOfDashes = MAX_NUMBER_OF_DASHES;
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
                    SoundEffectHelper.MakeNoise(myAudioSource, DashNoise);
                    RigidBody.AddForce(direction.normalized * DASH_FORCE, ForceMode2D.Impulse);
                    myCurrentNumberOfDashes -= 1;
                    myCurrentDashTimer = DASH_DISTANCE / DASH_SPEED;
                    break;
                }

                float speed = NORMAL_SPEED;

                if (RigidBody.velocity.magnitude < MAX_SPEED) {
                    RigidBody.AddForce(direction.normalized * speed * SPEED_TO_FORCE) ;
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
                break;

            case PlayerMovementState.eDashCooldown:
                myCurrentDashCooldown -= Time.deltaTime;
                if (myCurrentDashTimer < 0.0f)
                {
                    myState = PlayerMovementState.eNormal;
                    break;
                }
                if (RigidBody.velocity.magnitude < MAX_SPEED)
                {
                    RigidBody.AddForce(direction.normalized * NORMAL_SPEED * 0.2f * SPEED_TO_FORCE);
                }

                break;
        }

        //Debug.Log(myRigidBody.velocity.magnitude);
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

    private void Revive()
    {
        Debug.Log("REVIVAL!");

        explosionHelper.Explode(ReviveNoise, gameObject, 1500f, 5);

        //Revival Explosion
        IsDead = false;
        transform.rotation = new Quaternion(0, 0, 0, 0);
        GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        GetComponent<Rigidbody2D>().angularVelocity = 0;
        if (RigidBody != null)
        {
            RigidBody.freezeRotation = true;
        }
        myAdditionalLives -= 1;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: 1: Put this in a helper.
        MonoBehaviour[] list = other.gameObject.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour mb in list)
        {
            if (mb is ISpeedInhibitor)
            {
                ISpeedInhibitor speedInhib = (ISpeedInhibitor)mb;
                if (!mySpeedInhibitors.Contains(speedInhib)) {
                    mySpeedInhibitors.Add(speedInhib);
                    Debug.Log(mySpeedInhibitors.Count);
                    //TODO: Find the minimum SlowFactor and apply it to the drag
                    this.RigidBody.drag = (1 / mySpeedInhibitors.FirstOrDefault().SlowFactor) * NORMAL_DRAG;
                }
            }

            if (mb is IWearableItem) {
                IWearableItem wearableItem = (IWearableItem)mb;
                if (!wearableItem.Worn) {
                    wearableItem.MakePickUpNoise();
                    if (wearableItem.TypeOfItem == eWearableItemType.speed)
                    {
                        //TODO: 
                        //myAdditionalSpeed += wearableItem.Magnitude;
                    }
                    else if (wearableItem.TypeOfItem == eWearableItemType.lives)
                    {
                        myAdditionalLives += (int)wearableItem.Magnitude;
                        Debug.Log("ADD LIVES:" + myAdditionalLives);
                    }
                    else if (wearableItem.TypeOfItem == eWearableItemType.capacity)
                    {
                        myItemManager.AttemptToAddCapacity((int)wearableItem.Magnitude);
                    }
                    //Get rid the of object, so you can't keep picking it up!
                    wearableItem.Worn = true;
                    Destroy(other.gameObject);
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
        //Debug.Log("Exit Collider");
        if (mySpeedInhibitors.Count > 0) {
            MonoBehaviour[] list = other.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour mb in list)
            {
                if (mb is ISpeedInhibitor)
                {
                    mySpeedInhibitors.Remove((ISpeedInhibitor)mb);
                    if (mySpeedInhibitors.Count == 0) {
                        this.RigidBody.drag = NORMAL_DRAG;
                    }
                }
            }
        }
    }

    public void Kill()
    {
        IsDead = true;
        RigidBody.freezeRotation = false;
        //myRenderer.material.SetColor("_Color", Color.black);
    }

}



