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
        public ComputeShader heightEditor;
        public ComputeShader heatEditor;
        public ComputeShader humidityEditor;

        [Space(5f)]
        public float deepWaterLevel;
        public float waterLevel;
        public float shallowWaterLevel;
        public float sandLevel;
        public float grassLevel;
        public float rockLevel;
        public float hardRockLevel;

        public float iceLevel;

        public Color GetColor(float height, float heat, float humidity)
        {
            
            if (heat < iceLevel)
                return new Color(0.9f, 0.9f, 1f, 1f);

            if (height < deepWaterLevel)
                return new Color(0, 0, 0.7f, 1);
            else if (height < waterLevel)
                return Color.blue;
            else if (height < shallowWaterLevel)
                return new Color(0.3f, 0.7f, 1, 1);
            else if (height < sandLevel)
            {
                return Color.yellow;
            }
            else if (height < grassLevel)
            {
                return Color.green;
            }
            else if (height < rockLevel)
            {
                return Color.grey;
            }
            else if (height < hardRockLevel)
            {
                return new Color(0.3f, 0.3f, 0.3f, 1);
            }
            else
                return Color.white;
        }

        public void GenerateMap()
        {

            float[] height = heightGenerator.GenerateFloats(size);
            float[] heat = new float[size * size];
            float[] humidity = humidityGenerator.GenerateFloats(size);

            int threadGroup = Mathf.CeilToInt(size / 8f);


            ComputeBuffer heatBuffer = heatGenerator.GenerateComputeBuffer(size);
           
            heatEditor.SetBuffer(0, "heat", heatBuffer);
            heatEditor.SetInt("size", size);

            heatEditor.Dispatch(0, threadGroup, threadGroup, 1);
            
            heatBuffer.GetData(heat);

            Texture2D map = new Texture2D(size, size);

            Color[] cs = new Color[size * size];
            for (int i = 0; i < cs.Length; i++)
                cs[i] = GetColor(height[i], heat[i], humidity[i]);

            map.SetPixels(cs);
            map.filterMode = FilterMode.Point;
            map.wrapMode = TextureWrapMode.Clamp;

            map.Apply();

            File.WriteAllBytes(Application.dataPath + "/Procedural Map/Maps/map.png", map.EncodeToPNG());

            heatBuffer.Release();
        }

        private void Awake()
        {
            GenerateMap();
        }

        private void Update()
        {
            GenerateMap();
        }
    }
}
