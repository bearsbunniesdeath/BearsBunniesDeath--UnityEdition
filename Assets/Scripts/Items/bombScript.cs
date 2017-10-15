using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Assets.Scripts;
using Completed;
using Assets.Scripts.Items;

public class bombScript : MonoBehaviour, IHoldableObject {

    private Rigidbody2D myRigidBody;
    private Light myLight;
    public const float ForceMagnitude = 1500f;
    public const float TriggerRange = 0.5f;

    public AudioClip PickUpNoise;
    public AudioClip ArmNoise;
    public AudioClip TickNoise;

    public bool IsSet;
    private const float MAX_TIME_TO_ARM = 1.50f;
    private float myTimeToArm = 0f;

    public Sprite ArmedSprite;


    private AudioSource myAudioSource;
    public AudioClip ExplosionSound;
    private List<GameObject> myCurrentCollisions = new List<GameObject>();

    private bool myIsHeld = false;

    public Transform ObjectTransform
    {
        private set;
        get;
    }

    public bool IsHeld
    {
        get{
            return myIsHeld;
        }

        set
        {
            if (!value && myIsHeld) {
//Don't let the player reset timer by picking up, promote cooking bomb
                if (myTimeToArm == 0) {
                    myTimeToArm = MAX_TIME_TO_ARM;
                }
                //SoundEffectHelper.MakeNoise(myAudioSource, TickNoise);
            }
            myIsHeld = value;
        }
    }

    public bool IsHoldableInCurrentState
    {
        get
        {
            return !IsSet;
        }
    }

    public eItemType TypeOfItem
    {
        get
        {
            return eItemType.bomb;
        }
    }

    public void MakePickUpNoise()
    {
        SoundEffectHelper.MakeNoise(myAudioSource, PickUpNoise);
    }

    // Use this for initialization
    void Start () {
        myLight = transform.GetComponentInChildren<Light>(true);
        myLight.intensity = 0f;
        ObjectTransform = this.transform;
        myRigidBody = GetComponent<Rigidbody2D>();
        myAudioSource = GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update () {

        if (myLight.intensity > 0) {
            myLight.intensity = myLight.intensity - (Time.deltaTime);
        }

        if (IsSet)
        {

            ItemHelper.RemoveNullItems(myCurrentCollisions);
            ItemHelper.RemoveOutOfRangeItems(myCurrentCollisions, transform.position, 2f);

            foreach (GameObject go in myCurrentCollisions)
            {
                Vector2 heading = go.transform.position - gameObject.transform.position;
                if (heading.magnitude < TriggerRange && IsTriggeringObject(go))
                {
                    Explode();
                    break;
                }
            }
        }
        else if (myTimeToArm > 0f) {
            //Countdown to arm! Get outta here!
            myTimeToArm = myTimeToArm - Time.deltaTime;
            if (myTimeToArm < 0f) {
                SoundEffectHelper.MakeNoise(myAudioSource, ArmNoise);
                IsSet = true;
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                renderer.sprite = ArmedSprite;
                GetComponent<CircleCollider2D>().radius = 1.0f;
            }
        }

    }


    private bool IsTriggeringObject(GameObject go)
    {
        return go.tag == "Bunny" || go.tag == "Bomb" || go.tag == "Bear" || go.tag == "Player";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        myCurrentCollisions.Add(other.gameObject);
    }

    private void Explode()
    {

        myLight.intensity = 1.0f;

        explosionHelper.Explode(ExplosionSound, myAudioSource, gameObject, ForceMagnitude, 1);

        IsSet = false;
        GetComponent<SpriteRenderer>().sprite = null;

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (myCurrentCollisions.Contains(other.gameObject))
        {
            myCurrentCollisions.Remove(other.gameObject);
        }
    }

 
}
