using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts;
using Random = UnityEngine.Random;
using Assets.Scripts.Map;
using System.Collections.ObjectModel;

/// <summary>
/// A serializable object used to save the game map
/// </summary>
//[Serializable]
public class Map
{

    private List<MapPosition> criticalPathOfBlocksIndexes;

    private int _rows;
    private int _cols;

    private int _numberOfBlockRows;
    private int _numberOfBlockCols;

    private float _startPointSafeZoneRadius;       //Minimum distance between start point and a bear/enemy

    private MapObjectGrid mapObjects;

    public MapPosition StartPoint;
    public MapPosition EndPoint;

    private const int NUMBER_OF_TRAP_PATCHES = 2;
    private const int NUMBER_OF_JAILS = 2;
    private const int NUMBER_OF_BOMBS = 2;
    private const int NUMBER_OF_ARMED_BOMBS = 2;
    private const int NUMBER_OF_BUNNIES_PER_JAIL = 4;
    private const int NUMBER_OF_BUNNYGUARDS_PER_JAIL = 1;
    private const int NUMBER_OF_DODDADS = 25;
    private const int NUMBER_OF_ARROW_SHOOTERS = 0;

    public enum eMapBlockType
    {
        eCriticalPath,
        eHouse,
        eAllDense
    }


    public Map()
    {
        _rows = 64;
        _cols = 64;
        _numberOfBlockRows = 8;
        _numberOfBlockCols = 8;
        _startPointSafeZoneRadius = 16;

        //Init map object grid
        mapObjects = new MapObjectGrid(_rows, _cols);

        //Create the blocks
        CreateBlocks();

        AddEnvironmetalObjects();

        //Add all NPCs
        PopulateNPCs();

    }

    private void AddEnvironmetalObjects()
    {
        for (int numberOfTraps = 0; numberOfTraps < NUMBER_OF_TRAP_PATCHES; numberOfTraps++)
        {
            AddBearTraps();
        }

        for (int numberOfJails = 0; numberOfJails < NUMBER_OF_JAILS; numberOfJails++)
        {
            //TODO: Keep these from covering Start and Finish icons
            AddBunnyJail(NUMBER_OF_BUNNIES_PER_JAIL, NUMBER_OF_BUNNYGUARDS_PER_JAIL);
        }

        for (int numberOfBombs = 0; numberOfBombs < NUMBER_OF_BOMBS; numberOfBombs++)
        {
            //Clear patches to hold traps and goodies
            MapPosition patchSize = new MapPosition(1, 1);
            MapPosition placement;

            placement = new MapPosition(UnityEngine.Random.Range(0, _rows - 1 - (int)patchSize.x), UnityEngine.Random.Range(0, _cols - 1 - (int)patchSize.y));
            ClearRectangle(placement, new MapPosition(placement.x + patchSize.x, placement.y + patchSize.y));

            //Add trap in center of patch
            mapObjects.Add(new MapObject(MapBuilder.instance.bombObject, new Vector2(placement.x + patchSize.x / 2, placement.y + patchSize.y / 2)));
        }

        for (int numberOfBombs = 0; numberOfBombs < NUMBER_OF_ARMED_BOMBS; numberOfBombs++)
        {
            //Clear patches to hold traps and goodies
            MapPosition patchSize = new MapPosition(0, 0);
            MapPosition placement;

            placement = new MapPosition(UnityEngine.Random.Range(0, _rows - 1 - (int)patchSize.x), UnityEngine.Random.Range(0, _cols - 1 - (int)patchSize.y));
            ClearRectangle(placement, new MapPosition(placement.x + patchSize.x, placement.y + patchSize.y));

            //Add trap in center of patch
            mapObjects.Add(new MapObject(MapBuilder.instance.armedBombObject, new Vector2(placement.x + patchSize.x / 2, placement.y + patchSize.y / 2)));
        }


        for (int numberOfDoodads = 0; numberOfDoodads < NUMBER_OF_DODDADS; numberOfDoodads++)
        {
            //Choose a random doodad from the list
            GameObject newDoodad = MapBuilder.instance.doodads[UnityEngine.Random.Range(0, MapBuilder.instance.doodads.Length)];
            Renderer doodadRenderer = newDoodad.GetComponentInChildren<SpriteRenderer>();
            Vector2 doodadBounds = new Vector2(doodadRenderer.bounds.size.x, doodadRenderer.bounds.size.y); 

            MapPosition placement = new MapPosition(UnityEngine.Random.Range(0, _rows - 1 - (int)Math.Ceiling(doodadBounds.x)), UnityEngine.Random.Range(0, _cols - 1 - (int)Math.Ceiling(doodadBounds.y)));

            //Clear the space once we can get its width and height
            ClearRectangle(placement, new MapPosition(placement.x + Convert.ToInt16(Math.Ceiling(Convert.ToDouble(doodadBounds.x))) - 1, placement.y + Convert.ToInt16(Math.Ceiling(Convert.ToDouble(doodadBounds.y))) - 1));

            //Add doodad 
            mapObjects.Add(new MapObject(newDoodad, new Vector2(placement.x, placement.y)));
        }

        //Add arrow shooters
        for (int numberOfArrowShooters = 0; numberOfArrowShooters < NUMBER_OF_ARROW_SHOOTERS; numberOfArrowShooters++)
        {
            //TODO: Why isn't random placement generalized by now?
            mapObjects.Add(new MapObject(MapBuilder.instance.arrowShooter, mapObjects.OpenPositions[UnityEngine.Random.Range(0, mapObjects.OpenPositions.Count - 1)]));
        }

        //Start and Finish icons
        mapObjects.Add(new MapObject(MapBuilder.instance.StartObject, StartPoint.Vector2));
        mapObjects.Add(new MapObject(MapBuilder.instance.FinishObject, EndPoint.Vector2));

    }

