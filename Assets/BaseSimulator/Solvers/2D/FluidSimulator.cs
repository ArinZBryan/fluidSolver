using System;
using System.Linq;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

class FluidSimulator : MonoBehaviour, ISimulator
{
    public int gridSize = 32;
    int N;
    public int scale = 5;
    public float viscosity = 0;
    public float diffusionRate = 0;
    public float deltaTime = 0.1f;

    public float force = 100f;
    public float drawValue = 100f;
    public int penSize = 1;
    
    Texture2D densTex;
    Color[] densColour;
    Texture2D velTex;
    bool drawBoth = true;
    Texture2D bothTex;
    RenderTexture renderTexture;
    

    Solver2D solver;

    float mouseX = 0;
    float mouseY = 0;
    float mouseVelocityX = 0;
    float mouseVelocityY = 0;


    private void Awake()
    {
        densTex = new Texture2D(gridSize, gridSize);
        velTex = new Texture2D(gridSize * scale, gridSize * scale);
        bothTex = new Texture2D(gridSize, gridSize);
        renderTexture = new RenderTexture(gridSize * scale, gridSize * scale, 0);

        densTex.filterMode = FilterMode.Point;
        velTex.filterMode = FilterMode.Point;

        solver = new Solver2D(gridSize, diffusionRate, viscosity, deltaTime);
        N = gridSize + 2;
        densColour = new Color[gridSize * gridSize];
    }

    public RenderTexture getNextTexture()
    {
        //remap xy coords to be same as screen UV coords
        mouseX = Input.mousePosition.x;
        mouseY = Screen.height - Input.mousePosition.y;

        //clamp to area of simulation
        mouseX = Math.Clamp(mouseX, 0, gridSize * scale - 1);
        mouseY = Math.Clamp(mouseY, 0, gridSize * scale - 1);

        //get grid pos of cursor
        int cursorX = (int)(mouseX / scale);
        int cursorY = gridSize - (int)(mouseY / scale) - 1;

        //get mouse velocity
        mouseVelocityX = Input.GetAxis("Mouse X") * force;
        mouseVelocityY = Input.GetAxis("Mouse Y") * force;

        /*
         * 
         * This can be a bit funky with detecting the toggle, and I rarely use the ability to draw only velocity
        if (Input.GetKeyUp(KeyCode.T))
        {
            drawBoth = !drawBoth;
        }
        */

        if (Input.GetKey(KeyCode.V))
        {
            if (Input.GetMouseButton(0)) //LMB
            {
                ArrayFuncs.edit1DArrayAs2D(ref solver.getVelocityX(), mouseVelocityX, cursorX, cursorY, gridSize + 2, gridSize + 2);
                ArrayFuncs.edit1DArrayAs2D(ref solver.getVelocityY(), mouseVelocityY, cursorX, cursorY, gridSize + 2, gridSize + 2);
                //ArrayFuncs.paintTo1DArrayAs2D(ref solver.getVelocityX(), mouseVelocityX, cursorY, cursorX, gridSize, gridSize, penSize);
            }
            if (Input.GetMouseButton(1)) //RMB
            {
                //ArrayFuncs.paintTo1DArrayAs2D(ref solver.getVelocityY(), mouseVelocityY, cursorY, cursorX, gridSize, gridSize, penSize);
            }

            solver.vel_step();
            solver.dens_step();

            //zeros out prev_density - This prevents runaway densities
            solver.getDensityPrev() = Enumerable.Repeat(0f, (gridSize + 2) * (gridSize + 2)).ToArray();

            if (drawBoth)
            {
                drawDensityAndVelocity(solver.getDensity(), solver.getVelocityX(), solver.getVelocityY(), ref bothTex, ref densTex, Color.green);
            } else
            {
                drawVelocity(solver.getVelocityX(), solver.getVelocityY(), ref velTex, Color.black, Color.green);
            }
        }
        else
        {
            if (Input.GetMouseButton(0)) //LMB
            {
                ArrayFuncs.edit1DArrayAs2D(ref solver.getDensityPrev(), drawValue, cursorX, cursorY, gridSize + 2, gridSize + 2);
            }
            if (Input.GetMouseButton(1)) //RMB
            {
                ArrayFuncs.edit1DArrayAs2D(ref solver.getDensityPrev(), -drawValue, cursorX, cursorY, gridSize + 2, gridSize + 2);
            }

            solver.vel_step();
            solver.dens_step();

            //Zeros out prev_density - this prevents runaway densities
            solver.getDensityPrev() = Enumerable.Repeat(0f, (gridSize + 2) * (gridSize + 2)).ToArray();


            if (Input.GetKey(KeyCode.Y)) drawDensity(solver.getDensityPrev(), ref densTex);
            else drawDensity(solver.getDensity(), ref densTex);

        }
        return getCurrentTexture();
    }
    public RenderTexture getCurrentTexture()
    {
        ;
        if (Input.GetKey(KeyCode.V))
        {
            if (drawBoth) Graphics.Blit(bothTex, renderTexture);
            else Graphics.Blit(velTex, renderTexture);
        }
        else
        {
            Graphics.Blit(GPUTextureScaler.Scaled(densTex, gridSize * scale, gridSize * scale, FilterMode.Point), renderTexture);
        }
        return renderTexture;
    }
    public RenderTexture getGurrentExportableTexture()
    {
        Graphics.Blit(densTex, renderTexture);
        return renderTexture;
    }

