using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Items
{
    static class ItemHelper
    {
        //Sometimes destroy items still exist in scripts myCurrentCollisions, remove them here
        public static void RemoveNullItems(List<GameObject> gOs) {
            List<GameObject> KillMe = new List<GameObject>();
            foreach (GameObject go in gOs)
            {
                if (go == null)
                {
                    KillMe.Add(go);
                }
            }

            foreach (GameObject goKillMe in KillMe)
            {
                gOs.Remove(goKillMe);
            }
        }

        //Sanity check for items that should not be in myCurrentCollisions
        public static void RemoveOutOfRangeItems(List<GameObject> gOs, Vector3 refPosition, float maxDistance) {
            List<GameObject> KillMe = new List<GameObject>();
            foreach (GameObject go in gOs)
            {
                Vector3 difference = go.transform.position - refPosition;
                if (difference.magnitude > maxDistance)
                {
                    KillMe.Add(go);
                }
            }

            foreach (GameObject goKillMe in KillMe)
            {
                gOs.Remove(goKillMe);
            }
        }
    }
}