    private void CreateBlocks()
    {

        //MapBlockFactory BlockFactory = new MapBlockFactory();
        MapBlockFactory.RegenerateJSONFiles();

        criticalPathOfBlocksIndexes = GetRandomPreMadePath();

        if (Random.Range(0, 2) == 1)
        {
            MapHelper.FlipMapPositions(criticalPathOfBlocksIndexes, true, _numberOfBlockCols, _numberOfBlockRows);
        }
        if (Random.Range(0, 2) == 1)
        {
            MapHelper.FlipMapPositions(criticalPathOfBlocksIndexes, false, _numberOfBlockCols, _numberOfBlockRows);
        }
        if (Random.Range(0, 2) == 1)
        {
            criticalPathOfBlocksIndexes.Reverse();
        }


        //If a block is not in this list then its flags are all false
        //For each cell in the crit path, check the direction to the adjacent for both neighbours in criticalPathOfBlocksIndexes
        for (int i = 0; i < criticalPathOfBlocksIndexes.Count; i++)
        {
            MapHelper.eDirection? firstPathDir = null;
            MapHelper.eDirection? secondPathDir = null;
            if (i - 1 >= 0)
            {
                firstPathDir = MapHelper.DirectionRelativeIfAdjacent(criticalPathOfBlocksIndexes[i], criticalPathOfBlocksIndexes[i - 1]);
            }
            if (i + 1 < criticalPathOfBlocksIndexes.Count)
            {
                secondPathDir = MapHelper.DirectionRelativeIfAdjacent(criticalPathOfBlocksIndexes[i], criticalPathOfBlocksIndexes[i + 1]);
            }
            bool topOpen = false;
            bool bottomOpen = false;
            bool leftOpen = false;
            bool rightOpen = false;

            if (firstPathDir == MapHelper.eDirection.NORTH | secondPathDir == MapHelper.eDirection.NORTH)
            {
                topOpen = true;
            }

            if (firstPathDir == MapHelper.eDirection.SOUTH | secondPathDir == MapHelper.eDirection.SOUTH)
            {
                bottomOpen = true;
            }

            if (firstPathDir == MapHelper.eDirection.EAST | secondPathDir == MapHelper.eDirection.EAST)
            {
                rightOpen = true;
            }

            if (firstPathDir == MapHelper.eDirection.WEST | secondPathDir == MapHelper.eDirection.WEST)
            {
                leftOpen = true;
            }

            MapBlock newBlock;
            newBlock = MapBlockFactory.MakeMapBlock((int)criticalPathOfBlocksIndexes[i].x, (int)criticalPathOfBlocksIndexes[i].y, leftOpen, topOpen, rightOpen, bottomOpen);
            newBlock.Build();
            foreach (MapObject newObj in newBlock.Objects)
            {
                mapObjects.Add(newObj);
            }
        }

        //Fill the rest of the blocks not on the critical path
        for (int i = 0; i < _numberOfBlockRows; i++)
        {
            for (int j = 0; j < _numberOfBlockCols; j++)
            {
                if (!criticalPathOfBlocksIndexes.Exists(o => o.x == i & o.y == j))
                {
                    MapBlock block = MapBlockFactory.MakeMapBlock(i, j);
                    block.Build();
                    foreach (MapObject newObj in block.Objects)
                    {
                        mapObjects.Add(newObj);
                    }
                }
            }
        }

        //Add a border
        for (int i = -1; i < this._rows + 1; i++)
        {
            //Bottom Row
            mapObjects.Add(new MapObject(MapBuilder.instance.terrainObjects[UnityEngine.Random.Range(0, MapBuilder.instance.terrainObjects.Length)], new Vector2(i, -1)));
            //Top Row
            mapObjects.Add(new MapObject(MapBuilder.instance.terrainObjects[UnityEngine.Random.Range(0, MapBuilder.instance.terrainObjects.Length)], new Vector2(i, this._rows)));
        }
        for (int i = 0; i < this._rows; i++)
        {
            //Left Col
            mapObjects.Add(new MapObject(MapBuilder.instance.terrainObjects[UnityEngine.Random.Range(0, MapBuilder.instance.terrainObjects.Length)], new Vector2(-1, i)));
            //Right Col
            mapObjects.Add(new MapObject(MapBuilder.instance.terrainObjects[UnityEngine.Random.Range(0, MapBuilder.instance.terrainObjects.Length)], new Vector2(this._rows, i)));
        }



        //For now, just use one the center cells in the first and last block and startPoint/endPoint
        StartPoint = new MapPosition((int)criticalPathOfBlocksIndexes[0].x * 8 + 4, (int)criticalPathOfBlocksIndexes[0].y * 8 + 4);
        EndPoint = new MapPosition((int)criticalPathOfBlocksIndexes[criticalPathOfBlocksIndexes.Count - 1].x * 8 + 4, (int)criticalPathOfBlocksIndexes[criticalPathOfBlocksIndexes.Count - 1].y * 8 + 4);

    }

