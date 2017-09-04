using UnityEngine;
using System.Collections;

public class PlayerCenterPointColliderScript : MonoBehaviour {
    private Transform player;       // Reference to the player's transform.	// Use this for initialization
    void Start () {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = player.position;
    }
}
