using UnityEngine;
using System.Linq;
using Completed;
using UnityEngine.UI;
using System.Collections.Generic;
using Assets.Scripts.Map;

public class BoardManager : MonoBehaviour {

    public MapBuilder mapBuilder;

    //Public so anyone can spawn things in the board
    public Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.

    private Transform player;       // Reference to the player's transform.
    private Map myMap;

    private HUDScript myHUD;

    /// <summary>
    /// Creates a new scene using a random map
    /// </summary>
    public void SetupScene()
    {
        //Instantiate Board and set boardHolder to its transform
        if (boardHolder == null) {
            boardHolder = new GameObject("Board").transform;
        }

        //Create a random map
        myMap = mapBuilder.Create();

        //Generate each object
        //TODO: Profile to see if this is slow.
        GameObject[] allObjs = mapBuilder.GetAllGameObjects();
        foreach (MapObject mapObj in myMap.Objects)  {
            GameObject gameObj = allObjs.FirstOrDefault(o => (mapObj.Equals(o)));
            if (gameObj != null)
            {
                GameObject instance = Instantiate(gameObj, mapObj.Position, Quaternion.identity) as GameObject;

                //Randomly Choose a Order in layer so we don't get ugly overlap
                SpriteRenderer maybeSpriteRenderder = instance.GetComponent<SpriteRenderer>();
                if (maybeSpriteRenderder == null) {
                    maybeSpriteRenderder = instance.GetComponentInChildren<SpriteRenderer>();
                }
                if (maybeSpriteRenderder != null)
                {
                    maybeSpriteRenderder.sortingOrder = UnityEngine.Random.Range(0, 10000);
                }
                //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                instance.transform.SetParent(boardHolder);
            }
            else { } //TODO: Log an error                          
        }

        //Re-scan the grid graph
        AstarPath.active.Scan();
          
        //Validate the map
        MapVerifier verifier = new MapVerifier(myMap);
        verifier.IsMapValid();
		
		//TODO: Rebuild the map if it's not valid

		//Place the player
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player.position = new Vector3(myMap.StartPoint.x, myMap.StartPoint.y);
        PlayerBehaviour_1 playerScript = player.GetComponent<PlayerBehaviour_1>();
            playerScript.Reset();

        myHUD = GameObject.FindGameObjectWithTag("HUD").GetComponent<HUDScript>();

    }

    public void QuickReset() //Probably only for ease of use for debugging / testing
    {
        //TURN OFF LIGHTS for reset
        GameObject[] lights = GameObject.FindGameObjectsWithTag("Torch");
        foreach (GameObject probablyALight in lights){
             TorchBehaviour asTorchScript = probablyALight.GetComponent<TorchBehaviour>();
            asTorchScript.ForceChangeLight(false);
        }

        //Move player back to start
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = myMap.StartPoint.Vector2;
        PlayerBehaviour asPlayerBehaviour = player.GetComponent<PlayerBehaviour>();
        asPlayerBehaviour.Reset();

        //Randomly Place bears
        GameObject[] bears = GameObject.FindGameObjectsWithTag("Bear");
        foreach (GameObject probablyABear in bears)
        {
            probablyABear.transform.position = new Vector3(Random.Range(0, myMap.Cols), Random.Range(0, myMap.Rows), 0);
        }

    }


    public void ClearBoard()
    {
        var children = new List<GameObject>();
        foreach (Transform child in boardHolder)
        { children.Add(child.gameObject); }
        children.ForEach(child => Destroy(child));
        MapInventory.AliveBunnyTransforms.Clear();

    }

}
