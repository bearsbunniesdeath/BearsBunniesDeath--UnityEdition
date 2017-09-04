using System;
using UnityEngine;

[Serializable]
public class MapPosition {
    public float x;
    public float y;

    public MapPosition(float x1, float y1)
    {
        this.x = x1;
        this.y = y1;
    }

    public MapPosition(int x, int y)
    {
        this.x = (float)x;
        this.y = (float)y;
    }
    
    public Vector2 Vector2
    {
        get
        {
            return new Vector2(x, y);
        }
    }

    //public override bool Equals(object obj)
    //{
    //    var item = obj as MapPosition;
    //    if (item == null)
    //    {
    //        return false;
    //    }
    //    return this.x.Equals(item.x) && this.y.Equals(item.y);
    //}
}