    private void AddBunnyJail(int numOfBunnyPrisoners, int numOfBunnyGuards)
    {
        //Clear patches to hold jail
        MapPosition patchSize = new MapPosition(5, 5);

        MapPosition placement = new MapPosition(UnityEngine.Random.Range(0, _rows - 1 - (int)patchSize.x), UnityEngine.Random.Range(0, _cols - 1 - (int)patchSize.y));
        ClearRectangle(placement, new MapPosition(placement.x + patchSize.x, placement.y + patchSize.y));

        //Add trap in center of patch
        mapObjects.Add(new MapObject(MapBuilder.instance.bunnyJail, new Vector2(placement.x, placement.y)));
        for (int i = 0; i < numOfBunnyPrisoners; i++) {
            mapObjects.Add(new MapObject(MapBuilder.instance.bunnyPrisoner, new Vector2(placement.x + UnityEngine.Random.Range(1,3), placement.y + UnityEngine.Random.Range(1, 2))));
        }
        for (int i = 0; i < numOfBunnyGuards; i++)
        {
            mapObjects.Add(new MapObject(MapBuilder.instance.bunnyGuard, new Vector2(placement.x, placement.y + 3)));
        }

    }

    private void AddBearTraps()
    {
        //Clear patches to hold traps and goodies
        MapPosition patchSize = new MapPosition(3, 3);
        MapPosition placement;

        placement = new MapPosition(UnityEngine.Random.Range(0, _rows - 1 - (int)patchSize.x), UnityEngine.Random.Range(0, _cols - 1 - (int)patchSize.y));
        ClearRectangle(placement, new MapPosition(placement.x + patchSize.x, placement.y + patchSize.y));

        //Add trap in center of patch
        mapObjects.Add(new MapObject(MapBuilder.instance.trap, new Vector2(placement.x + patchSize.x / 2, placement.y + patchSize.y / 2)));
    }

