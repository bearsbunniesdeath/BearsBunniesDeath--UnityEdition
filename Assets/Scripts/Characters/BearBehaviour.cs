using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Map;
using Pathfinding;
using System;
using Assets.Scripts;

public class BearBehaviour : NPCBehaviour {

    private enum BehaviourType
    {
        eIdle,
        eRoaming,
        eTrackingPlayer,    //First level of aggro
        eHuntingPlayer,      //Second level of aggro
        eChargingTowardPlayer,
        eHuntingBunny,
        eEatingBunny,
        eStuckInTrap
    }

    //TODO: Probably move this to it's own class
    private Transform player;       // Reference to the player's transform.
    private List<Transform> bunnies;    // Reference to all of the bunnies

    //Determines aggro range of bear
    private float TRACKING_PLAYER_RANGE = 6;  //First level of aggro
    private float HUNTING_PLAYER_RANGE = 4;   //Second level of aggro
    private float CHARGE_PLAYER_RANGE = 4;   //threshold to charge
    private float DEAGGRO_PLAYER_RANGE = 9;   //The range at which the bear will stop tracking/hunting player 

    private float HUNTING_BUNNY_RANGE = 6;
    private BunnyBehaviour myHuntedBunnyScript;

    private float POUNCE_DETECT_RANGE = 2; //Units distance
    private float POUNCE_TIME = .5f; //Seconds
    private float POUNCE_RANGE = 3; //Units distance
    private  Vector3 NULL_VECTOR3 = new Vector3(-1000, -1000, -1000);
    private float navigationTimer = 0;

    private Animator myAnimator;
    private Renderer myRenderer;
    private AudioSource myAudioSource;
    public AudioClip ChargeNoise;
    public AudioClip PounceNoise;
    public AudioClip EatingNoise;
    public AudioClip TrapSetOffNoise;
    private Vector3 myPounceTarget;

    private Vector2 myChargeDirection;

    private float myPounceTimeRemaining = -1;
    private float myRunningPounceTime = 0.0f;

    private const float MAX_TIME_IN_TRAP = 5;
    private float myStuckTimeRemaining = 0f;

    [SerializeField]
    private BehaviourType behaviour;    

    // Use this for initialization
    void Start () {
        //TODO: This only needs to happen once in the beginning
        MapInventory.UpdateGlobalBunnyList();

        player = GameObject.FindGameObjectWithTag("Player").transform;      
        bunnies = GameObject.FindGameObjectsWithTag("Bunny").Select(go => go.transform).ToList();
        behaviour = BehaviourType.eIdle;  //Default for now
        myAnimator = GetComponent<Animator>();
        myRenderer = GetComponent<SpriteRenderer>();
        myAudioSource = GetComponent<AudioSource>();
        myPounceTarget = NULL_VECTOR3;

        StartInvokeRepeating();        
    }
	
	// Update is called once per frame
	void Update () {      
        switch (behaviour)
        {
            case BehaviourType.eIdle:
            case BehaviourType.eEatingBunny:
                break;  //Nothing really to do here :D
            case BehaviourType.eRoaming:
                MoveAlongPath(3f * Time.deltaTime, 0.2f);
                break;
            case BehaviourType.eTrackingPlayer:
                MoveAlongPath(3f * Time.deltaTime, 0.2f);
                CheckForChargeAttack();
                break;
            case BehaviourType.eChargingTowardPlayer:
                ChargeTowardPlayer();
                break;
            case BehaviourType.eHuntingPlayer:
                MoveAlongPath(3f * Time.deltaTime, 0.2f);
                CheckForPounce(player);
                break;
            case BehaviourType.eHuntingBunny:
                //TODO: Sometime bears stop moving, repeating this infinitely
                MoveAlongPath(3f * Time.deltaTime, 0.2f);               
                break;
            case BehaviourType.eStuckInTrap:
                if (myStuckTimeRemaining <= 0)
                    UpdateBehaviour(BehaviourType.eRoaming);
                else
                    myStuckTimeRemaining -= Time.deltaTime;
                break;
        }                    
    }

