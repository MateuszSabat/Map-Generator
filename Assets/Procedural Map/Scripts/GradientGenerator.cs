using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMap
{
    [CreateAssetMenu(fileName = "Gradient Generator", menuName = "Procedural Map/Gradient Generator")]
    public class GradientGenerator : ScriptableObject
    {
        public ComputeShader gradientGeneratorCompute;

        public int seed;
        public int size;
        public float scale;

        public ComputeBuffer Generate()
        {
            ComputeBuffer rndGenerators = new ComputeBuffer(2, 20);

            RndGenerator[] gen = new RndGenerator[2];
            gen[0] = new RndGenerator();
            gen[1] = new RndGenerator();

            int s = seed;
            s = xorshift(s) % 99;
            gen[0].wx = (s + 0.25356234f) / 22.03460124817f;

            s = xorshift(s * s - 9173571) % 99;
            gen[0].wy = (s - 0.6913404f) / 28.81307123f;

            s = xorshift(s / 814 + 5134071) % 99;
            gen[0].m = (s - 0.6913404f) / 40.9341705134f;

            s = xorshift(s * 134 - 515690871) % 99;
            gen[0].offsetx = (s + 5.5462f) / 10.012386134f;

            s = xorshift(s * s / 17 + 90151346) % 99;
            gen[0].offsety = (s - 8.51342f) / 51.07349237283f;


            s = xorshift(s * 71924 - 127823) % 99;
            gen[1].wx = (s + 0.25356234f) / 41.03450124817f;

            s = xorshift(s * s - 9173571) % 99;
            gen[1].wy = (s - 0.9913404f) / 13.808237123f;

            s = xorshift(s / 814 + 5134071) % 99;
            gen[1].m = (s - 9.2913404f) / 30.9031705134f;

            s = xorshift(s * 134 - 515690871) % 99;
            gen[1].offsetx = (s + 1.63462f) / 52.92086134f;

            s = xorshift(s * s / 17 + 90151346) % 99;
            gen[1].offsety = (s - 2.512642f) / 31.07349237283f;


            rndGenerators.SetData(gen);

            ComputeBuffer gradients = new ComputeBuffer(size * size, 8);

            gradientGeneratorCompute.SetBuffer(0, "gradient", gradients);
            gradientGeneratorCompute.SetBuffer(0, "rndGenerator", rndGenerators);

            gradientGeneratorCompute.SetInt("size", size);
            gradientGeneratorCompute.SetFloat("scale", scale);

            int threadGroup = Mathf.CeilToInt(size / 8f);

            gradientGeneratorCompute.Dispatch(0, threadGroup, threadGroup, 1);

            rndGenerators.Release();

            return gradients;
        }

        public int xorshift(int x)
        {
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 7;
            return x;
        }

        struct RndGenerator
        {
            public float wx;
            public float wy;
            public float m;
            public float offsetx;
            public float offsety;
        }       
    }
}
