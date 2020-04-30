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
        public float vertexDistanceToCalculateNormals;
        [Space(5f)]
        public ComputeShader heightCompute;
        public ComputeShader normalCompute;
        public ComputeShader heatCompute;
        public ComputeShader humidityCompute;
        public ComputeShader biomesCompute;
        public ComputeShader landTexCompute;

        [Space(5f)]
        public float[] heightLevel;
        public float[] heatLevel;
        public float[] humidityLevel;

        public Texture2D biomesSample;
        public Texture2D waterSample;

        public Vector4 lightDir;
        public MapData GenerateMap()
        {
            MapData data = new MapData(size);

            int threadGroup = Mathf.CeilToInt(size / 8f);

            #region height
            ComputeBuffer heightBuffer = heightGenerator.GenerateComputeBuffer(size);

            heightCompute.SetBuffer(0, "height", heightBuffer);
            heightCompute.SetInt("size", size);

            heightCompute.Dispatch(0, threadGroup, threadGroup, 1);

            heightBuffer.GetData(data.height);
            #endregion            
            #region normal
            ComputeBuffer normalBuffer = new ComputeBuffer(size * size, 12);

            normalCompute.SetBuffer(0, "normal", normalBuffer);
            normalCompute.SetBuffer(0, "height", heightBuffer);
            normalCompute.SetInt("size", size);
            normalCompute.SetFloat("vertexDistance", vertexDistanceToCalculateNormals);

            normalCompute.Dispatch(0, threadGroup, threadGroup, 1);

            normalBuffer.GetData(data.normal);
            #endregion
            #region heat
            ComputeBuffer heatBuffer = heatGenerator.GenerateComputeBuffer(size);
           
            heatCompute.SetBuffer(0, "heat", heatBuffer);
            heatCompute.SetBuffer(0, "height", heightBuffer);
            heatCompute.SetInt("size", size);

            heatCompute.Dispatch(0, threadGroup, threadGroup, 1);
            
            heatBuffer.GetData(data.heat);
            #endregion
            #region humidity
            ComputeBuffer humidityBuffer = humidityGenerator.GenerateComputeBuffer(size);

            humidityCompute.SetBuffer(0, "humidity", humidityBuffer);
            humidityCompute.SetBuffer(0, "height", heightBuffer);
            humidityCompute.SetInt("size", size);

            humidityCompute.Dispatch(0, threadGroup, threadGroup, 1);

            humidityBuffer.GetData(data.humidity);
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


            landTexCompute.SetBuffer(0, "color", colorBuffer);
            landTexCompute.SetBuffer(0, "height", heightBuffer);
            landTexCompute.SetBuffer(0, "normal", normalBuffer);
            landTexCompute.SetBuffer(0, "heat", heatBuffer);
            landTexCompute.SetBuffer(0, "humidity", humidityBuffer);
            landTexCompute.SetInt("size", size);
            landTexCompute.SetBuffer(0, "heightLevel", heightLevelBuffer);
            landTexCompute.SetBuffer(0, "heatLevel", heatLevelBuffer);
            landTexCompute.SetBuffer(0, "humidityLevel", humidityLevelBuffer);
            landTexCompute.SetInt("heightLevelCount", heightLevel.Length);
            landTexCompute.SetInt("heatLevelCount", heatLevel.Length);
            landTexCompute.SetInt("humidityLevelCount", humidityLevel.Length);
            landTexCompute.SetTexture(0, "landSample", biomesSample);
            landTexCompute.SetTexture(0, "waterSample", waterSample);

            landTexCompute.Dispatch(0, threadGroup, threadGroup, 1);

            colorBuffer.GetData(colors);

            map.SetPixels(colors);
            map.filterMode = FilterMode.Point;
            map.wrapMode = TextureWrapMode.Clamp;

            map.Apply();

            data.map = map;

            heightBuffer.Release();
            normalBuffer.Release();
            heatBuffer.Release();
            humidityBuffer.Release();

            heightLevelBuffer.Release();
            heatLevelBuffer.Release();
            humidityLevelBuffer.Release();

            colorBuffer.Release();

            return data;
        }

        
    }

    public class MapData
    {
        public int size;
        public Texture2D map;
        public float[] height;
        public float[] heat;
        public float[] humidity;
        public Vector3[] normal;

        public MapData(int _size)
        {
            size = _size;
            map = new Texture2D(size, size);
            height = new float[size * size];
            heat = new float[size * size];
            humidity = new float[size * size];
            normal = new Vector3[size * size];
        }


        public Mesh GetMesh(int sX, int sY, int d, float scale, float heightMultiplier)
        {
            int dx = Mathf.Min(d + 1, size - sX);
            int dy = Mathf.Min(d + 1, size - sY);
            Vector3[] vertices = new Vector3[dx * dy];
            Vector2[] uvs = new Vector2[dx * dy];
            int[] triangles = new int[(dx - 1) * (dy - 1) * 6];

            int vIndex = 0, tIndex = 0;

            float uvMultiplier = 1.0f / size;

            for (int x = 0; x < dx; x++)
                for (int y = 0; y < dy; y++)
                {
                    vertices[vIndex] = new Vector3(x * scale, height[(sY + y) * size + sX + x] * heightMultiplier * scale, y * scale);
                    uvs[vIndex] = new Vector2((sX + x) * uvMultiplier, (sY + y) * uvMultiplier);
                    if (x != dx - 1 && y != dy - 1)
                    {
                        triangles[tIndex] = vIndex;
                        triangles[tIndex + 1] = vIndex + 1;
                        triangles[tIndex + 2] = vIndex + dy + 1;

                        triangles[tIndex + 3] = vIndex;
                        triangles[tIndex + 4] = vIndex + dy + 1;
                        triangles[tIndex + 5] = vIndex + dy;

                        tIndex += 6;
                    }
                    vIndex++;
                }

            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };

            mesh.RecalculateNormals();

            return mesh;
        }
    }
}
