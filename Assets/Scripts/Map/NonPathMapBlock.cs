using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Map
{
    /// <summary>
    /// Map blocks that do not have the critical path in them, can be used for special "exploration" blocks or just left dense with forest
    /// </summary>
    [System.Serializable]
    class NonPathMapBlock : MapBlock
    {

        private float DENSE_PROBABILITY = 0.30f;
        private float THICK_GRASS_PROBABILITY = 0.50f;

        public NonPathMapBlock(int x, int y)
        {
            blockPosition = new Vector2(x, y);
            blockObjects = new MapObjectGrid(SIZE_OF_BLOCK, SIZE_OF_BLOCK);
        }

        public override void Build()
        {
            PopulateDensityAreas();
        }

        //SUPER HACK, should serialize pre made layout and call them from files at some point
        public void GenerateLayoutToJSON(int index) {
            DensityAreas = new List<DensityArea>();

            List<MapPosition> allCoords = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(this.SIZE_OF_BLOCK - 1, this.SIZE_OF_BLOCK - 1));

            if (index == 0) {
                DensityAreas.Add(new DensityArea(DENSE_PROBABILITY, allCoords, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(THICK_GRASS_PROBABILITY, allCoords, DensityArea.eMapItems.thickGrass));
            }
            else if (index == 1) {
                List<MapPosition> islandCoords = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(3, 3), new MapPosition(6, 6));
                DensityAreas.Add(new DensityArea(1f, islandCoords, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, islandCoords, DensityArea.eMapItems.thickGrass));
            }
            else if (index == 2)
            {
                //Bomb island in full forrest
                List<MapPosition> islandCoords = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(4, 4), new MapPosition(6, 6));
                DensityAreas.Add(new DensityArea(0.5f, islandCoords, DensityArea.eMapItems.armedBomb));
                DensityAreas.Add(new DensityArea(0.5f, islandCoords, DensityArea.eMapItems.bomb));
                DensityAreas.Add(new DensityArea(DENSE_PROBABILITY, allCoords, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(THICK_GRASS_PROBABILITY, allCoords, DensityArea.eMapItems.thickGrass));
            }
            else if (index == 3)
            {
                //Empty grassy Field
                DensityAreas.Add(new DensityArea(0.2f, allCoords, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(0.65f, allCoords, DensityArea.eMapItems.thickGrass));
            }
            else if (index == 4)
            {
                //Item Detour
                List<MapPosition> islandCoords = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(4, 4), new MapPosition(6, 6));
                DensityAreas.Add(new DensityArea(0.11f, islandCoords, DensityArea.eMapItems.bearTrap));
                DensityAreas.Add(new DensityArea(0.11f, islandCoords, DensityArea.eMapItems.bomb));
                DensityAreas.Add(new DensityArea(0.11f, islandCoords, DensityArea.eMapItems.armedBomb));
                DensityAreas.Add(new DensityArea(0.65f, allCoords, DensityArea.eMapItems.thickGrass));
            }
            else if (index == 5)
            {
                //closed "C"
                List<MapPosition> wall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(7, 0), new MapPosition(7, 7));
                List<MapPosition> insideWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(2, 3), new MapPosition(6, 4));
                List<MapPosition> topLeftWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 7), new MapPosition(3, 7));
                List<MapPosition> bottomLeftWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(3, 0));
                List<MapPosition> LeftWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 1), new MapPosition(0, 6));

                List<MapPosition> everythingElse = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(7, 7));
                DensityAreas.Add(new DensityArea(1f, wall, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, insideWall, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, topLeftWall, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, bottomLeftWall, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(0.05f, everythingElse, DensityArea.eMapItems.terrainObstacles));
            }
            else if (index == 6)
            {
                //Item throne
                List<MapPosition> topCenter = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(2, 5), new MapPosition(5, 5));
                List<MapPosition> bottomWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 1), new MapPosition(7, 1));
                List<MapPosition> topLeftCorner = new List<MapPosition> {new MapPosition(0,7), new MapPosition(0,6),new MapPosition(1, 7)};
                List<MapPosition> topRightCorner = new List<MapPosition> {new MapPosition(6,7), new MapPosition(7,6),new MapPosition(7, 7)};
                List<MapPosition> itemSpace = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(3, 3), new MapPosition(4, 4));
                List<MapPosition> innerLeftWall = new List<MapPosition> { new MapPosition(2, 3), new MapPosition(2, 4)};
                List<MapPosition> innerRightWall = new List<MapPosition> { new MapPosition(5, 3), new MapPosition(5, 4) };

                DensityAreas.Add(new DensityArea(1f, topCenter, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, bottomWall, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, topLeftCorner, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, topRightCorner, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, innerLeftWall, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, innerRightWall, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(0.2f, itemSpace, DensityArea.eMapItems.bearTrap));
                DensityAreas.Add(new DensityArea(0.2f, itemSpace, DensityArea.eMapItems.bomb));
            }
            else if (index == 7)
            {
                //Four squares
                List<MapPosition> topLeft = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(1, 5), new MapPosition(2,6));
                List<MapPosition> topRight = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(5, 5), new MapPosition(6, 6));
                List<MapPosition> botLeft = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(1, 1), new MapPosition(2, 2));
                List<MapPosition> botRight = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(5, 1), new MapPosition(6, 2));

                DensityAreas.Add(new DensityArea(1f, topLeft, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, topRight, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, botLeft, DensityArea.eMapItems.terrainObstacles));
                DensityAreas.Add(new DensityArea(1f, botRight, DensityArea.eMapItems.terrainObstacles));
            }

            string json = JsonUtility.ToJson(this,true);
            string filePath = "/JSON";
            string fileName = "/NonPathMapBlockLayout_" + index.ToString() + ".json";

            filePath = Application.dataPath + filePath + fileName;

            using (StreamWriter newTask = new StreamWriter(filePath, !System.IO.File.Exists(filePath)))
            {
                newTask.Write(json);
            }
        }

        /// <summary>
        /// JSON probably has something to do this automatically
        /// </summary>
        public void AfterSerialize(int x, int y) {
            blockPosition = new Vector2(x, y);
            blockObjects = new MapObjectGrid(SIZE_OF_BLOCK, SIZE_OF_BLOCK);
        }

    }
}
