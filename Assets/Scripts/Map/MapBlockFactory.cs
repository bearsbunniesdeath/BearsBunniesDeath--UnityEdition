using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Map
{
    static class MapBlockFactory
    {


        static private List<JSONLayout> NonPathJSONLayouts;
        static private List<String> JSONNameNonPathProbabilityMap;
        static private List<JSONLayout> StraightPathJSONLayouts;
        static private List<String> JSONNameStraightPathProbabilityMap;

        static MapBlockFactory(){
            NonPathJSONLayouts = new List<JSONLayout>();
            StraightPathJSONLayouts = new List<JSONLayout>();
            BuildAllNonPathJSONLayouts();
            BuildAllStraightPathJSONLayouts();
            SetJSONProbabityMaps();
            //CREATE many density area and assign them to JSONLayouts with Probabilities

        }

        private static void SetJSONProbabityMaps()
        {
            JSONNameNonPathProbabilityMap = new List<string>();
            foreach (JSONLayout jl in NonPathJSONLayouts) {
                for (int i = 0; i < (int)(jl.frequency); i++) {
                    JSONNameNonPathProbabilityMap.Add(jl.LayoutName);
                }
            }
            JSONNameStraightPathProbabilityMap = new List<string>();
            foreach (JSONLayout jl in StraightPathJSONLayouts)
            {
                for (int i = 0; i < (int)(jl.frequency); i++)
                {
                    JSONNameStraightPathProbabilityMap.Add(jl.LayoutName);
                }
            }
        }

        //TODO: Add difficulty levels that we give to premade blocks
        static public MapBlock MakeMapBlock(int x, int y, bool leftOpen = false, bool topOpen = false, bool rightOpen = false, bool bottomOpen = false)
        {

            MapBlock newBlock;
            int numberOfOpenings = MapHelper.BoolToInt(leftOpen) + MapHelper.BoolToInt(topOpen) + MapHelper.BoolToInt(rightOpen) + MapHelper.BoolToInt(bottomOpen);

            //Temp: Starter and End block is fine with what's happening with CriticalPathMapBlock logic.
            if (numberOfOpenings == 1)
            {
                newBlock = new CriticalPathMapBlock(x, y, leftOpen, topOpen, rightOpen, bottomOpen);
            }
            else if (numberOfOpenings == 2 && topOpen == bottomOpen) //Straight Path
            {
                //This is a part of critical path (Straight)
                string dirPath = Application.dataPath + "/JSON/PathMapBlockLayout/";
                String path = dirPath + JSONNameStraightPathProbabilityMap[UnityEngine.Random.Range(0, JSONNameStraightPathProbabilityMap.Count)] + ".JSON";

                //Read the text from directly from the test.txt file
                StreamReader reader = new StreamReader(path);
                String jsonText = reader.ReadToEnd();
                JSONMapBlock nonPathBlock = JsonUtility.FromJson<JSONMapBlock>(jsonText);
                nonPathBlock.AfterSerialize(x, y);
                newBlock = nonPathBlock;

                if (!topOpen)
                {//At this point it must be a horizontal straight path (rotate JSON for vertical path)
                    MapHelper.RotateMapObjectsInBlock(ref newBlock, MapHelper.eClockWiseTurn.eQuarter);
                }

            }
            else if (numberOfOpenings == 2) {
                //"L" shaped path 
                newBlock = new CriticalPathMapBlock(x, y, leftOpen, topOpen, rightOpen, bottomOpen);
            }
            else
            {
                ////This is a NOT part of critical path
                string dirPath = Application.dataPath + "/JSON/NonPathMapBlockLayout/";
                String path = dirPath + JSONNameNonPathProbabilityMap[UnityEngine.Random.Range(0, JSONNameNonPathProbabilityMap.Count)] + ".JSON";

                //Read the text from directly from the test.txt file
                StreamReader reader = new StreamReader(path);
                String jsonText = reader.ReadToEnd();
                JSONMapBlock nonPathBlock = JsonUtility.FromJson<JSONMapBlock>(jsonText);
                nonPathBlock.AfterSerialize(x, y);
                newBlock = nonPathBlock;
            }

            return newBlock;
        }

        static public void RegenerateJSONFiles() {
            foreach (JSONLayout jlayout in NonPathJSONLayouts) {
                jlayout.SaveToJSON();
            }
            foreach (JSONLayout jlayout in StraightPathJSONLayouts)
            {
                jlayout.SaveToJSON("PathMapBlockLayout");
            }
        }


        /// <summary>
        /// Builds and stores lists of JSONLayout and sets up a Probability Distribution map by name
        /// If requested, the factory will rebuild and save the actaul JSON files
        /// </summary>
        static public void BuildAllNonPathJSONLayouts() {
            List<MapPosition> allCoords = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(MapBlock.SIZE_OF_BLOCK - 1, MapBlock.SIZE_OF_BLOCK - 1));
            List<MapPosition> islandCoords = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(3, 3), new MapPosition(6, 6));

            #region //Dense area of obstacles and grass (FullDense.JSON)
            JSONLayout JSONLayout1 = new JSONLayout();
            JSONLayout1.DensityAreas.Add(new DensityArea(0.35f, allCoords, DensityArea.eMapItems.terrainObstacles));
            JSONLayout1.DensityAreas.Add(new DensityArea(0.10f, allCoords, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout1.DensityAreas.Add(new DensityArea(0.50f, allCoords, DensityArea.eMapItems.thickGrass));
            JSONLayout1.frequency = JSONLayout.eFrequency.eHigh;
            JSONLayout1.LayoutName = "FullDense";

            NonPathJSONLayouts.Add(JSONLayout1);
            #endregion

            #region             //Island of obstacles with some brush around it
            JSONLayout JSONLayout2 = new JSONLayout();
            JSONLayout2.DensityAreas.Add(new DensityArea(1f, islandCoords, DensityArea.eMapItems.terrainObstacles));
            JSONLayout2.DensityAreas.Add(new DensityArea(0.1f, allCoords, DensityArea.eMapItems.terrainObstacles));
            JSONLayout2.DensityAreas.Add(new DensityArea(1f, islandCoords, DensityArea.eMapItems.thickGrass));
            JSONLayout2.frequency = JSONLayout.eFrequency.eMedium;
            JSONLayout2.LayoutName = "Island";
            NonPathJSONLayouts.Add(JSONLayout2);
            #endregion

            #region             //Bomb island in full forest
            JSONLayout JSONLayout3 = new JSONLayout();
            JSONLayout3.DensityAreas.Add(new DensityArea(0.25f, islandCoords, DensityArea.eMapItems.armedBomb));
            JSONLayout3. DensityAreas.Add(new DensityArea(0.25f, islandCoords, DensityArea.eMapItems.bomb));
            JSONLayout3.DensityAreas.Add(new DensityArea(0.30f, allCoords, DensityArea.eMapItems.terrainObstacles));
            JSONLayout3.DensityAreas.Add(new DensityArea(0.10f, allCoords, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout3.DensityAreas.Add(new DensityArea(0.50f, allCoords, DensityArea.eMapItems.thickGrass));
            JSONLayout3.frequency = JSONLayout.eFrequency.eLow;
            JSONLayout3.LayoutName = "itemIsland";
            NonPathJSONLayouts.Add(JSONLayout3);
            #endregion

            #region   //Empty grassy Field
            JSONLayout JSONLayout4 = new JSONLayout();
            JSONLayout4.DensityAreas.Add(new DensityArea(0.2f, allCoords, DensityArea.eMapItems.terrainObstacles));
            JSONLayout4.DensityAreas.Add(new DensityArea(0.1f, allCoords, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout4.DensityAreas.Add(new DensityArea(0.65f, allCoords, DensityArea.eMapItems.thickGrass));
            JSONLayout4.frequency = JSONLayout.eFrequency.eLow;
            JSONLayout4.LayoutName = "EmptyGrassyField";
            NonPathJSONLayouts.Add(JSONLayout4);
            #endregion

            #region   //Item Detour
            JSONLayout JSONLayout5 = new JSONLayout();
            JSONLayout5.DensityAreas.Add(new DensityArea(0.05f, islandCoords, DensityArea.eMapItems.bearTrap));
            JSONLayout5.DensityAreas.Add(new DensityArea(0.05f, islandCoords, DensityArea.eMapItems.bomb));
            JSONLayout5.DensityAreas.Add(new DensityArea(0.11f, islandCoords, DensityArea.eMapItems.armedBomb));
            JSONLayout5.DensityAreas.Add(new DensityArea(0.65f, allCoords, DensityArea.eMapItems.thickGrass));
            JSONLayout5.DensityAreas.Add(new DensityArea(0.25f, allCoords, DensityArea.eMapItems.terrainObstacles));
            JSONLayout5.DensityAreas.Add(new DensityArea(0.10f, allCoords, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout5.frequency = JSONLayout.eFrequency.eLow;
            JSONLayout5.LayoutName = "ItemDetour";
            NonPathJSONLayouts.Add(JSONLayout5);
            #endregion

            #region   //closed "C"
            JSONLayout JSONLayout6 = new JSONLayout();
            List<MapPosition> wall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(7, 0), new MapPosition(7, 7));
            List<MapPosition> insideWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(2, 3), new MapPosition(6, 4));
            List<MapPosition> topLeftWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 7), new MapPosition(3, 7));
            List<MapPosition> bottomLeftWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(3, 0));
            List<MapPosition> LeftWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 1), new MapPosition(0, 6));

            List<MapPosition> everythingElse = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(7, 7));
            JSONLayout6.DensityAreas.Add(new DensityArea(1f, wall, DensityArea.eMapItems.terrainObstacles));
            JSONLayout6.DensityAreas.Add(new DensityArea(1f, insideWall, DensityArea.eMapItems.terrainObstacles));
            JSONLayout6.DensityAreas.Add(new DensityArea(1f, topLeftWall, DensityArea.eMapItems.terrainObstacles));
            JSONLayout6.DensityAreas.Add(new DensityArea(1f, bottomLeftWall, DensityArea.eMapItems.terrainObstacles));
            JSONLayout6.DensityAreas.Add(new DensityArea(0.05f, everythingElse, DensityArea.eMapItems.terrainObstacles));
            JSONLayout6.frequency = JSONLayout.eFrequency.eMedium;
            JSONLayout6.LayoutName = "ClosedC";
            NonPathJSONLayouts.Add(JSONLayout6);
            #endregion

            #region   //Item throne
            JSONLayout JSONLayout7 = new JSONLayout();
            List<MapPosition> topCenter = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(2, 5), new MapPosition(5, 5));
            List<MapPosition> bottomWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 1), new MapPosition(7, 1));
            List<MapPosition> topLeftCorner = new List<MapPosition> { new MapPosition(0, 7), new MapPosition(0, 6), new MapPosition(1, 7) };
            List<MapPosition> topRightCorner = new List<MapPosition> { new MapPosition(6, 7), new MapPosition(7, 6), new MapPosition(7, 7) };
            List<MapPosition> itemSpace = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(3, 3), new MapPosition(4, 4));
            List<MapPosition> innerLeftWall = new List<MapPosition> { new MapPosition(2, 3), new MapPosition(2, 4) };
            List<MapPosition> innerRightWall = new List<MapPosition> { new MapPosition(5, 3), new MapPosition(5, 4) };

            JSONLayout7.DensityAreas.Add(new DensityArea(1f, topCenter, DensityArea.eMapItems.terrainObstacles));
            JSONLayout7.DensityAreas.Add(new DensityArea(1f, bottomWall, DensityArea.eMapItems.terrainObstacles));
            JSONLayout7.DensityAreas.Add(new DensityArea(1f, topLeftCorner, DensityArea.eMapItems.terrainObstacles));
            JSONLayout7.DensityAreas.Add(new DensityArea(1f, topRightCorner, DensityArea.eMapItems.terrainObstacles));
            JSONLayout7.DensityAreas.Add(new DensityArea(1f, innerLeftWall, DensityArea.eMapItems.terrainObstacles));
            JSONLayout7.DensityAreas.Add(new DensityArea(1f, innerRightWall, DensityArea.eMapItems.terrainObstacles));
            JSONLayout7.DensityAreas.Add(new DensityArea(0.1f, itemSpace, DensityArea.eMapItems.bearTrap));
            JSONLayout7.DensityAreas.Add(new DensityArea(0.1f, itemSpace, DensityArea.eMapItems.bomb));
            JSONLayout7.frequency = JSONLayout.eFrequency.eLow;
            JSONLayout7.LayoutName = "Itemthrone";
            NonPathJSONLayouts.Add(JSONLayout7);
            #endregion

            #region //Four squares with some brush around it
            JSONLayout JSONLayout8 = new JSONLayout();
            List<MapPosition> topLeft = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(1, 5), new MapPosition(2, 6));
            List<MapPosition> topRight = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(5, 5), new MapPosition(6, 6));
            List<MapPosition> botLeft = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(1, 1), new MapPosition(2, 2));
            List<MapPosition> botRight = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(5, 1), new MapPosition(6, 2));

            JSONLayout8.DensityAreas.Add(new DensityArea(1f, topLeft, DensityArea.eMapItems.terrainObstacles));
            JSONLayout8.DensityAreas.Add(new DensityArea(1f, topRight, DensityArea.eMapItems.terrainObstacles));
            JSONLayout8.DensityAreas.Add(new DensityArea(1f, botLeft, DensityArea.eMapItems.terrainObstacles));
            JSONLayout8.DensityAreas.Add(new DensityArea(1f, botRight, DensityArea.eMapItems.terrainObstacles));
            JSONLayout8.DensityAreas.Add(new DensityArea(0.1f, allCoords, DensityArea.eMapItems.terrainObstacles));
            JSONLayout8.frequency = JSONLayout.eFrequency.eMedium;
            JSONLayout8.LayoutName = "FourSquares";
            NonPathJSONLayouts.Add(JSONLayout8);
            #endregion

            #region //Shoe Throne
            JSONLayout JSONLayout9 = new JSONLayout();
            List<MapPosition> leftPillar = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(1, 1), new MapPosition(1, 6));
            List<MapPosition> rightPillar = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(6, 1), new MapPosition(6, 6));
            List<MapPosition> topChunk = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(3, 4), new MapPosition(4, 6));
            List<MapPosition> botChunk = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(3, 1), new MapPosition(4, 1));

            JSONLayout9.DensityAreas.Add(new DensityArea(1f, leftPillar, DensityArea.eMapItems.terrainObstacles));
            JSONLayout9.DensityAreas.Add(new DensityArea(1f, rightPillar, DensityArea.eMapItems.terrainObstacles));
            JSONLayout9.DensityAreas.Add(new DensityArea(1f, topChunk, DensityArea.eMapItems.terrainObstacles));
            JSONLayout9.DensityAreas.Add(new DensityArea(1f, botChunk, DensityArea.eMapItems.terrainObstacles));
            JSONLayout9.DensityAreas.Add(new DensityArea(1f, new List<MapPosition> { new MapPosition(4, 2) }, DensityArea.eMapItems.shoes));
            JSONLayout9.frequency = JSONLayout.eFrequency.eLow;
            JSONLayout9.LayoutName = "ShoeThrone";
            NonPathJSONLayouts.Add(JSONLayout9);
            #endregion

            #region //BackPack Throne (encased)
            JSONLayout JSONLayout10 = new JSONLayout();
            List<MapPosition> leftPillar1 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(0, 7));
            List<MapPosition> rightPillar1 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(7, 0), new MapPosition(7, 7));
            List<MapPosition> topChunk1 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(3, 0), new MapPosition(4, 0));
            List<MapPosition> botChunk1 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(3, 7), new MapPosition(4, 7));

            JSONLayout10.DensityAreas.Add(new DensityArea(0.8f, leftPillar1, DensityArea.eMapItems.terrainObstacles));
            JSONLayout10.DensityAreas.Add(new DensityArea(1f, leftPillar1, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout10.DensityAreas.Add(new DensityArea(0.8f, rightPillar1, DensityArea.eMapItems.terrainObstacles));
            JSONLayout10.DensityAreas.Add(new DensityArea(1f, rightPillar1, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout10.DensityAreas.Add(new DensityArea(1f, topChunk1, DensityArea.eMapItems.terrainObstacles));
            JSONLayout10.DensityAreas.Add(new DensityArea(1f, botChunk1, DensityArea.eMapItems.terrainObstacles));
            JSONLayout10.DensityAreas.Add(new DensityArea(1f, new List<MapPosition> { new MapPosition(1, 0), new MapPosition(1, 2), new MapPosition(1, 5), new MapPosition(1, 7) }, DensityArea.eMapItems.terrainObstacles));
            JSONLayout10.DensityAreas.Add(new DensityArea(1f, new List<MapPosition> { new MapPosition(6, 0), new MapPosition(6, 2), new MapPosition(6, 5), new MapPosition(6, 7) }, DensityArea.eMapItems.terrainObstacles));
            JSONLayout10.DensityAreas.Add(new DensityArea(1f, new List<MapPosition> { new MapPosition(5, 5) }, DensityArea.eMapItems.backPack));
            JSONLayout10.frequency = JSONLayout.eFrequency.eLow;
            JSONLayout10.LayoutName = "EmptyThrone";
            NonPathJSONLayouts.Add(JSONLayout10);
            #endregion

            #region //One Way intersection
            JSONLayout JSONLayout11 = new JSONLayout();
            List<MapPosition> leftPillar2 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(2, 0), new MapPosition(2, 7));
            List<MapPosition> rightPillar2 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(5, 0), new MapPosition(5, 7));
            List<MapPosition> leftChunk2 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 3), new MapPosition(1, 4));
            List<MapPosition> rightChunk2 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(6, 3), new MapPosition(7, 4));

            JSONLayout11.DensityAreas.Add(new DensityArea(0.9f, leftPillar2, DensityArea.eMapItems.terrainObstacles));
            JSONLayout11.DensityAreas.Add(new DensityArea(1f, leftPillar2, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout11.DensityAreas.Add(new DensityArea(0.9f, rightPillar2, DensityArea.eMapItems.terrainObstacles));
            JSONLayout11.DensityAreas.Add(new DensityArea(1f, rightPillar2, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout11.DensityAreas.Add(new DensityArea(0.9f, leftChunk2, DensityArea.eMapItems.terrainObstacles));
            JSONLayout11.DensityAreas.Add(new DensityArea(1f, leftChunk2, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout11.DensityAreas.Add(new DensityArea(1f, rightChunk2, DensityArea.eMapItems.terrainObstacles)); JSONLayout10.frequency = JSONLayout.eFrequency.eLow;
            JSONLayout11.frequency = JSONLayout.eFrequency.eLow;
            JSONLayout11.LayoutName = "OneWayIntersection";
            NonPathJSONLayouts.Add(JSONLayout11);
            #endregion

            #region //House (has revival item inside)
            JSONLayout JSONLayout12 = new JSONLayout();
            List<MapPosition> housePlacement = new List<MapPosition> {new MapPosition(0,0)};
            JSONLayout12.DensityAreas.Add(new DensityArea(1f, housePlacement, DensityArea.eMapItems.house));
            JSONLayout12.frequency = JSONLayout.eFrequency.eLow;
            JSONLayout12.LayoutName = "HouseOnly";
            NonPathJSONLayouts.Add(JSONLayout12);
            #endregion

        }

        static public void BuildAllStraightPathJSONLayouts() {
            #region //Normal Vertical Path Surrounded by Dense brush
            JSONLayout JSONLayout1 = new JSONLayout();

            List<MapPosition> pathCoords = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(2, 0), new MapPosition(5,7));
            List<MapPosition> outsideCoordsLeft = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(1, 7));
            List<MapPosition> outsideCoordsRight = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(6, 0), new MapPosition(7, 7));

            JSONLayout1.DensityAreas.Add(new DensityArea(0.80f, outsideCoordsLeft, DensityArea.eMapItems.terrainObstacles));
            JSONLayout1.DensityAreas.Add(new DensityArea(1f, outsideCoordsLeft, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout1.DensityAreas.Add(new DensityArea(0.99f, outsideCoordsLeft, DensityArea.eMapItems.thickGrass));
            JSONLayout1.DensityAreas.Add(new DensityArea(0.80f, outsideCoordsRight, DensityArea.eMapItems.terrainObstacles));
            JSONLayout1.DensityAreas.Add(new DensityArea(1f, outsideCoordsRight, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout1.DensityAreas.Add(new DensityArea(0.99f, outsideCoordsRight, DensityArea.eMapItems.thickGrass));
            JSONLayout1.DensityAreas.Add(new DensityArea(0.05f, pathCoords, DensityArea.eMapItems.terrainObstacles));
            JSONLayout1.DensityAreas.Add(new DensityArea(0.05f, pathCoords, DensityArea.eMapItems.thickGrass));
            JSONLayout1.DensityAreas.Add(new DensityArea(1f, pathCoords, DensityArea.eMapItems.pathTile));
            JSONLayout1.frequency = JSONLayout.eFrequency.eHigh;
            JSONLayout1.LayoutName = "FourWidthSimpleStraightPath";

            StraightPathJSONLayouts.Add(JSONLayout1);
            #endregion

            #region //Zigzag path
            List<MapPosition> leftWall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(0, 7));
            List<MapPosition> rightwall = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(7, 0), new MapPosition(7, 7));
            List<MapPosition> zig1 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(5, 0), new MapPosition(7, 0));  //Shorter so we don't block path
            List<MapPosition> zag1 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(1, 2), new MapPosition(5, 2));
            List<MapPosition> zig2 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(2, 4), new MapPosition(6, 4));
            List<MapPosition> zag2 = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(1, 6), new MapPosition(5, 6));
            pathCoords = MapHelper.GetRectangleOfPositionsBetweenPoints(new MapPosition(0, 0), new MapPosition(7, 7)); //Everything else, why not?

            JSONLayout JSONLayout2 = new JSONLayout();
            JSONLayout2.DensityAreas.Add(new DensityArea(0.80f, leftWall, DensityArea.eMapItems.terrainObstacles));
            JSONLayout2.DensityAreas.Add(new DensityArea(1f, leftWall, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout2.DensityAreas.Add(new DensityArea(0.80f, rightwall, DensityArea.eMapItems.terrainObstacles));
            JSONLayout2.DensityAreas.Add(new DensityArea(1f, rightwall, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout2.DensityAreas.Add(new DensityArea(0.80f, zig1, DensityArea.eMapItems.terrainObstacles));
            JSONLayout2.DensityAreas.Add(new DensityArea(1f, zig1, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout2.DensityAreas.Add(new DensityArea(0.80f, zag1, DensityArea.eMapItems.terrainObstacles));
            JSONLayout2.DensityAreas.Add(new DensityArea(1f, zag1, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout2.DensityAreas.Add(new DensityArea(0.80f, zig2, DensityArea.eMapItems.terrainObstacles));
            JSONLayout2.DensityAreas.Add(new DensityArea(1f, zig2, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout2.DensityAreas.Add(new DensityArea(0.80f, zag2, DensityArea.eMapItems.terrainObstacles));
            JSONLayout2.DensityAreas.Add(new DensityArea(1f, zag2, DensityArea.eMapItems.jumpableObstcles));
            JSONLayout2.DensityAreas.Add(new DensityArea(1f, pathCoords, DensityArea.eMapItems.pathTile));
            JSONLayout2.frequency = JSONLayout.eFrequency.eHigh;
            JSONLayout2.LayoutName = "ZigZagStraightPath";

            StraightPathJSONLayouts.Add(JSONLayout2);
            #endregion
        }


    }

    class JSONLayout {
        public List<DensityArea> DensityAreas = new List<DensityArea>();
        public enum eFrequency {
            eLow = 1,
            eMedium = 2,
            eHigh = 3
        }

        public eFrequency frequency; //Enum for Low, Medium, High
        public string LayoutName;

        public void SaveToJSON(string dirName = "NonPathMapBlockLayout") {
            JSONMapBlock realMapBlock = new JSONMapBlock(0, 0);
            realMapBlock.SetDensityAreas(this.DensityAreas);

            string jsonText = realMapBlock.GetJSONString();

            string filePath = "/JSON/" + dirName +"/";
            string fileName =  LayoutName + ".json";

            filePath = Application.dataPath + filePath + fileName;

            using (StreamWriter newTask = new StreamWriter(filePath, !System.IO.File.Exists(filePath)))
            {
                newTask.Write(jsonText);
            }
        }
    }

}
