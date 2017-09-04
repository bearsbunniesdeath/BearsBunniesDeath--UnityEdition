using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Map
{
    class MapBlockFactory
    {

        //TODO: Add difficulty levels that we give to premade blocks
        public MapBlock MakeMapBlock(int x, int y, bool leftOpen = false, bool topOpen = false, bool rightOpen = false, bool bottomOpen = false)
        {

            MapBlock newBlock;
            if (leftOpen || topOpen || rightOpen || bottomOpen)
            {
                //This is a part of critical path
                newBlock = new CriticalPathMapBlock(x, y, leftOpen, topOpen, rightOpen, bottomOpen);
            }
            else {
                ////This is a NOT part of critical path
                string dirPath = Application.dataPath + "/JSON";
                var files = Directory.GetFiles(dirPath, "*.json");
                String path = files[UnityEngine.Random.Range(0, files.Length)];

                //Read the text from directly from the test.txt file
                StreamReader reader = new StreamReader(path);
                String jsonText = reader.ReadToEnd();
                NonPathMapBlock nonPathBlock = JsonUtility.FromJson<NonPathMapBlock>(jsonText);
                nonPathBlock.AfterSerialize(x,y);
                newBlock = nonPathBlock;
            }

            return newBlock;
        }

        public void RegenerateJSONFiles() {
            NonPathMapBlock nonPathBlockRegener = new NonPathMapBlock(0, 0);
            for (int i = 0; i < 8; i++) {
                nonPathBlockRegener.GenerateLayoutToJSON(i);
            }
        }


    }
}
