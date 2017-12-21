using UnityEngine;
using System.Collections;
using UnityEngine.UI;   //Allows us to use UI.
using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Items;
using Assets.Scripts.Environment;
using System.Linq;

namespace Completed
{

	public class PlayerBehaviour : MonoBehaviour
    {

        public AudioClip reviveSound;				
        public AudioClip dashSound;
        //		private Animator animator;					//Used to store a reference to the Player's animator component.

        private HUDScript myHUD;

        public float myStamina ;

        //Upgradeable stats
        private float myAdditionalSpeed = 0;
        private int myAdditionalLives = 0;
        private int myAdditionalCapacity = 0;

        //Revival Stuff
        const float REVIVAL_DELAY = 3.00f;
        private Timer myReviveDelayTimer;

        private IPlayerLightDimmer myCurrentDimmingElement;
        private float myTimeInThickGrass;

        private int MAX_PLAYER_STAMINA = 100;
        //private int MED_PLAYER_STAMINA = 100;
        //private int LOW_PLAYER_STAMINA = 50;

        public float sprintRateTest;

        public bool IsDead = false;
        public bool myIsInvincible = false;
        public Rigidbody2D rb2D;
        private Collider2D myCollider;
        private Animator myAnimator;

        private const int MAX_ITEMS = 3;
        List<IHoldableObject> myHeldObjects = new List<IHoldableObject>();

        private Vector2 myDashDirection;
        public float Speed;
        //private float BASE_SPEED = 3;
        //private float SPEED_RANGE = 3;
        private float DASH_SPEED = 10;
        private float NOT_DASHING_SPEED = 3;
        private float SPRINT_SPEED = 4;
        private float DASH_TIME = 0.15f;
        private float DASH_STUN_TIME = 0.2f;
        private float DASH_STUN_SPEED = 1f;
        private float DASH_ENERGY = 20f;
        private float TIRED_SPEED = 1.0f;
        private int MAX_SECOND_WIND_COUNT = 15;
        private float myDashTimer;
        private float STAMINA_INCREASE_RATE = 15; //gain of Stamina per second

        private float myTimeSinceLastSprintButtonUp = 0f;
        private float myTimeSinceLastSprint = 0f;

        private float mySecondWindCount = -1f;

        private int mySprintClickCountsPerTime = 0;
        public List<float> mySprintClickTimesArray = new List<float>();
        public const float SPRINT_BUTTON_CHECK_TIME = 1.000f;

        private Renderer myRenderer;

        private AudioSource myAudioSource;
        private Light myLight;
        private float myDefaultLightRange;

        internal void Reset()
        {
            IsDead = false;
            
            mySecondWindCount = -1;
            myAdditionalLives = 0;
            myAdditionalSpeed = 0;
            myAdditionalCapacity = 0;
            transform.rotation = new Quaternion(0, 0, 0,0);
            myHeldObjects.Clear();
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            GetComponent<Rigidbody2D>().angularVelocity = 0;
            if (rb2D != null) {
                rb2D.freezeRotation = true;
            }

            //TODO: Paint UI with a UI Manager
            //if (mySecondWindBar != null) {
            //    mySecondWindBar.rectTransform.localScale = new Vector3(0, 1, 1);
            //}
            myStamina = MAX_PLAYER_STAMINA;
        }

        //Start overrides the Start function of MovingObject
        protected void Start ()
		{
            //Get a component reference to the Player's animator component
            myAnimator = GetComponent<Animator>();

            //Get a component reference to this object's Rigidbody2D
            rb2D = GetComponent<Rigidbody2D>();
            myCollider = GetComponent<Collider2D>();
            myRenderer = GetComponent<SpriteRenderer>();
            myAudioSource = GetComponent<AudioSource>();
            myLight = GetComponentInChildren<Light>(true);
            myDefaultLightRange = myLight.range;
            myDashTimer = -DASH_STUN_TIME - 1;

            myStamina = MAX_PLAYER_STAMINA;
            myReviveDelayTimer = gameObject.AddComponent<Timer>();
            myReviveDelayTimer.ResetTime = REVIVAL_DELAY;

            myHUD = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUDScript>();
        }


		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{

		}


