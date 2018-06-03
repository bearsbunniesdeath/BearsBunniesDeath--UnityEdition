using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Builder class used to generate maps
/// </summary>
public class MapBuilder : MonoBehaviour {

    public static MapBuilder instance = null;

    public MapBuilder()
    {
        if (instance == null) 
            instance = this;
        else
            throw new Exception("Cannot create more than one MapBuilder");
    }

    // Using Serializable allows us to embed a class with sub properties in the inspector.
    [Serializable]
    public class Count
    {
        public int minimum;             //Minimum value for our Count class.
        public int maximum;             //Maximum value for our Count class.


        //Assignment constructor.
        public Count(int min, int max)
        {
            minimum = min;
            maximum = max;
        }
    }
    
    #region "Map settings"
    public int rows = 64;                                           //Number of rows in our gameboard
    public int columns = 64;                                        //Number of columns in our gameboard
    public GameObject[] floorTiles;                                 //Array of floor prefabs.
    public GameObject pathTile;
    public GameObject[] terrainObjects;                             //Array of terrain objects (tree, rocks, etc)
    public GameObject[] jumpableObjects;                             
    public GameObject thickGrass;
    public GameObject berryBush;
    public GameObject StartObject;                             //temp icon "S" to show the start
    public GameObject FinishObject;                             //temp icon "F" to show the Finish
    public GameObject TorchObject;                             //temp icon "F" to show the Finish
    public GameObject bear;                                         //Bear object
    public GameObject bunny;                                        //Bunny object
    public GameObject bunnyPrisoner;
    public GameObject bunnyGuard;
    public GameObject trap;                                        //Trap object
    public GameObject bunnyJail;
    public GameObject arrow;
    public GameObject arrowShooter;                                                                       
    public GameObject bombObject;
    public GameObject armedBombObject;
    public GameObject runningShoes;
    public GameObject reviveItem;
    public GameObject backPack;
    public GameObject house;
    public GameObject[] doodads;
    public Count numberOfBears;                                     //Number of bears to spawn on the map
    public Count numberOfBunnies;                                   //Number of bunnies to spawn on the map
    #endregion

    #region "Builders"
    /// <summary>
    /// Creates a random map using map settings
    /// </summary>
    /// <returns></returns>
    public Map Create()
    {
        Map map = new Map();

        //Loop along x axis, starting from -1 (to fill corner) with floor
        for (int x = -1; x < columns + 1; x++)
        {
            //Loop along y axis, starting from -1 to place floor tile prefabs and prepare to instantiate it.
            for (int y = -1; y < rows + 1; y++)
            {
                MapObject obj = new MapObject(floorTiles[Random.Range(0, floorTiles.Length)], new Vector2(x, y));
                map.AddObjectToMap(obj);               
            }
        }
    
        //LayoutObjectAtRandom(map, terrainObjects, countOfTerrainObjects.minimum, countOfTerrainObjects.maximum);

        return map;
    }

    /// <summary>
    /// Creates a map from a saved file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public Map Create(string path)
    {
        throw new NotImplementedException();
    }

    #endregion

    //TODO: Find a better way to convert MapObject -> GameObject
    //This is ugly
    public GameObject[] GetAllGameObjects()
    {
        List<GameObject> objs = floorTiles.Concat(terrainObjects).ToList();
        objs.AddRange(floorTiles.Concat(jumpableObjects).ToList());
        objs.Add(pathTile);
        objs.Add(thickGrass);
        objs.Add(berryBush);
        objs.Add(StartObject);
        objs.Add(FinishObject);
        objs.Add(bear);
        objs.Add(bunny);
        objs.Add(TorchObject);
        objs.Add(trap);
        objs.Add(bunnyJail);
        objs.Add(bunnyPrisoner);
        objs.Add(bunnyGuard);
        objs.Add(arrow);
        objs.Add(arrowShooter);
        objs.Add(bombObject);
        objs.Add(armedBombObject);
        objs.Add(runningShoes);
        objs.Add(reviveItem);
        objs.Add(backPack);
        objs.Add(house);
        foreach (GameObject item in doodads) {
            objs.Add(item);
        }

        return objs.ToArray();
    }

    void Awake()
    {       
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }
	
}
