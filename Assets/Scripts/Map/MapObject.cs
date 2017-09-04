using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Defines a GameObject at a certain location on the map
/// </summary>
[Serializable]
public class MapObject {

    public const string TORCH_NAME_STRING = "testTorch";

    private MapPosition _pos;
    private GameObjectIdentifier _id;

    public MapObject(GameObject obj, Vector2 pos)
    {
        _pos = new MapPosition(pos.x, pos.y);
        _id = new GameObjectIdentifier(obj.name, obj.tag);
    }

    public string Name
    {
        get
        {
            return _id.Name;
        }
    }

    public string Tag
    {
        get
        {
            return _id.Tag;
        }
    }

    public Vector2 Position
    {
        get
        {
            return new Vector2(_pos.x, _pos.y);
        }
    }

    public bool Equals(GameObject gameObj)
    {
        return _id.Name.Equals(gameObj.name) && _id.Tag.Equals(gameObj.tag);
    }
}
