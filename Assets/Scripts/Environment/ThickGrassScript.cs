using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Environment
{
    class ThickGrassScript : MonoBehaviour 
    {
        float myHealth;
        float MAX_HEALTH = 1.5f;
        float SLOWING_FACTOR = 0.5f;
        public float SlowingFactor;
        float DIMMING_FACTOR = 0.5f;
        public float DimmingFactor;

        void Start()
        {
            myHealth = MAX_HEALTH;
            SlowingFactor = SLOWING_FACTOR;
            DimmingFactor = DIMMING_FACTOR;
        }

        public void TakeDamage(float dmg) {
            myHealth -= dmg;
            if (myHealth < 0) {
                Destroy(gameObject);
            }
        }

    }
}
