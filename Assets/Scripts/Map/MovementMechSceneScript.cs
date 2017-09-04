using UnityEngine;
using System.Collections;
using Assets.Scripts.Map;

public class MovementMechSceneScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //This is for stuff we need to mock in MechMovement testing environment
        MapInventory.UpdateGlobalBunnyList();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
