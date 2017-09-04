using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.LevelLogic
{
    /// <summary>
    /// Attach to floormat prefabs that the user needs to touch to complete the level
    /// Reference this in a ILevelCompleteCriteria
    /// This guy only knows when he is being touched, let the ILevelCompleteCriteria take care any other logic
    /// </summary>
    class ContactPoint: MonoBehaviour
    {
        public Sprite OnSprite;
        public Sprite OffSprite;
        public AudioClip ToggleNoise;
        private SpriteRenderer mySpriteRenderer;
        private AudioSource myAudioSource;

        public Boolean IsBeingTouched;

        void Start()
        {
            //TODO: make a satisfying noise when changing state
            //myAudioSource = Get
            mySpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                IsBeingTouched = true;
                if (mySpriteRenderer != null) {
                    mySpriteRenderer.color = Color.red;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                IsBeingTouched = false;
                if (mySpriteRenderer != null)
                {
                    mySpriteRenderer.color = Color.white;
                }
            }
        }

    }
}