    private void ClearLine(MapPosition pointA, MapPosition pointB)
    {


        Boolean inLineWithEndPoint = false;

        double diffX = pointB.x - pointA.x;
        double diffY = pointB.y - pointA.y;
        int incrementX;
        int incrementY;

        //Deal with either difference being zero
        if (diffX == 0)
        {
            inLineWithEndPoint = true;
        }

        if (diffY == 0)
        {
            inLineWithEndPoint = true;
        }

        if (Math.Abs(diffX) > Math.Abs(diffY))
        {
            incrementY = Math.Sign(diffY);
            incrementX = (int)Math.Round(diffY / diffX) * Math.Sign(diffY);
        }
        else
        {
            incrementX = Math.Sign(diffX);
            incrementY = (int)Math.Round(diffY / diffX) * Math.Sign(diffX);
        }

        Vector2 currPoint = pointA.Vector2;


        int safetyWhile = 0;
        //Debug.Log(currPoint);
        mapObjects.RemoveObjectAtPoint(currPoint);
        while (!inLineWithEndPoint && safetyWhile < 1000)
        {

            for (int i = 0; i != incrementX; i = i + Math.Sign(incrementX))
            {
                currPoint = new Vector2(currPoint.x + Math.Sign(incrementX), currPoint.y);
                //Debug.Log(currPoint);
                mapObjects.RemoveObjectAtPoint(currPoint);

                inLineWithEndPoint = currPoint.y == pointB.y || currPoint.x == pointB.x;
                if (inLineWithEndPoint)
                {
                    break;
                }
            }

            if (inLineWithEndPoint)
            {
                break;
            }

            for (int j = 0; j != incrementY; j = j + Math.Sign(incrementY))
            {
                currPoint = new Vector2(currPoint.x, currPoint.y + Math.Sign(incrementY));
                //Debug.Log(currPoint);
                mapObjects.RemoveObjectAtPoint(currPoint);

                inLineWithEndPoint = currPoint.y == pointB.y || currPoint.x == pointB.x;
                if (inLineWithEndPoint)
                {
                    break;
                }
            }

            safetyWhile++;
        }

        //Debug.Log("In Line with Point B");

        //We are now a straight line away from end point
        if (currPoint.x != pointB.x)
        {
            //Draw out horizontal line until we end
            while ((currPoint.y != pointB.y || currPoint.x != pointB.x) && safetyWhile < 1000)
            {
                currPoint = new Vector2(currPoint.x + Math.Sign(pointB.x - currPoint.x), currPoint.y);
                //Debug.Log(currPoint);
                mapObjects.RemoveObjectAtPoint(currPoint);

                safetyWhile++;
            }
        }
        else if (currPoint.y != pointB.y)
        {
            //Draw out vertical line until we end
            while ((currPoint.y != pointB.y || currPoint.x != pointB.x) && safetyWhile < 1000)
            {
                currPoint = new Vector2(currPoint.x, currPoint.y + Math.Sign(pointB.y - currPoint.y));
                //Debug.Log(currPoint);
                mapObjects.RemoveObjectAtPoint(currPoint);

                safetyWhile++;
            }
        }


        if (safetyWhile >= 1000)
        {
            throw new Exception("Something went wrong");
        }

    }

    private  void ClearRectangle(MapPosition pointA, MapPosition pointB) {
        if (pointA.x > pointB.x || pointA.y > pointB.y)
        {
            throw new Exception("Must got from low coord to high");
        }
        else {
            for (int i = (int)(pointA.x); i <= (int)pointB.x; i++) {
                ClearLine(new MapPosition(i, pointA.y), new MapPosition(i, pointB.y));
            }
        }
    }

    private List<MapPosition> GetRandomPreMadePath()
    {
        //TODO: Premade path needs to be a class
        List<List<MapPosition>> AllOptions = new List<List<MapPosition>>();
        AllOptions.Add(
            new List<MapPosition> {
            new MapPosition(5, 7),
            new MapPosition(5, 6),
            new MapPosition(5, 5),
            new MapPosition(5, 4),
            new MapPosition(4, 4),
            new MapPosition(4, 5),
            new MapPosition(3, 5),
            new MapPosition(3, 4),
            new MapPosition(2, 4),
            new MapPosition(1, 4),
            new MapPosition(1, 3),
            new MapPosition(1, 2),
            new MapPosition(1, 1),
            new MapPosition(2, 1),
            new MapPosition(2, 2),
            new MapPosition(2, 3),
            new MapPosition(3, 3),
            new MapPosition(4, 3),
            new MapPosition(4, 2),
            new MapPosition(4, 1),
            new MapPosition(5, 1),
            new MapPosition(6, 1),
            });

        AllOptions.Add(
            new List<MapPosition> {
            new MapPosition(6, 6),
            new MapPosition(6, 5),
            new MapPosition(5, 5),
            new MapPosition(4, 5),
            new MapPosition(4, 6),
            new MapPosition(4, 7),
            new MapPosition(3, 7),
            new MapPosition(2, 7),
            new MapPosition(2, 6),
            new MapPosition(2, 5),
            new MapPosition(2, 4),
            new MapPosition(1, 4),
            new MapPosition(1, 3),
            new MapPosition(1, 2),
            new MapPosition(1, 1),
            new MapPosition(1, 0),
            new MapPosition(2, 0),
            new MapPosition(3, 0),
            new MapPosition(4, 0),
            new MapPosition(5, 0),
            new MapPosition(6, 0),
            new MapPosition(6, 1),
            new MapPosition(6, 2)});


        AllOptions.Add(
            new List<MapPosition> {
            new MapPosition(1, 6),
            new MapPosition(1, 5),
            new MapPosition(1, 4),
            new MapPosition(1, 3),
            new MapPosition(2, 3),
            new MapPosition(3, 3),
            new MapPosition(3, 4),
            new MapPosition(3, 5),
            new MapPosition(3, 6),
            new MapPosition(4, 6),
            new MapPosition(5, 6),
            new MapPosition(6, 6),
            new MapPosition(6, 5),
            new MapPosition(7, 5),
            new MapPosition(7, 4),
            new MapPosition(7, 3),
            new MapPosition(7, 2),
            new MapPosition(6, 2),
            new MapPosition(6, 1),
            new MapPosition(5, 1),
            new MapPosition(4, 1),
            new MapPosition(3, 1),
            new MapPosition(2, 1),
            new MapPosition(1, 1)});

        int randChoice = Random.Range(0, AllOptions.Count);
        return AllOptions[randChoice];
    }

