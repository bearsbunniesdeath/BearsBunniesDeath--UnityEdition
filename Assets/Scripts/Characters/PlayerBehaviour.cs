using UnityEngine;
using System.Collections;
using UnityEngine.UI;   //Allows us to use UI.
using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Items;
using Assets.Scripts.Environment;

namespace Completed
{

	public class PlayerBehaviour : MonoBehaviour
    {

        //		public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
        //		public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
        //		public AudioClip eatSound1;					//1 of 2 Audio clips to play when player collects a food object.
        //		public AudioClip eatSound2;					//2 of 2 Audio clips to play when player collects a food object.
        //		public AudioClip drinkSound1;				//1 of 2 Audio clips to play when player collects a soda object.
        //		public AudioClip drinkSound2;				//2 of 2 Audio clips to play when player collects a soda object.
        //		public AudioClip gameOverSound;				//Audio clip to play when player dies.
        //
        //		private Animator animator;					//Used to store a reference to the Player's animator component.

        private Image myStaminaBar;
        private Text mySecondWindText;
        private Image mySecondWindBar;
        private Image myBombHUD;
        private Image myBunnyHUD;
        private Image myTorchHUD;
        private Image myTrapHUD;
        public float myStamina ;

        private ThickGrassScript myCurrentThickGrass;

        private int MAX_PLAYER_STAMINA = 100;
        //private int MED_PLAYER_STAMINA = 100;
        //private int LOW_PLAYER_STAMINA = 50;

        public float sprintRateTest;

        public bool IsDead = false;
        public bool myIsInvincible = false;
        public Rigidbody2D rb2D;
        private Collider2D myCollider;
        private Animator myAnimator;
        List<GameObject> myCurrentCollisions = new List<GameObject>();
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

            transform.rotation = new Quaternion(0, 0, 0,0);
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            GetComponent<Rigidbody2D>().angularVelocity = 0;
            if (rb2D != null) {
                rb2D.freezeRotation = true;
            }

            //TODO: Paint UI with a UI Manager
            if (mySecondWindBar != null) {
                mySecondWindBar.rectTransform.localScale = new Vector3(0, 1, 1);
            }
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

            myStaminaBar = GameObject.Find("StaminaBar").GetComponent<Image>();
            mySecondWindBar = GameObject.Find("SecondWindBar").GetComponent<Image>();
            mySecondWindBar.rectTransform.localScale = new Vector3(0, 1, 1);

            myBombHUD = GameObject.Find("BombHUD").GetComponent<Image>();
            myBunnyHUD = GameObject.Find("BunnyHUD").GetComponent<Image>();
            myTorchHUD = GameObject.Find("TorchHUD").GetComponent<Image>();
            myTrapHUD = GameObject.Find("TrapHUD").GetComponent<Image>();

            //TODO: If any of these doesn't exist, don't try to update HUD


            myStamina = MAX_PLAYER_STAMINA;
        }


		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{

		}


		private void Update ()
		{
            ItemHelper.RemoveNullItems(myCurrentCollisions);
            ItemHelper.RemoveOutOfRangeItems(myCurrentCollisions, transform.position, 1f);

            myLight.range = myDefaultLightRange; //Reset the light, so it will be normal unless changed in AdjustSpeedAndLightForThickGrass()
            //TODO: Gradually brighten back to normal

            if (!Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Space)) {
                ReleaseAllItems();
            }
                  
