using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    [CreateAssetMenu(fileName = "Field Generator", menuName = "Procedural Map/Field Generator")]
    public class FieldGenerator : ScriptableObject
    {
        public ComputeShader compute;

        public GradientGenerator gradientGenerator;

        public float scale;

        public Vector2[] offset;

        public int octaves;
        public float lacunarity;
        public float persistance;

        public float[] GenerateFloats(int size)
        {
            ComputeBuffer fieldBuffer = GenerateComputeBuffer(size);

            float[] field = new float[size * size];

            fieldBuffer.GetData(field);

            fieldBuffer.Release();
            return field;
        }

        public ComputeBuffer GenerateComputeBuffer(int size)
        {
            if (offset.Length != octaves)
            {
                Debug.LogWarning("offset data has inappropriate length, it need to be equal octaves");
                return null;
            }

            ComputeBuffer gradientBuffer = gradientGenerator.Generate();
            ComputeBuffer offsetBuffer = new ComputeBuffer(octaves, 8);
            ComputeBuffer fieldBuffer = new ComputeBuffer(size * size, 4);

            offsetBuffer.SetData(offset);

            compute.SetBuffer(0, "gradient", gradientBuffer);
            compute.SetInt("gradientSize", gradientGenerator.size);

            compute.SetInt("size", size);
            compute.SetFloat("scale", scale);
            compute.SetInt("octaves", octaves);
            compute.SetFloat("lacunarity", lacunarity);
            compute.SetFloat("persistance", persistance);
            compute.SetBuffer(0, "offset", offsetBuffer);

            compute.SetBuffer(0, "field", fieldBuffer);

            int threadGroup = Mathf.CeilToInt(size / 8f);

            compute.Dispatch(0, threadGroup, threadGroup, 1);

            offsetBuffer.Release();
            gradientBuffer.Release();

            return fieldBuffer;
        }
    }
}
