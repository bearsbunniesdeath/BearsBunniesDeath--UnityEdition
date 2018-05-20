using Completed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    class explosionHelper
    {
        public static void Explode(AudioClip audClip, GameObject source, float forceMagnitude, float range) {

            AudioSource.PlayClipAtPoint(audClip, source.transform.position);

            Vector2 point = new Vector2(source.transform.position.x, source.transform.position.y);
            Collider2D[] toExplode = Physics2D.OverlapCircleAll(point, range);

            List<GameObject> collection = explosionHelper.GetExplodableGameObjectsFromColliders(toExplode);

            if (collection.Contains(source)) {
                collection.Remove(source);
            }

            foreach (GameObject go in collection)
            {
                Vector2 heading = go.transform.position - source.transform.position;
                if (heading.x == 0 && heading.y == 0)
                {
                    //Avoid errors by having a random angle
                    heading = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
                }

                Vector2 direction = heading / heading.magnitude;

                //For now just get a constant magnitude
                if (go.GetComponent<Rigidbody2D>() != null)
                {

                    if (go.GetComponent<BearBehaviour>() != null)
                    {
                        BearBehaviour explodedBear = go.GetComponent<BearBehaviour>();
                        explodedBear.Stun();
                    }

                    if (go.GetComponent<PlayerBehaviour_1>() != null)
                    {
                        PlayerBehaviour_1 explodedplayer = go.GetComponent<PlayerBehaviour_1>();
                        if (!explodedplayer.IsInvincible)
                        {
                            explodedplayer.Kill();
                        }
                    }

                    go.GetComponent<Rigidbody2D>().AddForce(forceMagnitude * direction);
                }

            }
        }

        public static List<GameObject>  GetExplodableGameObjectsFromColliders(Collider2D[] colliders) {
            List<String> ExplodableTags = new List<string>() {"Bear","Torch", "Trap", "Bomb", "Bunny", "Player"};

            List<GameObject> returnVal = new List<GameObject>();

            foreach (Collider2D item in colliders) {
                if (ExplodableTags.Contains(item.gameObject.tag)) {
                    returnVal.Add(item.gameObject);
                }
            }

            return returnVal;
         }
    }
}
