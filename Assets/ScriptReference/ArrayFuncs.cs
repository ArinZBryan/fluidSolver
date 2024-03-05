using System.Collections;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public struct PackedArray<T> : IEnumerable<T>
{
    public T[] data;
    public readonly int length;
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
    //FIXME:    This calculates the valid indicies incorrectly, for the time being, the above function is replacing this.
    //          It has no support for brushes larger than 1, so this needs doing sometime soon.
    //          In the mean time, a workaround is to just have a really large value to paint with, and let diffusion do its hting
    public PackedArray<T> paintTo1DArrayAs2D(T value, int x, int y, int arrW, int arrH, int brushSize)
    {
        int a = (brushSize - 1) / 2;
        List<int> validIndicies = new List<int>(brushSize * brushSize);
        for (int i = -a; i <= a; i++) for (int j = -a; j <= a; j++)
            {
                int index = (y + i) + (x + j) * arrW;
                if (y + i >= arrW || y + i < 0) { continue; }
                if (x + j >= arrH || x + j < 0) { continue; }
                if (index > this.length) { continue; }
                validIndicies.Add(index);
            }

        foreach (int index in validIndicies)
        {
            this[index] = value;
        }

        return this;
    }
    public string To2DString()
    {
        if (this.dimensions.Length != 2) { return ""; }
        StringBuilder stringBuilder = new StringBuilder();
        int longestChar = 0;
        for (int row = 0; row < this.dimensions[0]; row++)
        {
            for (int column = 0; column < this.dimensions[1]; column++)
            {
                if (this[row, column].ToString().Length > longestChar)
                {
                    longestChar = this[row, column].ToString().Length;
                }

            }
        }
        int xPadding = this.dimensions[1] * 2 + this.dimensions[1] * longestChar;
        for (int xDisplay = 0; xDisplay < this.dimensions[0]; xDisplay++)
        {
            for (int yDisplay = 0; yDisplay < this.dimensions[1]; yDisplay++)
            {
                if (yDisplay == 0)
                {
                    stringBuilder.Append(" ");
                }
                int requiredPadding = longestChar - this[xDisplay, yDisplay].ToString().Length;
                if (this[xDisplay, yDisplay].ToString().Length < longestChar)
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

                stringBuilder.Append(this[xDisplay, yDisplay].ToString());
                stringBuilder.Append(" ");
                if (yDisplay == this.dimensions[1] - 1)
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
    public string To1DString()
    {
        return "[ " + string.Join(", ", this.data) + "]";
    }
    public void getSlice2D(int xStart, int yStart, int xEnd, int yEnd, out PackedArray<T> ret)
    {
        if (this.dimensions.Length != 2) { ret = new PackedArray<T>(new int[] { 0 }); return; }
        int width = this.dimensions[0];
        int height = this.dimensions[1];
        ret = new PackedArray<T>(new int[] { xEnd - xStart, yEnd - yStart });
        for (int i = xStart; i < xEnd; i++) for (int j = yStart; i < yEnd; j++)
            {
                ret[i - xStart, j - yStart] = this[i, j];
            }
    }
    public PackedArray<T> scaleArrayAs2D(int scale)
    {
        if (this.dimensions.Length != 2) { return new PackedArray<T>(new int[] { 0 }); }
        PackedArray<T> dest = new PackedArray<T>(new int[] { this.dimensions[0] * scale, this.dimensions[1] * scale });
        for (int inputX = 0; inputX < this.dimensions[0]; inputX++) for (int inputY = 0; inputY < this.dimensions[1]; inputY++)
            {
                T item = this[inputX, inputY];
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