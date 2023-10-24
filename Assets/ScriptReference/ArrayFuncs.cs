using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Editor.Tasks;
using UnityEngine;

public static class ArrayFuncs
{
    public static void edit1DArrayAs2D<T>(ref T[] arr, T value, int x, int y, int arrW, int arrH)
    {
        T[,] values = array1Dto2D(arr, arrW, arrH);
        values[x,y] = value;
        arr = array2Dto1D(values);
    }

    //FIXME:    This calculates the valid indicies incorrectly, for the time being, the above function is replacing this.
    //          It has no support for brushes larger than 1, so this needs doing sometime soon.
    //          In the mean time, a workaround is to just have a really large value to paint with, and let diffusion do its hting
    public static string paintTo1DArrayAs2D<T>(ref T[] arr, T value, int x, int y, int arrW, int arrH, int brushSize)
    {
        int a = (brushSize - 1) / 2;
        List<int> validIndicies = new List<int>(brushSize * brushSize);
        for (int i = -a; i <= a; i++) for (int j = -a; j <= a; j++)
            {
                int index = (y + i) + (x + j) * arrW;
                if (y + i >= arrW || y + i < 0) { continue; }
                if (x + j >= arrH || x + j < 0) { continue; }
                if (index > arr.Length) { continue; }
                validIndicies.Add(index);
            }

        foreach (int index in validIndicies)
        {
            arr[index] = value;
        }

        return printArray2DMatrix(array1Dto2D(arr, arrW + 2, arrH + 2));
    }

    public static T[] array2Dto1D<T>(T[,] A)
    {
        int len = A.GetLength(0) * A.GetLength(1);
        T[] ret = new T[len];
        for (int i = 0; i < A.GetLength(0); i++) for (int j = 0; j < A.GetLength(1); j++)
            {
                ret[i + j * A.GetLength(0)] = A[i, j];
            }
        return ret;
    }

    public static T[,] array1Dto2D<T>(T[] A, int width, int height)
    {
        int len = width * height;
        if (len != A.Length)
        {
            StringBuilder ExceptionMessage = new StringBuilder();
            ExceptionMessage.Append("Expected length of A to be equal to width*height. A's lenth was: ");
            ExceptionMessage.Append(A.Length.ToString());
            ExceptionMessage.Append(" width*height was: ");
            ExceptionMessage.Append((width * height).ToString());
            Debug.LogError(ExceptionMessage.ToString());
        }
        T[,] ret = new T[width, height];
        for (int i = 0; i < width; i++) for (int j = 0; j < height; j++)
            {
                ret[i, j] = A[i + j * width];
            }
        return ret;
    }

    public static string printArray2D<T>(T[,] arr, string delimiter = "\t")
    {
        int rowLength = arr.GetLength(0);
        int colLength = arr.GetLength(1);
        StringBuilder msg = new StringBuilder();
        for (int i = 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                msg.Append(string.Format("{0} ", arr[i, j]));
            }
            msg.Append("\n");
        }
        //Console.WriteLine(msg.ToString());
        return msg.ToString();
    }
    public static string printArray2DMatrix<T>(T[,] matrix)
    {
        StringBuilder stringBuilder = new StringBuilder();
        int longestChar = 0;
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            for (int column = 0; column < matrix.GetLength(1); column++)
            {
                if (matrix[row, column].ToString().Length > longestChar)
                {
                    longestChar = matrix[row, column].ToString().Length;
                }

            }
        }
        int xPadding = matrix.GetLength(1) * 2 + matrix.GetLength(1) * longestChar;
        for (int padding = 0; padding < xPadding; padding++)
        {
            stringBuilder.Append(" ");
        }
        for (int xDisplay = 0; xDisplay < matrix.GetLength(0); xDisplay++)
        {
            for (int yDisplay = 0; yDisplay < matrix.GetLength(1); yDisplay++)
            {
                if (yDisplay == 0)
                {
                    stringBuilder.Append(" ");
                }
                int requiredPadding = longestChar - matrix[xDisplay, yDisplay].ToString().Length;
                if (matrix[xDisplay, yDisplay].ToString().Length < longestChar)
                {
                    if (requiredPadding % 2 != 0)
                    {
                        stringBuilder.Append(" ");
                        requiredPadding--;
                    }
                    for (int padding = 0; padding < (requiredPadding) / 2; padding++)
                    {
                        stringBuilder.Append(" ");
                    }
                }
                stringBuilder.Append(" ");

                stringBuilder.Append(matrix[xDisplay, yDisplay].ToString());
                stringBuilder.Append(" ");
                if (yDisplay == matrix.GetLength(1) - 1)
                {
                    stringBuilder.Append(" ");
                }
                for (int padding = 0; padding < requiredPadding / 2; padding++)
                {
                    stringBuilder.Append(" ");
                }
            }
            stringBuilder.Append(" \n");
        }
        return stringBuilder.ToString();
    }

    public static T accessArray1DAs2D<T>(int x, int y, int width, int height, in T[] array)
    {
        if (x >= width || x < 0) { Debug.LogError("Attempted 1D array access using out-of-bounds 2D Coordinates"); }
        if (y >= height || y < 0) { Debug.LogError("Attempted 1D array access using out-of-bounds 2D Coordinates"); }
        return array[x + y * width];
    }
    public static T accessArray2Das1D<T>(int index, in T[,] array)
    {
        int xLen = array.GetLength(0);
        int yLen = array.GetLength(1);
        if (index > (xLen - 1) * (yLen - 1)) { Debug.Log("Attempted 2D array access using out-of-bounds 1D Index"); }
        int x = index % (xLen - 1);
        int y = index / (xLen - 1);
        return array[x, y];
    }
    public static int accessArray1DAs2D(int x, int y, int width, int height)
    {
        /*
        if (x >= width) {
            Debug.LogError("Attempted 1D array access using out-of-bounds 2D Coordinates (x Coordinate)" + "(" + x.ToString() + " >= "  + width.ToString() + ")"); }
        if (x < 0) { 
            Debug.LogError("Attempted 1D array access using out-of-bounds 2D Coordinates (x Coordinate)" + "(" + x.ToString() + " < 0)"); }
        if (y >= height) { 
            Debug.LogError("Attempted 1D array access using out-of-bounds 2D Coordinates (y Coordinate)" + "(" + y.ToString() + " >= " + height.ToString() + ")"); }
        if (y < 0) { 
            Debug.LogError("Attempted 1D array access using out-of-bounds 2D Coordinates (y Coordinate)" + "(" + y.ToString() + " < 0)"); }
        */
        return x + y * width;
    }
    public static T[] getSlice1DFromArray1D<T>(in T[] values, int start, int length)
    {
        return values.Skip(start).Take(length).ToArray();
    }
    public static void getSlice2DFromArray2D<T>(in T[,] values, int xStart, int yStart, int xEnd, int yEnd, out T[,] ret)
    {
        int width = values.GetLength(0);
        int height = values.GetLength(1);
        ret = new T[xEnd - xStart, yEnd - yStart];
        for (int i = xStart; i < xEnd; i++) for (int j = yStart; i < yEnd; j++)
            {
                ret[i - xStart, j - yStart] = values[i, j];
            }
    }
}