    private void PopulateNPCs()
    {
        //Add bears to map
        PopulateBears();

        //Add bunnies to map
        PopulateBunnies();
    }

    private void PopulateBears()
    {
        //Figure out how many bears to spawn
        int numberOfBearsToSpawn = Random.Range(MapBuilder.instance.numberOfBears.minimum,
                                                MapBuilder.instance.numberOfBears.maximum);

        //Get valid positions      
        List<Vector2> validSpawnPoints = new List<Vector2>(mapObjects.OpenPositions);
        validSpawnPoints = validSpawnPoints.FindAll(v => (Vector2.Distance(v, StartPoint.Vector2) > _startPointSafeZoneRadius));

        SpawnNPC(validSpawnPoints, MapBuilder.instance.bear, numberOfBearsToSpawn);

        //for (int i = 0; i < numberOfBearsToSpawn; i++)
        //{
        //    //Make sure we have at least one place to add a bear
        //    if (validSpawnPoints.Count == 0)
        //    {
        //        break;
        //    }

        //    //Pick a random position        
        //    Vector2 positionToSpawn = validSpawnPoints[Random.Range(0, validSpawnPoints.Count - 1)];

        //    //Build a bear
        //    MapObject bear = new MapObject(MapBuilder.instance.bear, positionToSpawn);
        //    AddObjectToMap(bear);

        //    validSpawnPoints.Remove(positionToSpawn);
        //}
    }

    private void PopulateBunnies()
    {
        //Figure out how many bunnies to spawn
        int numberOfBunniesToSpawn = Random.Range(MapBuilder.instance.numberOfBunnies.minimum,
                                                  MapBuilder.instance.numberOfBunnies.maximum);

        //Spawn the bunnies!!
        SpawnNPC(new List<Vector2>(mapObjects.OpenPositions), MapBuilder.instance.bunny, numberOfBunniesToSpawn);

    }

    private void SpawnNPC(List<Vector2> openPositions, GameObject npcObject, int numberToSpawn)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {

            //Make sure we have at least one location to spawn NPC
            if (openPositions.Count == 0)
                break;

            //Pick a random position        
            Vector2 positionToSpawn = openPositions[Random.Range(0, openPositions.Count - 1)];

            //Build the NPC
            MapObject npc = new MapObject(npcObject, positionToSpawn);
            AddObjectToMap(npc);

            openPositions.Remove(positionToSpawn);
        }
    }

    public void AddObjectToMap(MapObject obj)
    {
        mapObjects.Add(obj);
    }

    /// <summary>
    /// Creates a new map with the number of rows and cols
    /// </summary>  
    public Map(int rows, int cols)
    {
        if (rows < 1 || cols < 1)
        {
            throw new ArgumentException("Invalid map size");
        }

        //Initialize grid positions
        _rows = rows;
        _cols = cols;
    }

    public ReadOnlyCollection<MapObject> Objects
    {
        get
        {
            return mapObjects.Objects;
        }
    }

    public int Rows
    {
        get
        {
            return _rows;
        }
    }

    public int Cols
    {
        get
        {
            return _cols;
        }
    }
}
