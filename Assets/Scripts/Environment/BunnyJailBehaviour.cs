using UnityEngine;
using System.Collections;

public class BunnyJailBehaviour : MonoBehaviour {

    public bool IsOpen = false;
    public AudioClip JailBreakSound;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !IsOpen)
        {

            AudioSource.PlayClipAtPoint(JailBreakSound, this.transform.position);

            //Player touched the switch! FREEEDOM!
            Transform doorTrans1 = transform.Find("DoorRock1");
            Destroy(doorTrans1.gameObject);
            doorTrans1 = transform.Find("DoorRock2");
            Destroy(doorTrans1.gameObject);
            doorTrans1 = transform.Find("DoorRock3");
            Destroy(doorTrans1.gameObject);
            doorTrans1 = transform.Find("DoorRock4");
            Destroy(doorTrans1.gameObject);

            //Re-scan the graph
            AstarPath.active.Scan();
            IsOpen = true;
        }
    }

}
