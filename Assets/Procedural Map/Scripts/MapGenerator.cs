using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ProceduralMap
{
    public class MapGenerator : MonoBehaviour
    {
        public int size;
        [Space(5f)]
        public FieldGenerator heightGenerator;
        public FieldGenerator heatGenerator;
        public FieldGenerator humidityGenerator;

        [Space(5f)]
        public ComputeShader heightCompute;
        public ComputeShader slopeCompute;
        public ComputeShader heatCompute;
        public ComputeShader humidityCompute;

        [Space(5f)]
        public AnimationCurve heightCurve;

        [Space(5f)]
        public float deepWaterLevel;
        public float waterLevel;
        public float shallowWaterLevel;
        public float sandLevel;
        public float rockLevel;
        public float hardRockLevel;
        public float highSnowLevel;

        [Space(5f)]
        public float iceLevel;
        public float tundraLevel;
        public float scrubLevel;
        public float forestLevel;

        [Space(5f)]
        public float dessertLevel;
        public float dryLevel;
        public float wetLevel;

        public Color deepWater;
        public Color water;
        public Color shallowWater;
        public Color snowDessertColor;
        public Color dryTundra;
        public Color tundra;
        public Color rainTundra;
        public Color dessert;
        public Color dryScrub;
        public Color scrub;
        public Color forestConifer;
        public Color rainForest;
        public Color steppe;
        public Color forestLeaves;
        public Color dessertHot;
        public Color woodland;
        public Color rainForestHpt;


        public Color GetColor(float height, float heat, float humidity, float slope)
        {

            if (heat < iceLevel)
                return snowDessertColor;

            if (height < deepWaterLevel)
                return deepWater;

            if (height < waterLevel)
                return water;

            if (height < shallowWaterLevel)
                return shallowWater;

            if (height < sandLevel)
                return Color.yellow;
            if (height > highSnowLevel)
                return new Color(0.95f, 0.95f, 0.95f, 1);
            if (height > hardRockLevel)
                return new Color(0.4f, 0.4f, 0.4f, 1);
            if (height > rockLevel)
                return new Color(0.5f, 0.5f, 0.5f, 1);

            if(heat < tundraLevel)
            {
                if (humidity < dessertLevel)
                    return dryTundra;
                if (humidity < wetLevel)
                    return tundra;

                return rainTundra;
            }

            if(humidity < dessertLevel)
            {
                if (heat < forestLevel)
                    return dessert;
                return dessertHot;
            }

            if(heat < scrubLevel)
            {
                if (humidity < dryLevel)
                    return dryScrub;
                if (humidity < wetLevel)
                    return scrub;
                return forestConifer;
            }

            if(heat < forestLevel)
            {
                if (humidity < dryLevel)
                    return steppe;
                if (humidity < wetLevel)
                    return forestLeaves;
                return rainForest;
            }

            if (humidity < dryLevel)
                return woodland;

            return rainForest;

        }

        public void GenerateMap()
        {

            float[] height = new float[size * size];
            float[] slope = new float[size * size];
            float[] heat = new float[size * size];
            float[] humidity = new float[size * size];

            int threadGroup = Mathf.CeilToInt(size / 8f);

            #region height
            ComputeBuffer heightBuffer = heightGenerator.GenerateComputeBuffer(size);

            heightCompute.SetBuffer(0, "height", heightBuffer);
            heightCompute.SetInt("size", size);

            heightCompute.Dispatch(0, threadGroup, threadGroup, 1);

            heightBuffer.GetData(height);
            #endregion            
            #region slope
            ComputeBuffer slopeBuffer = new ComputeBuffer(size * size, 4);

            slopeCompute.SetBuffer(0, "slope", slopeBuffer);
            slopeCompute.SetBuffer(0, "height", heightBuffer);
            slopeCompute.SetInt("size", size);

            slopeCompute.Dispatch(0, threadGroup, threadGroup, 1);

            slopeBuffer.GetData(slope);
            #endregion            
            #region heat
            ComputeBuffer heatBuffer = heatGenerator.GenerateComputeBuffer(size);
           
            heatCompute.SetBuffer(0, "heat", heatBuffer);
            heatCompute.SetBuffer(0, "height", heightBuffer);
            heatCompute.SetInt("size", size);

            heatCompute.Dispatch(0, threadGroup, threadGroup, 1);
            
            heatBuffer.GetData(heat);
            #endregion
            #region humidity
            ComputeBuffer humidityBuffer = humidityGenerator.GenerateComputeBuffer(size);

            humidityCompute.SetBuffer(0, "humidity", humidityBuffer);
            humidityCompute.SetBuffer(0, "height", heightBuffer);
            humidityCompute.SetInt("size", size);

            humidityCompute.Dispatch(0, threadGroup, threadGroup, 1);

            humidityBuffer.GetData(humidity);
            #endregion

            Texture2D map = new Texture2D(size, size);

            Color[] cs = new Color[size * size];
            for (int i = 0; i < cs.Length; i++) {
                cs[i] = GetColor(height[i], heat[i], humidity[i], slope[i]);
            }

            map.SetPixels(cs);
            map.filterMode = FilterMode.Point;
            map.wrapMode = TextureWrapMode.Clamp;

            map.Apply();

            File.WriteAllBytes(Application.dataPath + "/Procedural Map/Maps/map.png", map.EncodeToPNG());

            heightBuffer.Release();
            slopeBuffer.Release();
            heatBuffer.Release();
        }
    }
}
