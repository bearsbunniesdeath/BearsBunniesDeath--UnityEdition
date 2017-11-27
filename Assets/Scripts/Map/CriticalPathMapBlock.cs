using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Map
{
    class CriticalPathMapBlock : MapBlock
    {

        private enum epathSegmentType
        {
            topToBottom,
            leftToRight,
            NWCorner,
            NECorner,
            SWCorner,
            SECorner
        }

        private bool leftOpen;
        private bool topOpen;
        private bool rightOpen;
        private bool bottomOpen;

        private List<MapPosition> sparsePositions;
        private List<MapPosition> densePositions; //Need to save this for thick grass placement

        private float DENSE_PROBABILITY = 0.30f;
        private float THICK_GRASS_PROBABILITY = 0.50f;
        private float SPARSE_PROBABILITY = 0.00f;
        private float UNDER_HOUSE_PROBABILITY = 0.0f;
        private float AROUND_HOUSE_PROBABILITY = 0.1f;
        private int PATH_WIDTH = 3;

        public CriticalPathMapBlock(int x, int y, bool leftOpen = false, bool topOpen = false, bool rightOpen = false, bool bottomOpen = false)
        {
            blockPosition = new Vector2(x, y);

            blockObjects = new MapObjectGrid(SIZE_OF_BLOCK, SIZE_OF_BLOCK);

            this.leftOpen = leftOpen;
            this.topOpen = topOpen;
            this.rightOpen = rightOpen;
            this.bottomOpen = bottomOpen;
        }

        public override void Build()
        {
            this.DensityAreas = new List<DensityArea>();
            CreateSparseAreaForCritPath();
            CreateDenseAreaForCritPath();

            if (UnityEngine.Random.Range(0, 4) == 0)
            {
                //Randonly Replace simple block layouts with pre made different ones
                SetSpecialLayoutForBlock();
                //TODO: Apply a special path
            }

            foreach (MapPosition pathPos in GetPathSegmentFromOpenSides(2)) //Actaul path is thicker than drawn path (with brown tiles)
            {
                blockObjects.Add(new MapObject(MapBuilder.instance.pathTile, MapPositionFromCellPosition(pathPos.Vector2)));
            }


            if (!this.blockObjects.Objects.Any(o => o.Name == MapObject.TORCH_NAME_STRING))
            {
                //No torch at this point, this is a generic layout, just put in middle

                //Since the blocks are smaller now, only do a < 1 torch:block ratio
                if (UnityEngine.Random.Range(0, 5) == 1)
                {
                    this.blockObjects.Add(new MapObject(MapBuilder.instance.TorchObject, MapPositionFromCellPosition(new Vector2(3, 3))));
                }
            }

            PopulateDensityAreas();
        }

        private void CreateSparseAreaForCritPath()
        {

            int pathBorderWidth = (SIZE_OF_BLOCK - PATH_WIDTH) / 2;
            MapPosition startPointOfRect = null;
            MapPosition endPointOfRect = null;

            this.sparsePositions = GetPathSegmentFromOpenSides(PATH_WIDTH);
            DensityAreas.Add(new DensityArea(SPARSE_PROBABILITY, sparsePositions, DensityArea.eMapItems.terrainObstacles));

        }

        private List<MapPosition> GetPathSegmentFromOpenSides(int pathWidth)
        {
            List<MapPosition> returnList = new List<MapPosition>();
            int pathWidthForSegment = pathWidth;
            int pathBorderWidth = (SIZE_OF_BLOCK - pathWidth) / 2;
            MapPosition startPointOfRect = null;
            MapPosition endPointOfRect = null;
            if (this.topOpen)
            {
                startPointOfRect = new MapPosition(pathBorderWidth, pathBorderWidth);
                endPointOfRect = new MapPosition(pathBorderWidth + pathWidthForSegment - 1, SIZE_OF_BLOCK - 1);
                returnList.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(startPointOfRect, endPointOfRect));
            }

            if (this.bottomOpen)
            {
                startPointOfRect = new MapPosition(pathBorderWidth, 0);
                endPointOfRect = new MapPosition(pathBorderWidth + pathWidthForSegment - 1, pathBorderWidth + pathWidthForSegment - 1);
                returnList.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(startPointOfRect, endPointOfRect));
            }

            if (this.leftOpen)
            {
                startPointOfRect = new MapPosition(0, pathBorderWidth);
                endPointOfRect = new MapPosition(pathBorderWidth + pathWidthForSegment - 1, pathBorderWidth + pathWidthForSegment - 1);
                returnList.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(startPointOfRect, endPointOfRect));
            }

            if (this.rightOpen)
            {
                startPointOfRect = new MapPosition(pathBorderWidth, pathBorderWidth);
                endPointOfRect = new MapPosition(SIZE_OF_BLOCK - 1, pathBorderWidth + pathWidthForSegment - 1);
                returnList.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(startPointOfRect, endPointOfRect));
            }

            returnList = MapHelper.GetDistinctMapPositions(returnList);
            //returnList = returnList.Distinct().ToList();

            return returnList;
        }

        private void CreateDenseAreaForCritPath()
        {
            densePositions = new List<MapPosition>();
            foreach (Vector2 coord in blockObjects.OpenPositions)
            {
                if (!sparsePositions.Exists(o => o.x == coord.x & o.y == coord.y))
                {
                    densePositions.Add(new MapPosition(coord.x, coord.y));
                }
            }
            DensityAreas.Add(new DensityArea(DENSE_PROBABILITY, densePositions, DensityArea.eMapItems.terrainObstacles));
            DensityAreas.Add(new DensityArea(THICK_GRASS_PROBABILITY, densePositions, DensityArea.eMapItems.thickGrass));
        }

        private void SetSpecialLayoutForBlock()
        {

            if (this.bottomOpen && this.topOpen)
            {
                this.DensityAreas = GetRandomSpecialLayout(epathSegmentType.topToBottom);
                this.blockObjects.Add(new MapObject(MapBuilder.instance.TorchObject, MapPositionFromCellPosition(new Vector2(3, 1))));

            }
            else if (this.rightOpen && this.leftOpen)
            {
                this.DensityAreas = GetRandomSpecialLayout(epathSegmentType.leftToRight);
                this.blockObjects.Add(new MapObject(MapBuilder.instance.TorchObject, MapPositionFromCellPosition(new Vector2(1, 3))));
            }
        }

        private List<DensityArea> GetRandomSpecialLayout(epathSegmentType type)
        {
            if (type == epathSegmentType.topToBottom)
            {
                List<List<DensityArea>> AllTopToBottomLayouts = GetAllPathLayoutsTopToBottom();
                return AllTopToBottomLayouts[UnityEngine.Random.Range(0, AllTopToBottomLayouts.Count)];
            }
            else if (type == epathSegmentType.leftToRight)
            {
                //Solid Walls
                List<List<DensityArea>> AllLeftToRightLayouts = GetAllPathLayoutsLeftToRight();
                return AllLeftToRightLayouts[0];
            }
            else
            {
                return null;
            }
        }

        //Should I sub class this List<DensityArea>?
        private static List<List<DensityArea>> GetAllPathLayoutsTopToBottom()
        {
            List<List<DensityArea>> AllLayouts = new List<List<DensityArea>>();

            //ONE ISLAND -----------------
            //Solid Walls
            //Unfortunately, this kind of needs to be hardcoded values
            List<MapPosition> wallCoords = new List<MapPosition>();
            wallCoords.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(0, 7))); //Left Wall
            wallCoords.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(7, 0), new MapPosition(7, 7))); //Right Wall
            wallCoords.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(3, 2), new MapPosition(4, 5))); //Center Island
            List<MapPosition> fuzzyEdges = new List<MapPosition>();
            fuzzyEdges.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(2, 1), new MapPosition(5, 6))); //Fuzz around island
            DensityArea solidWallsDensityArea = new DensityArea(1.0f, wallCoords, DensityArea.eMapItems.terrainObstacles);
            DensityArea fuzzyWallsDensityArea = new DensityArea(0.5f, fuzzyEdges, DensityArea.eMapItems.terrainObstacles);
            AllLayouts.Add(new List<DensityArea> { solidWallsDensityArea, fuzzyWallsDensityArea });
            //===========================

            return AllLayouts;
        }

        //Should I sub class this List<DensityArea>?
        private static List<List<DensityArea>> GetAllPathLayoutsLeftToRight()
        {
            List<List<DensityArea>> AllLayouts = new List<List<DensityArea>>();

            //ONE ISLAND -----------------
            //Solid Walls
            List<MapPosition> wallCoords = new List<MapPosition>();
            wallCoords.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(7, 0))); //Left Wall
            wallCoords.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 7), new MapPosition(7, 7))); //Right Wall
            wallCoords.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(2, 3), new MapPosition(5, 4))); //Center Island
            List<MapPosition> fuzzyEdges = new List<MapPosition>();
            fuzzyEdges.AddRange(MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(1, 2), new MapPosition(6, 5))); //Fuzz around island
            DensityArea solidWallsDensityArea = new DensityArea(1.0f, wallCoords, DensityArea.eMapItems.terrainObstacles);
            DensityArea fuzzyWallsDensityArea = new DensityArea(0.5f, fuzzyEdges, DensityArea.eMapItems.terrainObstacles);
            AllLayouts.Add(new List<DensityArea> { solidWallsDensityArea, fuzzyWallsDensityArea });
            //===========================
            return AllLayouts;
        }

    }
}
