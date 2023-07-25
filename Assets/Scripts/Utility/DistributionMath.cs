﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utility
{
    public static class DistributionMath
    {
        public static float NextGaussian(float mean, float standardDeviation, float min, float max)
        {
            float x;
            do
            {
                x = NextGaussian(mean, standardDeviation);
            } while (x < min || x > max);

            return x;
        }
        
        public static float NextGaussian(float mean, float standardDeviation)
        {
            return mean + NextGaussian() * standardDeviation;
        }

        public static float NextGaussian()
        {
            float v1, v2, s;
            do
            {
                v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
                v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
                s = v1 * v1 + v2 * v2;
            } while (s >= 1.0f || s == 0f);

            s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);

            return v1 * s;
        }

        public static int NextPoisson(float lambda)
        {
            float expLambda = Mathf.Exp(-lambda);
            int k = 0;
            float p = 1;

            do
            {
                k++;
                p = p * Random.Range(0.0f, 1.0f);
            } while (p > expLambda);

            return k;
        }
    }
}