            if (!IsDead)
            {
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
                            }
                            else
                            {
                                myAudioSource.Play();
                                 //Full power Dash
                                 myStamina = myStamina - DASH_ENERGY;
                                myDashTimer = DASH_TIME;
                            }
                        }
                        else if (Input.GetKey(KeyCode.Space))
                        {
                            myRenderer.material.SetColor("_Color", Color.blue);
                            Speed = AdjustSpeedAndLightForThickGrass(SPRINT_SPEED);
                            DoRegularMoveControls();
                            myStamina = myStamina - 25 * Time.deltaTime;
                            myTimeSinceLastSprint = Time.fixedTime;
                        }
                        else
                        {
                            //Normal State
                            if (Input.GetKey(KeyCode.LeftShift))
                            {
                                LookForItemsToHold();
                            }

                            if (Time.fixedTime - myTimeSinceLastSprint > 1) {
                                myStamina = Math.Min(myStamina + Time.deltaTime * 15, MAX_PLAYER_STAMINA);
                            }
                            Speed = AdjustSpeedAndLightForThickGrass(NOT_DASHING_SPEED);
                            DoRegularMoveControls();
                        }
                    }
                    else if (myDashTimer > 0f)
                    {
                        //In dash



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
                        myRenderer.material.SetColor("_Color", Color.yellow);
                        myDashTimer -= Time.deltaTime;
                        Speed = DASH_STUN_SPEED;
                        DoRegularMoveControls();
                    }
                }
                else {
                    //Tired
                    Speed = TIRED_SPEED;
                    DoRegularMoveControls();
                    myRenderer.material.SetColor("_Color", Color.green);
                    if (mySecondWindCount < 0)
                    {
                        //First time here (reset)
                        mySecondWindCount = MAX_SECOND_WIND_COUNT;
                    }
                    else {
                        //myTiredTimer = myTiredTimer - (Time.deltaTime * 10f);
                        if (Input.GetKeyDown(KeyCode.Space))
                        {
                            mySecondWindCount -= 1;
                        }
                        if (mySecondWindCount < 0) {
                            //Break out of tired mode
                            myStamina = DASH_ENERGY*2+1;
                        }
                    }
                    if (mySecondWindBar != null)
                    {
                        mySecondWindBar.rectTransform.localScale = new Vector3(Math.Max(mySecondWindCount / MAX_SECOND_WIND_COUNT, 0), 1, 1);
                    }
                }

               
            }

            if (myStaminaBar != null){
                myStaminaBar.rectTransform.localScale = new Vector3(myStamina / (float)MAX_PLAYER_STAMINA, 1, 1);
                UpdateItemHUD();
            }            

            foreach (IHoldableObject heldObject in myHeldObjects) {
                heldObject.ObjectTransform.position = this.transform.position;
            }

        }

        public bool IsInThickGrass()
        {
            return myCurrentThickGrass != null;
        }

        private float AdjustSpeedAndLightForThickGrass(float inSpeed)
        {
            if (myCurrentThickGrass != null)
            {

                myLight.range = myDefaultLightRange* myCurrentThickGrass.DimmingFactor;

                myCurrentThickGrass.TakeDamage(inSpeed * Time.deltaTime);
                if (myCurrentThickGrass != null)
                {
                    return inSpeed * myCurrentThickGrass.SlowingFactor; //It might be destroyed after taking damage
                }
                    
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

        private void LookForItemsToHold()
        {

            //TODO: Use logic in bomb script to delete nulls (destroyed items) from myCurrentCollisions

            //Logic to see what I can hold

            foreach (GameObject itemInReach in myCurrentCollisions) {
                //Cast into IHoldable interface
                IHoldableObject asHoldableIterface = (IHoldableObject)(itemInReach.GetComponent(typeof(IHoldableObject)));

                //As of now, the rule is one of each type
                if ( !myHeldObjects.Exists(o => o.TypeOfItem == asHoldableIterface.TypeOfItem) && asHoldableIterface.IsHoldableInCurrentState) {
                    if (!asHoldableIterface.IsHeld) {
                        asHoldableIterface.MakePickUpNoise();
                    }
                    asHoldableIterface.IsHeld = true;
                    myHeldObjects.Add(asHoldableIterface);
                }

            }

        }

        private void UpdateItemHUD()
        {
            ////Change the alpha of the color of each image for visibility
            //Color onColor = new Color(255, 255, 255, 255);
            //Color offColor = new Color(255, 255, 255, 50);

            //myBombHUD.color = myHeldObjects.Exists(o => o.TypeOfItem == eItemType.bomb)?onColor:offColor;
            //myBunnyHUD.color = myHeldObjects.Exists(o => o.TypeOfItem == eItemType.bunny) ? onColor : offColor;
            //myTorchHUD.color = myHeldObjects.Exists(o => o.TypeOfItem == eItemType.torch) ? onColor : offColor;
            //myTrapHUD.color = myHeldObjects.Exists(o => o.TypeOfItem == eItemType.trap) ? onColor : offColor;

            myBombHUD.enabled = myHeldObjects.Exists(o => o.TypeOfItem == eItemType.bomb);
            myBunnyHUD.enabled = myHeldObjects.Exists(o => o.TypeOfItem == eItemType.bunny);
            myTorchHUD.enabled = myHeldObjects.Exists(o => o.TypeOfItem == eItemType.torch);
            myTrapHUD.enabled = myHeldObjects.Exists(o => o.TypeOfItem == eItemType.trap);
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

        private float GetSprintButtonRate()
        {
            if (Input.GetKeyDown(KeyCode.Space)) {
                mySprintClickCountsPerTime += 1;
                mySprintClickTimesArray.Add(Time.fixedTime);
                if (mySprintClickTimesArray.Count > 10) {
                    mySprintClickTimesArray.RemoveAt(0);
                }
            }

            float rangeOfCheck = Time.fixedTime - SPRINT_BUTTON_CHECK_TIME;

            int countInTimeRange = mySprintClickTimesArray.FindAll(delegate (float item)
            {
                return item > rangeOfCheck;
            }).Count;

            return countInTimeRange;
            //returns amounts of sprint clicks in 1 second
        }


        //private void SpamToSprintBehaviour() {
        //    float sprintRate = GetSprintButtonRate();
        //    sprintRateTest = sprintRate;

        //    if (sprintRate > 0 && myStamina > 0)
        //    {
        //        myStamina -= sprintRate * 2.5f * Time.deltaTime;
        //    }
        //    else
        //    {
        //        if (myStamina < MAX_PLAYER_STAMINA)
        //        {
        //            myStamina += STAMINA_INCREASE_RATE * Time.deltaTime;
        //        }
        //    }

        //    if (myStamina > MED_PLAYER_STAMINA)
        //    {
        //        Speed = BASE_SPEED + SPEED_RANGE * sprintRate / 10;
        //        myRenderer.material.SetColor("_Color", Color.white);
        //    }
        //    else if (myStamina > LOW_PLAYER_STAMINA)
        //    {
        //        //Speed is slower
        //        Speed = BASE_SPEED + SPEED_RANGE * 0.6666f * sprintRate / 10;
        //        myRenderer.material.SetColor("_Color", Color.red);
        //    }
        //    else
        //    {
        //        //TODO: DECIDE if we want have constant slow sleep for low stamina OR shorter range, and still use spacebar spamming
        //        Speed = BASE_SPEED + SPEED_RANGE * 0.3333f * sprintRate / 10;
        //        myRenderer.material.SetColor("_Color", Color.blue);
        //    }

        //    DoRegularMoveControls();

        //}

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
        private void OnTriggerEnter2D (Collider2D other)
		{
            //Check if the tag of the trigger collided with is Exit.
            if (other.tag == "Bear")
            {
                if (!myIsInvincible) {
                    Kill();
                }                
                
                myRenderer.material.SetColor("_Color", Color.black);
            }
            if (other.tag == "Torch" || other.tag == "Bunny" || other.tag == "Trap" || other.tag == "Bomb") {
                myCurrentCollisions.Add(other.gameObject);
            }
            if (other.tag == "ThickGrass")
            {
                myCurrentThickGrass  = other.GetComponent<ThickGrassScript>();
            }
        }

        public void Kill()
        {
            IsDead = true;
            //TODO: Explode any bombs in hands
            //GameObject maybeBombInHand = myHeldObjects.Find(o => o.TypeOfItem == eItemType.bomb)

            //Unfreeze rotation for comedic effect
            rb2D.freezeRotation = false;

            if (GameObject.Find("DeadText") != null && GameObject.Find("DeadText").GetComponent<Text>() != null) {
                Text deadtext = GameObject.Find("DeadText").GetComponent<Text>();
                deadtext.rectTransform.localScale = new Vector3(4, 1, 1);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (myCurrentCollisions.Contains(other.gameObject)) {
                myCurrentCollisions.Remove(other.gameObject);
            }
            if (other.tag == "ThickGrass")
            {
                myCurrentThickGrass = null;
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