    private void ChargeTowardPlayer()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, myChargeDirection, 1.25f, 1 << LayerMask.NameToLayer("Obstacles"));
        if (hit.collider != null)
        {
            UpdateBehaviour(BehaviourType.eIdle);
            UpdateBehaviour(); //Immediately check what to do now
        }
        transform.Translate(myChargeDirection * Time.deltaTime * 5f);
    }

    private void CheckForChargeAttack()
    {
        var heading = player.position - transform.position;
        var distance = heading.magnitude;
        if (distance < CHARGE_PLAYER_RANGE)
        {
            //Want long shots only
            return;
        }

        var direction = heading / distance; // This is now the normalized direction.

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, 1 << LayerMask.NameToLayer("Obstacles"));
        if (hit.collider == null)
        {
            myChargeDirection = direction;
            SoundEffectHelper.MakeNoise(myAudioSource, ChargeNoise);
            UpdateBehaviour(BehaviourType.eChargingTowardPlayer);
        }
    }

    void CheckForPounce(Transform target)
    {
        //transform.position = Vector3.Lerp(transform.position, player.position, Random.Range(0.04f, 1)*Time.deltaTime);  //OLD movement

        List<Vector2> availCoords = new List<Vector2>();
        Vector2 start = new Vector2(0, 0);
        Vector2 end = new Vector2(0, 0);
        //Navigator.GetShortestPathCoordinate(start, end ,availCoords);   //TODO: Update this to use new helper methods

        //Lerp to closest coordinate in path without bumping into obstacles


        var heading = target.position - transform.position;
        var distance = heading.magnitude;
        var direction = heading / distance; // This is now the normalized direction.

        //"Charge" pounce for x number of seconds of update time - don't move
        if (myPounceTimeRemaining > 0)
        {
            myPounceTimeRemaining -= Time.deltaTime;

            //At some point in charging get players location and save as pounce target

            //TODO: Shorten the Pounce range if about to hit an obstacle
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, POUNCE_RANGE, 1 << LayerMask.NameToLayer("Obstacles"));
            if (hit.collider == null)
            {
                myPounceTarget = transform.position + direction * POUNCE_RANGE;
            }
            else
            {
                myPounceTarget = transform.position + direction * Mathf.Max(hit.distance - 0.5f, 0f);
            }
        }
        else if (myPounceTimeRemaining < 0 & myPounceTimeRemaining > -1)
        {
            //POUNCE HERE
            //Quick lerp dive to target.
            transform.position = Vector3.Lerp(transform.position, myPounceTarget, UnityEngine.Random.Range(5f, 10) * Time.deltaTime);
            myRunningPounceTime += Time.deltaTime;
            myRenderer.material.SetColor("_Color", Color.blue);
            //TODO: Check if bear stuck in pounce, and reset it stuck

            if (Vector2.Distance(transform.position, myPounceTarget) < 0.3 || myRunningPounceTime > 1.0f)
            {
                myRunningPounceTime = 0f;
                myRenderer.material.SetColor("_Color", Color.white);
                myPounceTimeRemaining = -1.1f;
                myPounceTarget = NULL_VECTOR3;
            }
        }
        //If bear is within pounce range
        else if (Vector2.Distance(transform.position, target.position) < POUNCE_DETECT_RANGE & (UnityEngine.Random.Range(0, 1.0f) > 0.9))
        {

            myRenderer.material.SetColor("_Color", Color.red);
            myPounceTimeRemaining = POUNCE_TIME;
        }
    }

    protected override void UpdatePath()
    {
        if (behaviour == BehaviourType.eIdle)
        {
            path = null;
            currentWayPoint = 0;
        }
        else if (behaviour == BehaviourType.eRoaming)
        {
            Navigator.StartRandomPath(transform.position, 5, OnPathCalculated);
        }
        else if (behaviour == BehaviourType.eTrackingPlayer || behaviour == BehaviourType.eHuntingPlayer)
        {
            Navigator.StartPathBetween(transform.position, player.position, OnPathCalculated);
        }
        else if (behaviour == BehaviourType.eHuntingBunny)
        {
            if (ClosestBunny() != null)
            {
                Navigator.StartPathBetween(transform.position, ClosestBunny().position, OnPathCalculated);
            }
        }
    }

    protected override void UpdateBehaviour()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceToClosestBunny = DistanceToClosestBunny();

        //NOTE: Order matters for each behaviour 
        switch (behaviour)
        {
            case BehaviourType.eIdle:
                if (distanceToClosestBunny <= HUNTING_BUNNY_RANGE)
                    UpdateBehaviour(BehaviourType.eHuntingBunny);
                else if (distanceToPlayer <= TRACKING_PLAYER_RANGE)
                {
                    SoundEffectHelper.MakeNoise(myAudioSource, PounceNoise);
                    UpdateBehaviour(BehaviourType.eTrackingPlayer);
                }
                else if (UnityEngine.Random.Range(0f, 1f) <= 0.25)
                    //25% chance of roaming
                    UpdateBehaviour(BehaviourType.eRoaming);
                break;
            case BehaviourType.eRoaming:
                if (distanceToClosestBunny <= HUNTING_BUNNY_RANGE)
                    UpdateBehaviour(BehaviourType.eHuntingBunny);
                else if (distanceToPlayer <= TRACKING_PLAYER_RANGE) {
                    SoundEffectHelper.MakeNoise(myAudioSource,PounceNoise);
                    UpdateBehaviour(BehaviourType.eTrackingPlayer);
                }
                else if (UnityEngine.Random.Range(0f, 1f) <= 0.05)
                    //5% chance of going idle
                    UpdateBehaviour(BehaviourType.eIdle);
                break;
            case BehaviourType.eTrackingPlayer:
                if (distanceToClosestBunny <= HUNTING_BUNNY_RANGE)
                    UpdateBehaviour(BehaviourType.eHuntingBunny);
                else if (distanceToPlayer < HUNTING_PLAYER_RANGE)
                    UpdateBehaviour(BehaviourType.eHuntingPlayer);
                else if (distanceToPlayer > 1.20 * DEAGGRO_PLAYER_RANGE)
                    UpdateBehaviour(BehaviourType.eRoaming);
                break;
            case BehaviourType.eHuntingPlayer:
                if (distanceToClosestBunny <= HUNTING_BUNNY_RANGE)
                    UpdateBehaviour(BehaviourType.eHuntingBunny);
                else if (distanceToPlayer > 1.20 * DEAGGRO_PLAYER_RANGE)
                    UpdateBehaviour(BehaviourType.eRoaming);
                break;
            case BehaviourType.eHuntingBunny:
                if (distanceToClosestBunny > 1.2 * HUNTING_BUNNY_RANGE)
                    UpdateBehaviour(BehaviourType.eRoaming);
                break;
            case BehaviourType.eEatingBunny:
                if (UnityEngine.Random.Range(0f, 1f) <= 0.3)
                {
                    myHuntedBunnyScript.DropCorpse();
                    UpdateBehaviour(BehaviourType.eRoaming);
                }
                break;
            case BehaviourType.eStuckInTrap:
                break;
        }
    }

    private void UpdateBehaviour(BehaviourType behaviour)
    {
        if (this.behaviour != behaviour)
        {
            this.behaviour = behaviour;
            if (behaviour == BehaviourType.eTrackingPlayer || behaviour == BehaviourType.eHuntingPlayer || behaviour == BehaviourType.eHuntingBunny)
            {
                //Bear should update path more frequently
                ChangeInvokeRate(RepeatingMethod.UpdatePath, 0f, 0.5f);
                myAnimator.Play("Roaming");
            }
            else if (behaviour == BehaviourType.eIdle)
            {
                ChangeInvokeRate(RepeatingMethod.UpdatePath, 0f, 30f); //Do not need to refresh path very often...we are stationary :)   
                myAnimator.Play("Idle");
            }
            else if (behaviour == BehaviourType.eRoaming)
            {
                ChangeInvokeRate(RepeatingMethod.UpdatePath, 0f, 5f);
                myAnimator.Play("Roaming"); //TODO: More anim
            }
            else if (behaviour == BehaviourType.eChargingTowardPlayer)
            {
                myAnimator.Play("Charging");            
            }
            else if (behaviour == BehaviourType.eStuckInTrap)
            {
                myAnimator.Play("StuckInTrap");
            }
            else if (behaviour == BehaviourType.eEatingBunny)
            {
                SoundEffectHelper.MakeNoise(myAudioSource, EatingNoise);
                myAnimator.Play("EatingBunny");
            }
            else
            {
                myAnimator.Play("Idle");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Bunny")
        {
            if (!collision.gameObject.GetComponent(typeof(BunnyBehaviour)).Equals(null)) {
                 myHuntedBunnyScript = (BunnyBehaviour)collision.gameObject.GetComponent(typeof(BunnyBehaviour));
                if (myHuntedBunnyScript.HasEdibleCarcass)
                {
                    UpdateBehaviour(BehaviourType.eEatingBunny);
                    myHuntedBunnyScript.HasEdibleCarcass = false;
                }
            }

        }
        if (collision.tag == "Trap")
        {
            SoundEffectHelper.MakeNoise(myAudioSource, TrapSetOffNoise);
            UpdateBehaviour(BehaviourType.eStuckInTrap);
            Destroy(collision.gameObject);
            myStuckTimeRemaining = MAX_TIME_IN_TRAP;
        }
    }

    //TODO: Move all this out of here
    private float DistanceToClosestBunny()
    {

        bunnies = MapInventory.AliveBunnyTransforms;

        if (bunnies.Count <= 0) { return float.MaxValue; } //Early exit (no bunnies)
        //Make sure the bear has a path to the bunny for declaring it the closest.

        //Clean up List from nulls - Shouldn't have to do this but, here we are:
        bunnies.RemoveAll(o => o == null);


        List<Transform> reachableBunnies = bunnies.Where(b => Navigator.PathExistsBetween(Navigator.Vect2FromVect3(transform.position), Navigator.Vect2FromVect3(b.position))).ToList<Transform>();
        if (reachableBunnies.Count <= 0) { return float.MaxValue;} //Early exit (can't reach any bunnies)
        return reachableBunnies.Min(b => Vector2.Distance(transform.position, b.position));
    }  

    private Transform ClosestBunny()
    {

        bunnies = MapInventory.AliveBunnyTransforms;

        if (bunnies.Count == 0)
            return null;

        float minDistance = float.MaxValue;
        Transform closestBunny = null;
        List<Transform> reachableBunnies = bunnies.Where(b => Navigator.PathExistsBetween(Navigator.Vect2FromVect3(transform.position), Navigator.Vect2FromVect3(b.position))).ToList<Transform>();

        foreach (Transform bunny in reachableBunnies)
        {
            //Check if its alive!
            BunnyBehaviour bunnyScript = (BunnyBehaviour)bunny.GetComponent<BunnyBehaviour>();
            if (bunnyScript.IsAlive) {
                float distanceToBunny = Vector2.Distance(transform.position, bunny.position);
                if (distanceToBunny < minDistance)
                {
                    closestBunny = bunny;
                    minDistance = distanceToBunny;
                }
            }

        }
        return closestBunny;
    }

    public void Stun() {
        //TEMP
        //TODO: Make another stunned behaviour
        UpdateBehaviour(BehaviourType.eStuckInTrap);
        myStuckTimeRemaining = MAX_TIME_IN_TRAP;
    }

}
