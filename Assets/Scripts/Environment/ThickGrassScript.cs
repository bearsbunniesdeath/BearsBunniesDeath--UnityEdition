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
        Rigidbody2D myDamagingRigidBody = null;
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
            if (myDamagingRigidBody != null)
            {
                //Debug.Log(myDamagingRigidBody.velocity.magnitude);
                if (myDamagingRigidBody.velocity.magnitude > 0.4f)
                {
                    if (!myAudioSource.isPlaying)
                    {
                        float randomStartingTime = UnityEngine.Random.Range(0f, WalkingThroughGrassSound.length);
                        myAudioSource.time = randomStartingTime;
                        myAudioSource.Play();
                    }
                    TakeDamage(myDamagingRigidBody.velocity.magnitude * Time.deltaTime);
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
            //if (other.gameObject.tag == "Player") {
                myDamagingRigidBody = other.attachedRigidbody;
            //}
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            myDamagingRigidBody = null;
            if (myHealth < 0f)
            {
                Destroy(gameObject);
            }
        }

    }
}
