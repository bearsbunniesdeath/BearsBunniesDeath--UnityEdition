using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using System.Linq;

public static class Navigator  {

    /// <summary>
    /// Checks for a valid path between to points
    /// </summary>
    /// <param name="a">Point A</param>
    /// <param name="b">Point B</param>  
    public static bool PathExistsBetween(Vector2 a, Vector2 b)
    {
        NNInfo nodeA = AstarPath.active.GetNearest(new Vector3(a.x, a.y, 0));
        NNInfo nodeB = AstarPath.active.GetNearest(new Vector3(b.x, b.y, 0));

        return PathUtilities.IsPathPossible(nodeA.node, nodeB.node);
    }

    public static Vector2 Vect2FromVect3(Vector3 a)
    {
        return (new Vector2(a.x, a.y));
    }

    /// <summary>
    /// Starts a direct path calculation between two points.
    /// Calls the callback method after a path has been calculated
    /// <param name="a">Point A</param>
    /// <param name="b">Point B</param>
    /// <param name="callback">The method to call once the path is calculated</param>
    /// <param name="useNearestNode">Find the nearest walkable node for start/end if they are not walkable</param>
    /// </summary>
    public static void StartPathBetween(Vector2 a, Vector2 b, OnPathDelegate callback, bool useNearestNode = false)
    {
        //Build a path using a and b
        Path path = ABPath.Construct(new Vector3(a.x, a.y, 0), new Vector3(b.x, b.y, 0), callback);
        
        if (!useNearestNode)
            //Strictly make the start and end node to be a and b
            path.nnConstraint = NNConstraint.None;

        //Start the path calculation
        AstarPath.StartPath(path);       
    }

    /// <summary>
    /// Starts a random path calculation between a start point and a point at a random radius
    /// </summary>
    /// <param name="start">Start point</param>
    /// <param name="radius">Distance from the start point the end point will be created</param>
    /// <param name="callback">The method to call once the path is calculated</param>
    public static void StartRandomPath(Vector2 start, int radius, OnPathDelegate callback)
    {
        //Pick a random direction
        Vector2 unitDirection = Random.insideUnitCircle.normalized;

        //Get the rough point that we want to travel towards at the given radius
        Vector2 offset = unitDirection * radius;
        Vector2 roughPoint = new Vector2(start.x + offset.x, start.y + offset.y);

        //Get the actual point we should go towards
        NNInfo actualPoint = AstarPath.active.GetNearest(roughPoint, NNConstraint.Default);   //Default constraint makes sure point is walkable

        //Calculate a path
        Path path = ABPath.Construct(new Vector3(start.x, start.y, 0), (Vector3) actualPoint.node.position, callback);
        path.nnConstraint = NNConstraint.None;

        //Start the path calculation
        AstarPath.StartPath(path);
    }
}