		private void Update ()
		{

            myLight.range = myDefaultLightRange; //Reset the light, so it will be normal unless changed in AdjustSpeedAndLightForThickGrass()
            //TODO: Gradually brighten back to normal

            if (Input.GetKeyDown(KeyCode.LeftShift)) {// || Input.GetKey(KeyCode.Space) || myDashTimer > 0f) {
                ReleaseTopItemInStack();
            }

            if (!IsDead)
            {
                AliveUpdate(); }
            else {
                if (myReviveDelayTimer.CheckForDone()) {
                    Revive();
                }
                else if (!myReviveDelayTimer.IsRunning && myAdditionalLives > 0) {
                    myReviveDelayTimer.StartTimer();
                }
            }

            myHeldObjects = myHeldObjects.Where(o => ( o != null && o.ObjectTransform != null)).ToList();
            foreach (IHoldableObject heldObject in myHeldObjects) {
                if (heldObject.ObjectTransform != null)
                {
                    heldObject.ObjectTransform.position = this.transform.position;
                }
                else {
                    Debug.Log("Why don't we have a Transform?");
                }
            }

            UpdateHUD();

        }

        private void UpdateHUD()
        {
            myHUD.SetStaminaBar(myStamina / (float)MAX_PLAYER_STAMINA);
            myHUD.SetSecondWindBar(Math.Max(mySecondWindCount / MAX_SECOND_WIND_COUNT, 0));
            UpdateItemHUD();
        }

        private void UpdateItemHUD()
        {
            List<eHUDItemType> itemTypeList = new List<eHUDItemType>();
            foreach (IHoldableObject currItem in myHeldObjects) {
                itemTypeList.Add(HUDScript.GetHUDTypeFromIHoldable(currItem));
            }

            myHUD.SetItemStack(itemTypeList);
        }

        private void ReleaseTopItemInStack()
        {
            if (myHeldObjects.Count > 0) {
                IHoldableObject dropMe = myHeldObjects[myHeldObjects.Count - 1];
                Debug.Log("DROPPED" + dropMe.ToString());
                dropMe.IsHeld = false;
                myHeldObjects.Remove(dropMe);
            }
        }

        //private void ForceInsertItem(IHoldableObject item, int index) {
        //    item.MakePickUpNoise();
        //    item.IsHeld = true;
        //    myHeldObjects.Insert(index, item);
        //    CheckForMateableBunnies();

        //    if (myHeldObjects.Count > MAX_ITEMS) {
        //        ReleaseTopItemInStack();
        //    }

        //}

        private void Revive()
        {
            Debug.Log("REVIVAL!");

            explosionHelper.Explode(reviveSound, myAudioSource, gameObject, 1500f, 5);

            //Revival Explosion
            IsDead = false;
            transform.rotation = new Quaternion(0, 0, 0, 0);
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            GetComponent<Rigidbody2D>().angularVelocity = 0;
            if (rb2D != null)
            {
                rb2D.freezeRotation = true;
            }
            myAdditionalLives -= 1;
            myHUD.SetBigText("");
        }

