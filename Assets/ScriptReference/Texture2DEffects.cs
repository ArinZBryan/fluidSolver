using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITexture2DEffect
{
    void runEffect(ref Vector4[,] texture);
}

public class Texture2DEffects
{ 
    public class NoEffect : ITexture2DEffect
    {
        public void runEffect(ref Vector4[,] vectors)
        {
            //Do Nothing
        }
    }

    public class Blur : ITexture2DEffect
    {
        public void runEffect(ref Vector4[,] vectors)
        {
            int imageWidth = vectors.GetLength(0);
            int imageHeight = vectors.GetLength(1);
            for (int x = 1; x < imageWidth - 1; x++)
            {
                for (int y = 1; y < imageHeight - 1; y++)
                {
                    vectors[x, y] = (vectors[x - 1, y + 1] + vectors[x, y + 1] + vectors[x + 1, y + 1] +
                                    vectors[x - 1, y] + vectors[x, y] + vectors[x + 1, y] +
                                    vectors[x - 1, y - 1] + vectors[x, y - 1] + vectors[x + 1, y - 1] ) / 9;
                }
            } 
        }
    } 
}
