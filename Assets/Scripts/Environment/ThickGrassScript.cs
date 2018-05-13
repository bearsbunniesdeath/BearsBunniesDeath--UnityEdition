using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    class ThickGrassScript : MonoBehaviour, IPlayerLightDimmer, ISpeedInhibitor
    {
        float myHealth;
        /// <summary>
        /// A rigid body from a moving object that damages the grass based on it's speed
        /// </summary>
        Rigidbody2D myDamagingRigidBody = null;
        float MAX_HEALTH = 1.5f;
        float SLOWING_FACTOR = 0.1f;
        public float SlowingFactor;
        float DIMMING_FACTOR = 0.5f;

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
            if (myDamagingRigidBody != null) {
                //TakeDamage(myDamagingRigidBody.velocity.magnitude * Time.deltaTime);
            }
        }

        void FixedUpdate() {
            if (myDamagingRigidBody != null)
            {
                TakeDamage(myDamagingRigidBody.velocity.magnitude * Time.deltaTime);
            }
        }

        void Start()
        {
            DimmingTime = 0.5f;
            myHealth = MAX_HEALTH;
            SlowingFactor = SLOWING_FACTOR;
            DimmingFactor = DIMMING_FACTOR;
        }

        private void TakeDamage(float dmg) {
            //Debug.Log("Grass: Taking damage: " + dmg);
            myHealth -= dmg;
            if (myHealth < 0f) {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            myDamagingRigidBody = other.attachedRigidbody;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            myDamagingRigidBody = null;
        }

    }
}
