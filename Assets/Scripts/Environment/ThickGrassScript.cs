using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    class ThickGrassScript : MonoBehaviour, ILightInhibitor, ISpeedInhibitor
    {

        public AudioClip WalkingThroughGrassSound;
        private AudioSource myAudioSource;
        float myHealth;
        /// <summary>
        /// A rigid body from a moving object that damages the grass based on it's speed
        /// </summary>
       List< Rigidbody2D> myDamagingRigidBodies = new List<Rigidbody2D>();
        float MAX_HEALTH = 1.0f;
        float SLOWING_FACTOR = 0.19f;
        public float SlowingFactor;
        float DIMMING_FACTOR = 0.35f;

        public float DimmingTime
        {
            private set;
            get;
        }

        public float DimmingFactor
        {
            private set;
            get;
        }

        public float SlowFactor
        {
            get
            {
                return SLOWING_FACTOR;
            }
        }

        void Update(){
        }

        void FixedUpdate() {
            if (myDamagingRigidBodies.Count > 0)
            {
                if (myDamagingRigidBodies.Exists(x => x.velocity.magnitude > 0.4f))
                {
                    if (!myAudioSource.isPlaying)
                    {
                        float randomStartingTime = UnityEngine.Random.Range(0f, WalkingThroughGrassSound.length);
                        myAudioSource.time = randomStartingTime;
                        myAudioSource.Play();
                    }
                    foreach (Rigidbody2D damager in myDamagingRigidBodies.Where(x => x.velocity.magnitude > 0.4f)) {
                        TakeDamage(damager.velocity.magnitude * Time.deltaTime);
                    }
                }
                else if (myAudioSource.isPlaying) {
                    myAudioSource.Stop();
                }
                    
            }
            else {
                if (myAudioSource.isPlaying)
                {
                    myAudioSource.Stop();
                }
            }
        }

        void Start()
        {
            myAudioSource = GetComponent<AudioSource>();

            DimmingTime = 0.5f;
            myHealth = MAX_HEALTH;
            SlowingFactor = SLOWING_FACTOR;
            DimmingFactor = DIMMING_FACTOR;
        }

        private void TakeDamage(float dmg) {
            //Debug.Log("Grass: Taking damage: " + dmg);
            myHealth -= dmg;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //TODO: Put in helper.
            //A held item should not be considered as a damagingRigidBody
            //Should we just disable all colliders when held?
            MonoBehaviour[] list = other.gameObject.GetComponentsInParent<MonoBehaviour>();
            if (list.Length == 0)
            {
                list = other.gameObject.GetComponents<MonoBehaviour>();
            }
            foreach (MonoBehaviour mb in list)
            {
                if (mb is IHoldableObject)
                {
                    IHoldableObject holdable = (IHoldableObject)mb;
                    if (holdable.IsHeld) {
                        return;
                    }
                }
            }

            if (!myDamagingRigidBodies.Contains(other.attachedRigidbody)) {
                myDamagingRigidBodies.Add(other.attachedRigidbody);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (myDamagingRigidBodies.Contains(other.attachedRigidbody))
            {
                myDamagingRigidBodies.Remove(other.attachedRigidbody);
            }
            if (myHealth < 0f)
            {
                Destroy(gameObject);
            }
        }

    }
}
