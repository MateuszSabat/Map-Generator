using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        public ComputeShader normalCompute;
        public ComputeShader heatCompute;
        public ComputeShader humidityCompute;
        public ComputeShader biomesCompute;

        [Space(5f)]
        public float[] heightLevel;
        public float[] heatLevel;
        public float[] humidityLevel;

        public Texture2D biomesSample;
        public Texture2D waterSample;


        public void GenerateMap()
        {

            float[] height = new float[size * size];
            float[] normal = new float[size * size];
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
            #region normal
            ComputeBuffer normalBuffer = new ComputeBuffer(size * size, 4);

            normalCompute.SetBuffer(0, "normal", normalBuffer);
            normalCompute.SetBuffer(0, "h", heightBuffer);
            normalCompute.SetInt("size", size);

            normalCompute.Dispatch(0, threadGroup, threadGroup, 1);

            normalBuffer.GetData(normal);
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

            ComputeBuffer colorBuffer = new ComputeBuffer(size * size, 16);

            ComputeBuffer heightLevelBuffer = new ComputeBuffer(heightLevel.Length, 4);
            ComputeBuffer heatLevelBuffer = new ComputeBuffer(heatLevel.Length, 4);
            ComputeBuffer humidityLevelBuffer = new ComputeBuffer(humidityLevel.Length, 4);

            heightLevelBuffer.SetData(heightLevel);
            heatLevelBuffer.SetData(heatLevel);
            humidityLevelBuffer.SetData(humidityLevel);

            biomesCompute.SetBuffer(0, "color", colorBuffer);
            biomesCompute.SetBuffer(0, "height", heightBuffer);
            biomesCompute.SetBuffer(0, "normal", normalBuffer);
            biomesCompute.SetBuffer(0, "heat", heatBuffer);
            biomesCompute.SetBuffer(0, "humidity", humidityBuffer);
            biomesCompute.SetInt("size", size);
            biomesCompute.SetBuffer(0, "heightLevel", heightLevelBuffer);
            biomesCompute.SetBuffer(0, "heatLevel", heatLevelBuffer);
            biomesCompute.SetBuffer(0, "humidityLevel", humidityLevelBuffer);
            biomesCompute.SetInt("heightLevelCount", heightLevel.Length);
            biomesCompute.SetInt("heatLevelCount", heatLevel.Length);
            biomesCompute.SetInt("humidityLevelCount", humidityLevel.Length);
            biomesCompute.SetTexture(0, "landSample", biomesSample);
            biomesCompute.SetTexture(0, "waterSample", waterSample);

            biomesCompute.Dispatch(0, threadGroup, threadGroup, 1);

            Color[] colors = new Color[size * size];

            colorBuffer.GetData(colors);

            map.SetPixels(colors);
            map.filterMode = FilterMode.Point;
            map.wrapMode = TextureWrapMode.Clamp;

            map.Apply();

            File.WriteAllBytes(Application.dataPath + "/Procedural Map/Data/Maps/map.png", map.EncodeToPNG());

            heightBuffer.Release();
            normalBuffer.Release();
            heatBuffer.Release();
            humidityBuffer.Release();

            heightLevelBuffer.Release();
            heatLevelBuffer.Release();
            humidityLevelBuffer.Release();

            colorBuffer.Release();
        }
    }
}
