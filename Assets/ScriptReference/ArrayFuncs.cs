using System.Collections;
using System.Collections.Generic;
using System.Text;

public static class PackedArrayFuncs
{
    //FIXME:    This calculates the valid indicies incorrectly, for the time being, the above function is replacing this.
    //          It has no support for brushes larger than 1, so this needs doing sometime soon.
    //          In the mean time, a workaround is to just have a really large value to paint with, and let diffusion do its hting
    public static string paintTo1DArrayAs2D<T>(ref PackedArray<T> arr, T value, int x, int y, int arrW, int arrH, int brushSize)
    {
        int a = (brushSize - 1) / 2;
        List<int> validIndicies = new List<int>(brushSize * brushSize);
        for (int i = -a; i <= a; i++) for (int j = -a; j <= a; j++)
            {
                int index = (y + i) + (x + j) * arrW;
                if (y + i >= arrW || y + i < 0) { continue; }
                if (x + j >= arrH || x + j < 0) { continue; }
                if (index > arr.length) { continue; }
                validIndicies.Add(index);
            }

        foreach (int index in validIndicies)
        {
            arr[index] = value;
        }

        return printArray2D(arr);
    }

    public static string printArray2D<T>(PackedArray<T> matrix)
    {
        if (matrix.dimensions.Length != 2) { return ""; }
        StringBuilder stringBuilder = new StringBuilder();
        int longestChar = 0;
        for (int row = 0; row < matrix.dimensions[0]; row++)
        {
            for (int column = 0; column < matrix.dimensions[1]; column++)
            {
                if (matrix[row, column].ToString().Length > longestChar)
                {
                    longestChar = matrix[row, column].ToString().Length;
                }

            }
        }
        int xPadding = matrix.dimensions[1] * 2 + matrix.dimensions[1] * longestChar;
        for (int xDisplay = 0; xDisplay < matrix.dimensions[0]; xDisplay++)
        {
            for (int yDisplay = 0; yDisplay < matrix.dimensions[1]; yDisplay++)
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
                if (yDisplay == matrix.dimensions[1] - 1)
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
    public static void getSlice2D<T>(in PackedArray<T> values, int xStart, int yStart, int xEnd, int yEnd, out PackedArray<T> ret)
    {
        if (values.dimensions.Length != 2) { ret = new PackedArray<T>(new int[]{ 0 });  return; }
        int width = values.dimensions[0];
        int height = values.dimensions[1];
        ret = new PackedArray<T>(new int[]{ xEnd - xStart, yEnd - yStart });
        for (int i = xStart; i < xEnd; i++) for (int j = yStart; i < yEnd; j++)
            {
                ret[i - xStart, j - yStart] = values[i, j];
            }
    }
    public static PackedArray<T> scaleArrayAs2D<T>(PackedArray<T> arr, int scale)
    {
        if (arr.dimensions.Length != 2) { return new PackedArray<T>(new int[] { 0 }); }
        PackedArray<T> dest = new PackedArray<T>(new int[] { arr.dimensions[0] * scale, arr.dimensions[1] * scale });
        for (int inputX = 0; inputX < arr.dimensions[0]; inputX++) for (int inputY = 0; inputY < arr.dimensions[1]; inputY++)
            {
                T item = arr[inputX, inputY];
                for (int outputX = 0; outputX < scale; outputX++) for (int outputY = 0; outputY < scale; outputY++)
                    {
                        int destX = inputX * scale + outputX;
                        int destY = inputY * scale + outputY;
                        dest[destX, destY] = item;
                    }
            }
        return dest;
    }
}

public struct PackedArray<T> : IEnumerable<T>
{
    public T[] data;
    public int length;
    public int[] dimensions;
    public PackedArray(int[] Dimensions)
    {
        this.dimensions = Dimensions;
        this.length = 1;
        foreach (int dim in Dimensions)
        {
            this.length *= dim;
        }
        data = new T[this.length];
    }
    public T this[int i]
    {
       get { return data[i]; }
       set { data[i] = value; }
    }
    public T this[int i, int j]
    {
        get { return data[i + j * dimensions[0]]; }
        set { data[i + j* dimensions[0]] = value; }
    }
    public T this[int i, int j, int k]
    {
        get { return data[i + j * dimensions[0] + k * dimensions[0] * dimensions[1]]; }
        set { data[i + j * dimensions[0] + k * dimensions[0] * dimensions[1]] = value; }
}
    public IEnumerator<T> GetEnumerator()
    {
        return (IEnumerator<T>)data.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return data.GetEnumerator();
    }
}
