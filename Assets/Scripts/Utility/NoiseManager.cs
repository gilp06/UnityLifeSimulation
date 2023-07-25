using UnityEngine;

namespace Utility
{
    public class NoiseManager
    {

        public static float GetNoiseMap(float x, float y, float scale, float width, float height, int seed)
        {
            float xVal = (x/width + seed) * scale;
            float yVal = (y/height + seed) * scale;
            return Mathf.PerlinNoise(xVal, yVal);
        }
    }
}
