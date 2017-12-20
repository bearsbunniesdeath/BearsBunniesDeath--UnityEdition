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

        public void SetDensityAreas(List<DensityArea> areas) {
            this.DensityAreas = areas;
        }

        public string GetJSONString() {
            return JsonUtility.ToJson(this, true);
        }

        public override void Build()
        {
            PopulateDensityAreas();
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