        private void AliveUpdate() {
            if (myStamina >= 0 && mySecondWindCount < 0)
            {
                if (myDashTimer < -DASH_STUN_TIME)
                {
                    myRenderer.material.SetColor("_Color", Color.white);
                    //Has been stunned long enough dash again

                    if (DidDashTapInput())
                    {

                        if (myStamina < 25)
                        {
                            //Just a little spurt of what's left in stamina
                            myStamina = -0.1f;
                            myDashTimer = DASH_TIME * myStamina / DASH_ENERGY;
                            AdjustSpeedAndLightForThickGrass(DASH_SPEED); // TODO: Better way to adjust lights for dimmers without calling this
                        }
                        else
                        {
                            SoundEffectHelper.MakeNoise(myAudioSource, dashSound);
                            //Full power Dash
                            myStamina = myStamina - DASH_ENERGY;
                            myDashTimer = DASH_TIME;
                            AdjustSpeedAndLightForThickGrass(DASH_SPEED); // TODO: Better way to adjust lights for dimmers without calling this
                        }
                    }
                    //For now we are not having a sprint, mechanism only dives

                    //else if (Input.GetKey(KeyCode.Space))
                    //{
                    //    myRenderer.material.SetColor("_Color", Color.blue);
                    //    Speed = AdjustSpeedAndLightForThickGrass(SPRINT_SPEED);
                    //    DoRegularMoveControls();
                    //    myStamina = myStamina - 25 * Time.deltaTime;
                    //    myTimeSinceLastSprint = Time.fixedTime;
                    //}
                    else
                    {

                        if (Time.fixedTime - myTimeSinceLastSprint > 1)
                        {
                            myStamina = Math.Min(myStamina + Time.deltaTime * 15, MAX_PLAYER_STAMINA);
                        }
                        Speed = AdjustSpeedAndLightForThickGrass(NOT_DASHING_SPEED + myAdditionalSpeed);
                        DoRegularMoveControls();
                    }
                }
                else if (myDashTimer > 0f)
                {
                    //In dash
                    AdjustSpeedAndLightForThickGrass(DASH_SPEED); // TODO: Better way to adjust lights for dimmers without calling this
                    myRenderer.material.SetColor("_Color", Color.red);
                    myDashTimer -= Time.deltaTime;
                    Vector3 movement = new Vector3(myDashDirection.x, myDashDirection.y, 0);
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, movement, 1, 1 << LayerMask.NameToLayer("Obstacles"));
                    if (hit.collider == null || hit.distance > 0.1)
                    {
                        movement *= Time.deltaTime;
                        transform.Translate(movement);
                    }

                }
                else
                {
                    //in stun
                    AdjustSpeedAndLightForThickGrass(DASH_STUN_SPEED); // TODO: Better way to adjust lights for dimmers without calling this
                    myRenderer.material.SetColor("_Color", Color.yellow);
                    myDashTimer -= Time.deltaTime;
                    Speed = DASH_STUN_SPEED;
                    DoRegularMoveControls();
                }
            }
            else
            {
                //Tired
                Speed = TIRED_SPEED;
                DoRegularMoveControls();
                myRenderer.material.SetColor("_Color", Color.green);
                if (mySecondWindCount < 0)
                {
                    //First time here (reset)
                    mySecondWindCount = MAX_SECOND_WIND_COUNT;
                }
                else
                {
                    //myTiredTimer = myTiredTimer - (Time.deltaTime * 10f);
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        mySecondWindCount -= 1;
                    }
                    if (mySecondWindCount < 0)
                    {
                        //Break out of tired mode
                        myStamina = DASH_ENERGY * 2 + 1;
                    }
                }
            }
        }

        public bool IsInDarkArea()
        {
            return myCurrentDimmingElement != null;
        }

        private float AdjustSpeedAndLightForThickGrass(float inSpeed)
        {
            if (myCurrentDimmingElement != null)
            {
                myTimeInThickGrass = Math.Min(myTimeInThickGrass + Time.deltaTime, myCurrentDimmingElement.DimmingTime);
                myLight.range = myDefaultLightRange*(1 - (1 - myCurrentDimmingElement.DimmingFactor) * myTimeInThickGrass / myCurrentDimmingElement.DimmingTime);

                if (myCurrentDimmingElement.GetType() == typeof(ThickGrassScript)) {

                    ThickGrassScript asGrass = (ThickGrassScript)myCurrentDimmingElement;

                    //Only grass does this stuff
                    asGrass.TakeDamage(inSpeed * Time.deltaTime);
                    if (asGrass != null)
                    {
                        return inSpeed * asGrass.SlowingFactor; //It might be destroyed after taking damage
                    }
                }



            }
            else {
                myTimeInThickGrass = Math.Max(0, myTimeInThickGrass - 1.5f* Time.deltaTime);
                //When walkin through multiple patches of grass, the darkness shouldn't let up instantly
            }
            return inSpeed;
        }

        private void ReleaseAllItems()
        {
            foreach (IHoldableObject item in myHeldObjects) {
                item.IsHeld = false;
            }
            myHeldObjects.Clear();
        }

        private void AttemptToPickUpItem(GameObject go)
        {

            //Logic to see what I can hold

            //Cast into IHoldable interface
                IHoldableObject asHoldableIterface = (IHoldableObject)(go.GetComponent(typeof(IHoldableObject)));

            if (myHeldObjects.Contains(asHoldableIterface) || myHeldObjects.Count >= MAX_ITEMS + myAdditionalCapacity) { return; }

                //As of now, the rule is one of each type
                if ( !(myHeldObjects.Exists(o => o.TypeOfItem == eItemType.torch) && asHoldableIterface.TypeOfItem == eItemType.torch) && asHoldableIterface.IsHoldableInCurrentState) {
                    if (!asHoldableIterface.IsHeld) {
                        asHoldableIterface.MakePickUpNoise();
                    }
                    asHoldableIterface.IsHeld = true;
                    myHeldObjects.Add(asHoldableIterface);
                    CheckForMateableBunnies();
                }

        }

        private void CheckForMateableBunnies() 
        {
            for (int i = 0; i < myHeldObjects.Count - 1; i++) { //Don't do the last one since we always look forward
                IHoldableObject currItem = myHeldObjects[i];
                IHoldableObject nextItem = myHeldObjects[i + 1];
                if (currItem.TypeOfItem == eItemType.bunny && nextItem.TypeOfItem == eItemType.bunny){
                    BunnyBehaviour currBunny = (BunnyBehaviour)currItem;
                    BunnyBehaviour nextBunny = (BunnyBehaviour)nextItem;
                    if (currBunny.Gender != BunnyBehaviour.eBunnyGender.baby && nextBunny.Gender != BunnyBehaviour.eBunnyGender.baby)
                    {
                        if (currBunny.Gender != nextBunny.Gender && currBunny.Mate == null && nextBunny.Mate == null) {
                            Debug.Log("  ;) ");
                            //Must go both ways, so both can check if the other's still there.
                            currBunny.StartMating(nextBunny);
                            nextBunny.StartMating(currBunny);
                        }
                    }
                }
            }
        }

        private bool DidDashTapInput() {


            if (Input.GetKeyUp(KeyCode.Space)) {
                if (Time.fixedTime - myTimeSinceLastSprintButtonUp < 0.4) {
                    Debug.Log("This was a tap");
                    return true;
                }
            }
            if (!Input.GetKey(KeyCode.Space))
            {
                myTimeSinceLastSprintButtonUp = Time.fixedTime;
            }
            return false;
        }

        private void DoRegularMoveControls()
        {
            Vector2 velocity = new Vector2(0, 0);
            //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction

            //Try a jittery step
            //Speed = Speed * UnityEngine.Random.Range(0.7f, 1.3f);

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                velocity.x = -Speed;
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                velocity.x = Speed;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                velocity.y = -Speed;
            }
            else if (Input.GetKey(KeyCode.UpArrow))
            {
                velocity.y = Speed;
            }


            if (velocity.x != 0 || velocity.y != 0)
            {
                myDashDirection = velocity.normalized * DASH_SPEED;
                Vector3 movement = new Vector3(velocity.x, velocity.y, 0);
                movement *= Time.deltaTime;
                transform.Translate(movement);
                if (myAnimator != null) {
                    myAnimator.Play("Walking");
                }
            }
        }


        //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
        private void OnTriggerEnter2D(Collider2D other)
        {
            //Check if the tag of the trigger collided with is Exit.
            if (other.tag == "Bear")
            {
                if (!myIsInvincible) {
                    Kill();
                }

                myRenderer.material.SetColor("_Color", Color.black);
            }
            else if (other.tag == "Torch" || other.tag == "Bunny" || other.tag == "Trap" || other.tag == "Bomb") {
                AttemptToPickUpItem(other.gameObject);
            }
            else if (other.tag == "ThickGrass" || other.tag == "HouseInterior")
            {
                myCurrentDimmingElement  = other.GetComponent<IPlayerLightDimmer>();
            }
            else if (other.tag == "WearableItem")
            {
                IWearableItem acquiredItem = other.GetComponent<IWearableItem>();

                if (acquiredItem == null) {
                    throw new Exception("Why is this NON-IWearable tagged as one! / possibly something else that's buggy?");
                }

                acquiredItem.MakePickUpNoise();
                if (acquiredItem.TypeOfItem == eWearableItemType.speed)
                {
                    myAdditionalSpeed += acquiredItem.Magnitude;
                }
                else if (acquiredItem.TypeOfItem == eWearableItemType.lives) {
                    myAdditionalLives += (int) acquiredItem.Magnitude;
                }
                else if (acquiredItem.TypeOfItem == eWearableItemType.capacity)
                {
                    Debug.Log("Picked up BackPack");
                    myAdditionalCapacity += (int)acquiredItem.Magnitude;
                }
                //Get rid the of object, so you can't keep picking it up!
                Destroy(other.gameObject);

            }
        }

        public void Kill()
        {
            if (IsDead) {
                return;
            }

            IsDead = true;
            //TODO: Explode any bombs in hands
            //GameObject maybeBombInHand = myHeldObjects.Find(o => o.TypeOfItem == eItemType.bomb)

            //Unfreeze rotation for comedic effect
            rb2D.freezeRotation = false;

                myHUD.SetBigText("DEAD.");
                if (myAdditionalLives > 0) {
                    myHUD.SetBigText("dead?");
                }
            
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "ThickGrass" || other.tag == "HouseInterior")
            {
                myCurrentDimmingElement = null;
            }
        }
        
        private void OnWalkingAnimationEnded()
        {
            myAnimator.Play("Idle");
        }


        //Restart reloads the scene when called.
        private void Restart ()
		{
			//Load the last scene loaded, in this case Main, the only scene in the game.
			//Application.LoadLevel (Application.loadedLevel);
		}


		//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
		private void CheckIfGameOver ()
		{
//			//Check if food point total is less than or equal to zero.
//			if (food <= 0) 
//			{
//				//Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
//				SoundManager.instance.PlaySingle (gameOverSound);
//
//				//Stop the background music.
//				SoundManager.instance.musicSource.Stop();
//
//				//Call the GameOver function of GameManager.
//				GameManager.instance.GameOver ();
			}
		}
	}


