using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public static class MapHelper
    {
        public enum eDirection
        {
            NORTH, EAST, SOUTH, WEST
        }

        public static Vector2 UnitVectorFromDirection(eDirection direction)
        {
            switch(direction)
            {
                case eDirection.NORTH:
                    return new Vector2(0, 1);
                case eDirection.WEST:
                    return new Vector2(-1, 0);
                case eDirection.EAST:
                    return new Vector2(1, 0);
                case eDirection.SOUTH:
                    return new Vector2(0, -1);
            }

            throw new ArgumentException("Don't know what direction this is.");
        }

        /// <summary>
        /// Returns Relative direction of coord2 to coord 1, if these coords are not 1 cell adjacent returns null
        /// </summary>
        /// <param name="coord1"></param>
        /// <param name="coord2"></param>
        /// <returns>Relative direction of coord2 to coord 1, if these coords are not 1 cell adjacent returns null</returns>
        public static eDirection? DirectionRelativeIfAdjacent(MapPosition coord1, MapPosition coord2) {
            if (coord1.x == coord2.x)
            {
                if (coord1.y == coord2.y + 1)
                {
                    return eDirection.SOUTH;
                }
                else if (coord1.y + 1 == coord2.y)
                {
                    return eDirection.NORTH;
                }
            }
            else if (coord1.y == coord2.y) {
                if (coord1.x == coord2.x + 1)
                {
                    return eDirection.WEST;
                }
                else if (coord1.x + 1 == coord2.x)
                {
                    return eDirection.EAST;
                }
            }

            return null;
        }

        public static List<MapPosition> GetRectangleOfPositionsBetweenPoints(MapPosition coord1, MapPosition coord2) {
            List<MapPosition> retList = new List<MapPosition>();
            for (int i = (int)coord1.x; i <= (int)coord2.x; i++)
            {
                for (int j = (int)coord1.y; j <= (int)coord2.y; j++)
                {
                    retList.Add(new MapPosition(i, j));
                }
            }

            return retList;
        }

        public static double GetHypotenuseLength(double side1, double side2)
        {
            return Math.Sqrt(side1 * side1 + side2 * side2);
        }

        //TODO: There's probably something in C# that does this, but...eh...
        public static List<MapPosition> GetDistinctMapPositions(List<MapPosition> originalList) {
            List<MapPosition> killMe = new List<MapPosition>();
            foreach (MapPosition mapPos in originalList) {
                if (!killMe.Contains(mapPos)) {
                    //get duplicates of this first instance but the the first instance
                    List<MapPosition> test = originalList.FindAll(o => !o.Equals(mapPos) && mapPos.x == o.x && mapPos.y == o.y).ToList();
                    if (test.Count > 0) {
                        killMe.AddRange(test);
                    }
                }
            }

            foreach (MapPosition toBeKilled in killMe) {
                originalList.Remove(toBeKilled);
            }

            return originalList;
        }

        //Check if this is byRef?
        public static void FlipMapPositions(List<MapPosition> criticalPathOfBlocksIndexes, Boolean horizontal, int numberOfCols, int numberOfRows)
        {
            if (horizontal)
            {
                foreach (MapPosition pos in criticalPathOfBlocksIndexes)
                {
                    pos.x = FlipGridCoordValue((int)pos.x, numberOfCols);
                }
            }
            else
            {
                foreach (MapPosition pos in criticalPathOfBlocksIndexes)
                {
                    pos.y = FlipGridCoordValue((int)pos.y, numberOfRows);
                }
            }
        }

        private static float FlipGridCoordValue(float x, int max)
        {
            int asInt = (int)x;
            return Math.Abs(asInt - max) - 1;
        }

        public static void RotateMapObjectsInBlock(List<MapObject> originalPosList, int blockSize) {
            int centerPoint = blockSize / 2; 
            foreach (MapObject MapObj in originalPosList) {

                Math.Cos(2);

            }

        }

    }
}