    void drawDensity(in float[] density, ref Texture2D drawTex)
    {
        for (int simCellX = 1; simCellX <= gridSize; simCellX++) for (int simCellY = 1; simCellY <= gridSize; simCellY++)
            {
                //This was two calls to ArrayFuncs.AccessArray1DAs2D(..), but the function calls were a big time hog
                int colIndex = (simCellX - 1) + (simCellY - 1)*gridSize;
                int denIndex = (simCellX) +(simCellY)*(gridSize + 2);
                densColour[colIndex].r = density[denIndex];
                densColour[colIndex].g = density[denIndex];
                densColour[colIndex].b = density[denIndex];
                densColour[colIndex].a = 1f;
            }
        drawTex.SetPixels(densColour);
        drawTex.Apply();

    }
    void drawVelocity(in float[] velocityX, in float[] velocityY, ref Texture2D drawTex, Color background, Color foreground)
    {
        int maxLength = (scale - 1) / 2;

        //Makes a texture of one colour
        Color[] pixels = Enumerable.Repeat(background, Screen.width * Screen.height).ToArray();
        drawTex.SetPixels(pixels);
        Vector2 normalised;
        Vector2 velocity;
        int xCoord, yCoord;
        for (int i = 1; i < N; i++)
        {
            for (int j = 1; j < N; j++)
            {
                velocity.x = ArrayFuncs.accessArray1DAs2D(i, j, N, N, velocityX);
                velocity.y = ArrayFuncs.accessArray1DAs2D(i, j, N, N, velocityY);

                normalised = velocity.normalized;

                xCoord = (i - 1) * scale + 2;
                yCoord = (j - 1) * scale + 2;
                line(ref drawTex, xCoord, yCoord, (int)Math.Round((normalised * maxLength).x) + xCoord, (int)Math.Round((normalised * maxLength).y) + yCoord, foreground);

            }
        }
        drawTex.Apply();
    }
    public void drawDensityAndVelocity(in float[] density, in float[] velocityX, in float[] velocityY, ref Texture2D bothTex, ref Texture2D densTex, Color foreground)
    {
        bothTex.Reinitialize(gridSize, gridSize);
        for (int simCellX = 1; simCellX <= gridSize; simCellX++) for (int simCellY = 1; simCellY <= gridSize; simCellY++)
            {
                //This was two calls to ArrayFuncs.AccessArray1DAs2D(..), but the function calls were a big time hog
                int colIndex = (simCellX - 1) + (simCellY - 1) * gridSize;
                int denIndex = (simCellX) + (simCellY) * (gridSize + 2);

                densColour[colIndex].r = density[denIndex];
                densColour[colIndex].g = density[denIndex];
                densColour[colIndex].b = density[denIndex];
                densColour[colIndex].a = 1f;
            }
        bothTex.SetPixels(densColour);
        bothTex.Apply();
        GPUTextureScaler.Scale(bothTex, gridSize * scale, gridSize * scale, FilterMode.Point);
        densTex.SetPixels(bothTex.GetPixels());
        
        //Makes a texture of one colour


        int maxLength = (scale - 1) / 2;

        Vector2 normalised;
        Vector2 velocity;
        int xCoord, yCoord;
        for (int i = 1; i < N; i++)
        {
            for (int j = 1; j < N; j++)
            {
                velocity.x = ArrayFuncs.accessArray1DAs2D(i, j, N, N, velocityX);
                velocity.y = ArrayFuncs.accessArray1DAs2D(i, j, N, N, velocityY);

                normalised = velocity.normalized;

                xCoord = (i - 1) * scale + 2;
                yCoord = (j - 1) * scale + 2;
                line(ref bothTex, xCoord, yCoord, (int)Math.Round((normalised * maxLength).x) + xCoord, (int)Math.Round((normalised * maxLength).y) + yCoord, foreground);

            }
        }
        bothTex.Apply();
    }
    public void line(ref Texture2D tex, int x, int y, int x2, int y2, Color color)
    {
        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            tex.SetPixel(x, y, color);
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
    }
    

    /*IN-EDITOR DEBUG BUTTONS*/
    [Button("Print Density")]
    void printDensity() { Debug.Log(ArrayFuncs.printArray2D<float>(ArrayFuncs.array1Dto2D(solver.getDensity(), gridSize + 2, gridSize + 2))); }
    [Button("Print Previous Density")]
    void printPrevDensity() { Debug.Log(ArrayFuncs.printArray2D<float>(ArrayFuncs.array1Dto2D(solver.getDensityPrev(), gridSize + 2, gridSize + 2))); }
}