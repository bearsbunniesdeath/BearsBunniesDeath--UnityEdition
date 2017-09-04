using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Takes a map and verifies properties of the map
/// </summary>
public class MapVerifier
{

    private Map _mapToVerify;

    public MapVerifier(Map map)
    {
        _mapToVerify = map;
    }

    public bool IsMapValid()
    {       
        return Navigator.PathExistsBetween(_mapToVerify.StartPoint.Vector2, _mapToVerify.EndPoint.Vector2);                 
    }

}
