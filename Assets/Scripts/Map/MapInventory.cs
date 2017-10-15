using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Map
{
    static class MapInventory
    {

        public static  List<Transform> AliveBunnyTransforms = new List<Transform>();

        public static void UpdateGlobalBunnyList() {
            AliveBunnyTransforms.Clear();

            BunnyBehaviour[] bunnyGameObjs = (BunnyBehaviour[])UnityEngine.Object.FindObjectsOfType(typeof(BunnyBehaviour)) as BunnyBehaviour[];
            foreach (BunnyBehaviour bunSct in bunnyGameObjs.Where(b => b.IsAlive)) {
                Transform bTrans = bunSct.GetComponentInParent<Transform>();

                AliveBunnyTransforms.Add(bTrans);
            }

        }

        public static List<Transform> LocateTransforms<T>() where T: MonoBehaviour {
            T[] behaviors = (T[])UnityEngine.Object.FindObjectsOfType(typeof(T)) as T[];
            return behaviors.Select(b => b.GetComponentInParent<Transform>()).ToList();
        } 

    }
}
