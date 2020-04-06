using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    public class MapDisplay : MonoBehaviour
    {
        MeshFilter meshFilter;


        public void GenerateMesh(float[,] heightMap)
        {
            meshFilter = GetComponent<MeshFilter>();

            int width = heightMap.GetLength(0);
            int length = heightMap.GetLength(1);
            Vector3[] vertices = new Vector3[width * length];
            int[] tris = new int[(width -1) * (length -1) * 6];
            int triIndex = 0;
            for(int x=0; x<width; x++)
                for(int y=0; y<length; y++)
                {
                    int i = x * length + y;
                    vertices[i] = new Vector3(x, heightMap[x, y], y);
                    if(x!=width-1 && y != length - 1)
                    {
                        tris[triIndex] = i;
                        tris[triIndex + 1] = i + 1;
                        tris[triIndex + 2] = i + 1 + length;

                        tris[triIndex + 3] = i;
                        tris[triIndex + 4] = i + 1 + length;
                        tris[triIndex + 5] = i + length;

                        triIndex += 6;
                    }
                }

            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = tris
            };

            mesh.RecalculateNormals();

            meshFilter.sharedMesh = mesh;
        }
    }
}
