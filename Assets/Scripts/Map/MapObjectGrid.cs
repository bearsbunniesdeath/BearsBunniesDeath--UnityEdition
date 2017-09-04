using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Map
{
    /// <summary>
    /// A list of MapObjects that keeps track of open positions
    /// </summary>
    [Serializable]
    public class MapObjectGrid
    {
        private int _length;
        private int _width;

        private List<MapObject> _mapObjects = new List<MapObject>();
        private List<Vector2> _openPositions = new List<Vector2>();

        public MapObjectGrid(int length, int width)
        {
            if (length < 1 || width < 1)
                throw new ArgumentException("Length and width must be greater than zero");

            _length = length;
            _width = width;

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    _openPositions.Add(new Vector2(i, j));
                }
            }
        }

        public int Length
        {
            get
            {
                return _length;
            }
        }

        public int Width
        {
            get
            {
                return _width;
            }
        }

        /// <summary>
        /// Add a MapObject to the grid. Removes the open position.
        /// </summary>
        /// <param name="item">MapObject to add to grid</param>
        public void Add(MapObject item)
        {          
            Vector2 toBeRemoved = _openPositions.FirstOrDefault(v => (v.x == item.Position.x && v.y == item.Position.y));
            if (toBeRemoved != null) {
                _openPositions.Remove(toBeRemoved);
                _mapObjects.Add(item);
            } else {
                throw new ArgumentException("Cannot add item to the map object. Position is not open");
            }
        }

        /// <summary>
        /// Removes a MapObject from the grid. Opens the closed position.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(MapObject item)
        {
            _mapObjects.Remove(item);
            _openPositions.Add(item.Position); //Not sure if this works
        }

        /// <summary>
        /// Removes a MapObject from the grid. Opens the closed position.
        /// </summary>
        /// <param name="item"></param>
        public void RemoveObjectAtPoint(Vector2 coord, List<string> ignoreTags = null)
        {

            if (ignoreTags == null) {
                //Cannot have a default List<String> so this is what we actaully want as defualt
                ignoreTags = new List<string> { "FloorTile" };
            }

            List<MapObject> foundOnSpot;
            foundOnSpot = _mapObjects.FindAll(v => (v.Position.x == coord.x && v.Position.y == coord.y && !ignoreTags.Contains(v.Tag)));

            foreach(MapObject killMe in foundOnSpot) {

                if (killMe.Name != MapObject.TORCH_NAME_STRING) {
                    _mapObjects.Remove(killMe);
                    _openPositions.Add(killMe.Position); //Not sure if this works
                }
            }
        }

        /// <summary>
        /// Collection of map objects on the grid
        /// </summary>
        public ReadOnlyCollection<MapObject> Objects
        {
            get
            {
                return _mapObjects.AsReadOnly();
            }
        }

        /// <summary>
        /// Collection of open position in the grid
        /// </summary>
        public ReadOnlyCollection<Vector2> OpenPositions
        {
            get
            {
                return _openPositions.AsReadOnly();
            }
        }
    }
}
