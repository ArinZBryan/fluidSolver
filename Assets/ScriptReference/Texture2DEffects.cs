using AdvancedEditorTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class Kernel
{
    float[,] kernel;
    public int size;
    public Kernel(float[,] kernel)
    {
        this.kernel = kernel;
        this.size = kernel.GetLength(0);
    }
    public Kernel(string representation)
    {
        var res = Regex.Matches(representation, @"\[([^\[]*?)*?\]");

        string[] rows = res.Select(x => x.Value).ToArray();
        List<float[]> matrix = new List<float[]>();

        foreach (string row in rows)
        {
            matrix.Add(row.Substring(1, row.Length - 2).Split(',').Select(x => x.Trim()).Select(x => float.Parse(x)).ToArray());
        }

        float[][] k = matrix.ToArray();
        this.kernel = new float[k.Length, k[0].Length];
        for (int i = 0; i < k.Length; i++)
        {
            for (int j = 0; j < k[0].Length; j++)
            {
                kernel[i, j] = k[i][j];
            }
        }
        this.size = kernel.Length;
    }
    public float[,] apply(float[,] source)
    {
        int sourceW = source.GetLength(0);
        int sourceH = source.GetLength(1);

        if (sourceW < this.size || sourceH < this.size)
        { throw new System.ArgumentException("Kernel size is larger than source size"); }

        //Extend array for processing
        float[,] extendedSource = new float[sourceW + this.size - 1, sourceH + this.size - 1];
        int extendedW = sourceW + this.size - 1;

        int boundaryCells = (this.size - 1) / 2;
        for (int i = -boundaryCells; i < sourceW + boundaryCells; i++)
        {
            for (int j = -boundaryCells; j < sourceH + boundaryCells; j++)
            {
                //fill in the boundary cells with the nearest source cell
                extendedSource[i + boundaryCells, j + boundaryCells] = source[Math.Clamp(i, 0, sourceW - 1), Math.Clamp(j, 0, sourceH -1)];
            }
        }   

        //Apply kernel
        for (int i = 0; i < sourceW; i++)
        {
            for (int j = 0; j < sourceH; j++)
            {
                float sum = 0;
                for (int k = 0; k < this.size; k++)
                {
                    for (int l = 0; l < this.size; l++)
                    {
                        sum += extendedSource[i + k, j + l] * this.kernel[k, l];
                    }
                }
                source[i, j] = sum;
            }
        }
        return source;
    }
    public Texture2D apply(Texture2D source)
    {
        int sourceW = source.width;
        int sourceH = source.height;

        PackedArray<Color> pixels = new PackedArray<Color>(source.GetPixels(), new int[] {sourceW, sourceH});
        if (sourceW < this.size || sourceH < this.size)
        { throw new System.ArgumentException("Kernel size is larger than source size"); }

        //Extend array for processing
        Color[,] extendedSource = new Color[sourceW + this.size - 1, sourceH + this.size - 1];
        int extendedW = sourceW + this.size - 1;

        int boundaryCells = (this.size - 1) / 2;
        for (int i = -boundaryCells; i < sourceW + boundaryCells; i++)
        {
            for (int j = -boundaryCells; j < sourceH + boundaryCells; j++)
            {
                //fill in the boundary cells with the nearest source cell
                extendedSource[i + boundaryCells, j + boundaryCells] = pixels[Math.Clamp(i, 0, sourceW - 1), Math.Clamp(j, 0, sourceH - 1)];
            }
        }

        Color color = new Color(0, 0, 0, 0);
        //Apply kernel
        for (int i = 0; i < sourceW; i++)
        {
            for (int j = 0; j < sourceH; j++)
            {
                float sumR = 0;
                float sumG = 0;
                float sumB = 0;
                float sumA = 0;
                for (int k = 0; k < this.size; k++)
                {
                    for (int l = 0; l < this.size; l++)
                    {
                        sumR += extendedSource[i + k, j + l].r * this.kernel[k, l];
                        sumG += extendedSource[i + k, j + l].g * this.kernel[k, l];
                        sumB += extendedSource[i + k, j + l].b * this.kernel[k, l];
                        sumA += extendedSource[i + k, j + l].a * this.kernel[k, l];
                    }
                }
                color.r = sumR;
                color.g = sumG;
                color.b = sumB;
                color.a = sumA;
                pixels[i, j] = color;
            }
        }
        source.SetPixels(pixels.data);
        source.Apply();
        return source;
    }

    public static Kernel boxBlur3x3 = new Kernel(new float[,] { 
        { 1f/9f, 1f/9f, 1f/9f },
        { 1f/9f, 1f/9f, 1f/9f },
        { 1f/9f, 1f/9f, 1f/9f }
    });
    public static Kernel boxBlur5x5 = new Kernel(new float[,] { 
        { 1f/25f, 1f/25f, 1f/25f, 1f/25f, 1f/25f },
        { 1f/25f, 1f/25f, 1f/25f, 1f/25f, 1f/25f },
        { 1f/25f, 1f/25f, 1f/25f, 1f/25f, 1f/25f },
        { 1f/25f, 1f/25f, 1f/25f, 1f/25f, 1f/25f },
        { 1f/25f, 1f/25f, 1f/25f, 1f/25f, 1f/25f }
    });
    public static Kernel boxBlur7x7 = new Kernel(new float[,] {
        { 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f },
        { 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f },
        { 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f },
        { 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f },
        { 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f },
        { 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f },
        { 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f, 1f/49f }
    });
    public static Kernel gaussianBlur3x3 = new Kernel(new float[,] { 
        { 1f/16f, 2f/16f, 1f/16f },
        { 2f/16f, 4f/16f, 2f/16f },
        { 1f/16f, 2f/16f, 1f/16f }
    });
    public static Kernel gaussianBlur5x5 = new Kernel(new float[,] {
        { 1f/273f, 4f/273f , 7f/273f , 4f/273f,  1f/273f },
        { 4f/273f, 16f/273f, 26f/273f, 16f/273f, 4f/273f },
        { 7f/273f, 26f/273f, 41f/273f, 26f/273f, 7f/273f },
        { 4f/273f, 16f/273f, 26f/273f, 16f/273f, 4f/273f },
        { 1f/273f, 4f/273f , 7f/273f , 4f/273f,  1f/273f }
    });
    public static Kernel gaussianBlur7x7 = new Kernel(new float[,] {
        { 0f/1003f, 0f/1003f,  1f/1003f,  2f/1003f,   1f/1003f,  0f/1003f,  0f/1003f },
        { 0f/1003f, 3f/1003f,  13f/1003f, 22f/1003f,  13f/1003f, 3f/1003f,  0f/1003f },
        { 1f/1003f, 13f/1003f, 59f/1003f, 97f/1003f,  59f/1003f, 13f/1003f, 1f/1003f },
        { 2f/1003f, 22f/1003f, 97f/1003f, 159f/1003f, 97f/1003f, 22f/1003f, 2f/1003f },
        { 1f/1003f, 13f/1003f, 59f/1003f, 97f/1003f,  59f/1003f, 13f/1003f, 1f/1003f },
        { 0f/1003f, 3f/1003f,  13f/1003f, 22f/1003f,  13f/1003f, 3f/1003f,  0f/1003f },
        { 0f/1003f, 0f/1003f,  1f/1003f,  2f/1003f,   1f/1003f,  0f/1003f,  0f/1003f }
    });
    public static Kernel sharpen3x3 = new Kernel(new float[,]
    {
        { 0f, -1f, 0f },
        { -1f, 5f, -1f },
        { 0f, -1f, 0f }
    });
}

