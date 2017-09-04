using UnityEngine;
using System.Collections;
using Assets.Scripts;
using Assets.Scripts.Map;
using System;

public class BunnyBehaviour : NPCBehaviour, IHoldableObject
{
    public Sprite deadBunnySprite;
    public Sprite[] hatSprites;

    private AudioSource myAudioSource;
    public AudioClip PickUpSound;

    public bool IsInvincible;
    private bool myHasEdibleCarcass = true;

    private bool myIsHeld;
    private Vector2 targetHopPoint;
    private RaycastHit2D targetHopPointRayCast;

    public const int PRISONER_HAT_INDEX = 0;

    public bool IsHeld
    {
        get { return myIsHeld; }
        set {
            if (value)
            {
                behaviour = BehaviourType.eIdle;
                CancelInvoke(RepeatingMethod.UpdateBehaviour);
                this.GetComponent<Collider2D>().enabled = false;
            }
            else {
                if (IsAlive) {
                    Start();
                    this.GetComponent<Collider2D>().enabled = true;
                }
            } 
        }
    }

    public bool HasEdibleCarcass
    {
        get { return myHasEdibleCarcass; }
        set {
            myHasEdibleCarcass = value;
            this.GetComponent<Collider2D>().enabled = value;
        }
    }

    public int HatIndex
    {
        set {
            Transform hatTrans = transform.Find("Hat");
            SpriteRenderer hatRenderer = hatTrans.GetComponentInChildren<SpriteRenderer>();
            hatRenderer.sprite = hatSprites[value];
        }
    }

    public Transform ObjectTransform
    {
        private set;
        get;
    }

    public eItemType TypeOfItem
    {
        get
        {
            return eItemType.bunny;
        }
    }

    public bool IsHoldableInCurrentState
    {
        get
        {
            return (IsAlive);
        }
    }

    private enum BehaviourType
    {
        eIdle,
        eHopping
    }

    [SerializeField]
    private BehaviourType behaviour;

    // Use this for initialization
    void Start()
    {

        //HatIndex = PRISONER_HAT_INDEX;

        myAudioSource = GetComponent<AudioSource>();
        ObjectTransform = this.transform;
        behaviour = BehaviourType.eIdle;

        StartInvokeRepeating();

        CancelInvoke(RepeatingMethod.UpdatePath);
        ChangeInvokeRate(RepeatingMethod.UpdateBehaviour, 1f, 1f); //Begin hopping at once a second
    }

    // Update is called once per frame
    void Update()
    {
        switch (behaviour)
        {
            case BehaviourType.eIdle:
                break;
            case BehaviourType.eHopping:               
                TryHopping();               
                break;
        }
    }

    protected override void UpdateBehaviour()
    {
        switch (behaviour)
        {
            case BehaviourType.eIdle:
                Vector2 randomDirection = UnityEngine.Random.insideUnitCircle.normalized;                
                targetHopPoint = new Vector2(transform.position.x + randomDirection.x,
                                             transform.position.y + randomDirection.y);
                targetHopPointRayCast = Physics2D.Raycast(transform.position, randomDirection, Mathf.Infinity, 1 << LayerMask.NameToLayer("Obstacles"));
                this.behaviour = BehaviourType.eHopping;
                break;
            case BehaviourType.eHopping:
                this.behaviour = BehaviourType.eIdle;
                break;
        }
    }

    protected override void UpdatePath()
    {
        Debug.Log("Tried to update the bunny path but the bunny doesn't use a path.");
    }

    /// <summary>
    /// Tries to hop towards the target hop point. If there is a collider in the way then
    /// don't attempt it
    /// </summary>
    private void TryHopping()
    {               
        if (Vector2.Distance(targetHopPointRayCast.point, transform.position) > 2)       
            MoveTowards(targetHopPoint, 3f * Time.deltaTime);                        
    }

    //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
    private void OnTriggerEnter2D(Collider2D other)
    {     
        if (other.tag == "Bear")
        {
            if (!IsInvincible && IsAlive)
            {
                //Bunny is ded. RIP
                IsAlive = false;

                MapInventory.UpdateGlobalBunnyList();

                //Change sprite to nothing since the bear animation has bunny in bears mouth
                SpriteRenderer renderer = GetComponent<SpriteRenderer>();
                renderer.sprite = null;
                foreach (Collider c in gameObject.GetComponents<Collider>())
                {
                    c.enabled = false;
                }
                Rigidbody2D rigidBodyToKill = GetComponent<Rigidbody2D>();
                Destroy(rigidBodyToKill);
            }
        }

    }

    public void DropCorpse()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = deadBunnySprite;
    }

    public void MakePickUpNoise()
    {
        SoundEffectHelper.MakeNoise(myAudioSource, PickUpSound);
    }
}