using UnityEngine;
using System.Collections.Generic;
using System;
using Assets.Scripts;
using Assets.Scripts.Map;
using System.Collections.ObjectModel;
using System.Linq;

[Serializable]
public abstract class MapBlock
{
    [SerializeField]
    protected Vector2 blockPosition;        //The position of the block within the map
    [SerializeField]
    protected List<DensityArea> DensityAreas;

    [SerializeField]
    protected MapObjectGrid blockObjects;   //A list of all objects created for the block
    [SerializeField]
    public static int SIZE_OF_BLOCK = 8;               //THE MAP SHOULD ONLY CARE ABOUT THIS                                          

    /// <summary>
    /// Builds the block. Generating a list of MapObjects
    /// </summary>
    abstract public void Build();

    public ReadOnlyCollection<MapObject> Objects
    {
        get
        {
            return blockObjects.Objects;
        }
    }

    public Vector2 Position
    {
        get
        {
            return blockPosition;
        }
    }

    protected Vector2 MapPositionFromCellPosition(Vector2 cellPosition)
    {
        if (Position.x > SIZE_OF_BLOCK -1 || Position.y > SIZE_OF_BLOCK - 1 || Position.x < 0 || Position.y < 0) {
            throw new Exception("Sure about that bud? You better know what you're doing.");
        }
        return new Vector2(Position.x * (float)SIZE_OF_BLOCK + cellPosition.x,
                               Position.y * (float)SIZE_OF_BLOCK + cellPosition.y);
    }

    protected void PopulateDensityAreas()
    {

        foreach (DensityArea currDensityArea in DensityAreas)
        {
            foreach (MapPosition mapPos in currDensityArea.DensityCoords)
            {
                if (UnityEngine.Random.Range(0f, 1f) < currDensityArea.Density)
                //TODO: Check for full path blockage
                {
                    //TODO: Duplicate Position Check
                    Vector2 fullMapCoord = MapPositionFromCellPosition(new Vector2(mapPos.x, mapPos.y));
                    if (!this.blockObjects.Objects.Any(o => o.Position.x == fullMapCoord.x && o.Position.y == fullMapCoord.y && o.Tag != "FloorTile"))
                    {
                        GameObject[] gameObjs = DensityAreaObjTypeToGameObject(currDensityArea.GameObjType);
                        blockObjects.Add(new MapObject(gameObjs[UnityEngine.Random.Range(0, gameObjs.Length)], fullMapCoord));
                    }
                    else
                    {
                        Debug.Assert(1 == 1);
                    }
                }
            }
        }
    }

    private GameObject[] DensityAreaObjTypeToGameObject(DensityArea.eMapItems type) {
        if (type == DensityArea.eMapItems.bearTrap)
        {
            return new GameObject[] { MapBuilder.instance.trap };
        }
        else if (type == DensityArea.eMapItems.arrowTrap)
        {
            return new GameObject[] { MapBuilder.instance.arrowShooter };
        }
        else if (type == DensityArea.eMapItems.terrainObstacles)
        {
            return MapBuilder.instance.terrainObjects;
        }
        else if (type == DensityArea.eMapItems.pathTile)
        {
            return new GameObject[] { MapBuilder.instance.pathTile };
        }
        else if (type == DensityArea.eMapItems.bomb)
        {
            return new GameObject[] { MapBuilder.instance.bombObject };
        }
        else if (type == DensityArea.eMapItems.armedBomb)
        {
            return new GameObject[] { MapBuilder.instance.armedBombObject };
        }
        else if (type == DensityArea.eMapItems.thickGrass)
        {
            return new GameObject[] { MapBuilder.instance.thickGrass };
        }
        else if (type == DensityArea.eMapItems.shoes)
        {
            return new GameObject[] { MapBuilder.instance.runningShoes };
        }
        else if (type == DensityArea.eMapItems.reviveItem)
        {
            return new GameObject[] { MapBuilder.instance.reviveItem};
        }
        else if (type == DensityArea.eMapItems.house)
        {
            return new GameObject[] { MapBuilder.instance.house};
        }
        else
        {
            return null;
        }
    }
}


[Serializable]
public class DensityArea
{
    [Serializable]
    //Need this to be an enum instead of GameObject so can serialize
    public enum eMapItems
    {
        bearTrap,
        arrowTrap,
        terrainObstacles,
        pathTile,
        bomb,
        armedBomb,
        shoes,
        reviveItem,
        thickGrass,
        house
    }

    public List<MapPosition> DensityCoords;
    public float Density = 0;
    public eMapItems GameObjType;

    public DensityArea(float inDensity, List<MapPosition> inCoords, eMapItems inGameObjList)
    {
        DensityCoords = inCoords;
        Density = inDensity;
        GameObjType = inGameObjList;
    }

}

public class BlockLayout {
    public List<DensityArea> DensityAreas;
    public List<GameObject> Torches;
    public List<MapPosition> shortCutNodes;

}

