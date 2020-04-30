using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap {
    public class MapDisplay : MonoBehaviour
    {
        public MapGenerator map;

        public int chunkSize;
        public float projectionScale;
        public float scale;

        public Material material;

        public void Create()
        {
            MapData data = map.GenerateMap();

            int chunkCount = Mathf.CeilToInt(data.size / ((float)chunkSize));

            Material mat = new Material(material);
            mat.mainTexture = data.map;

            for(int x=0; x< chunkCount; x++)
                for(int y=0; y<chunkCount; y++)
                {
                    GameObject chunk = new GameObject("Chunk (" + x.ToString() + ", " + y.ToString() + ")");
                    chunk.transform.position = new Vector3(x * chunkSize, 0, y * chunkSize);
                    MeshFilter mf = chunk.AddComponent<MeshFilter>();
                    mf.sharedMesh = data.GetMesh(x * chunkSize, y * chunkSize, chunkSize, 1.0f, 200.0f);
                    MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
                    mr.sharedMaterial = mat;
                    chunk.transform.parent = transform;
                }
        }
    }
}
