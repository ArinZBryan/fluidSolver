using System;
using System.Collections.Generic;
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
    PackedArray<Color> densColour;
    Texture2D velTex;
    PackedArray<Color> velColour;

    bool drawBoth = true;
    Texture2D bothTex;
    PackedArray<Color> bothColour;
    RenderTexture renderTexture;
    
    List<SimulationObject> objects = new List<SimulationObject>();
    Texture2D objectTex;
    PackedArray<Color> objectColour;

    Solver2D solver;

    float mouseX = 0;
    float mouseY = 0;
    float mouseVelocityX = 0;
    float mouseVelocityY = 0;


    private void Awake()
    {
        densTex = new Texture2D(gridSize * scale, gridSize * scale);
        velTex = new Texture2D(gridSize * scale, gridSize * scale);
        bothTex = new Texture2D(gridSize * scale, gridSize * scale);
        objectTex = new Texture2D(gridSize * scale, gridSize * scale);
        renderTexture = new RenderTexture(gridSize * scale, gridSize * scale, 0);

        densTex.filterMode = FilterMode.Point;
        velTex.filterMode = FilterMode.Point;

        solver = new Solver2D(gridSize, diffusionRate, viscosity, deltaTime);
        N = gridSize + 2;
        densColour = new PackedArray<Color>(new int[]{ gridSize, gridSize });
        velColour = new PackedArray<Color>(new int[] { gridSize * scale, gridSize * scale });
        bothColour = new PackedArray<Color>(new int[] { gridSize * gridSize, gridSize * gridSize });
        objectColour = new PackedArray<Color>(new int[] { gridSize * scale, gridSize * scale });
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

        runSimulationObjects();
        if (Input.GetKey(KeyCode.O))
        {
            drawSimulationObjects();
        }
        else if (Input.GetKey(KeyCode.V))
        {
            if (Input.GetMouseButton(0)) //LMB
            {
                solver.getVelocityX()[cursorX, cursorY] = mouseVelocityX;
                solver.getVelocityY()[cursorX, cursorY] = mouseVelocityY;
                //ArrayFuncs.paintTo1DArrayAs2D(ref solver.getVelocityX(), mouseVelocityX, cursorY, cursorX, gridSize, gridSize, penSize);
            }
            if (Input.GetMouseButton(1)) //RMB
            {
                //ArrayFuncs.paintTo1DArrayAs2D(ref solver.getVelocityY(), mouseVelocityY, cursorY, cursorX, gridSize, gridSize, penSize);
            }

            solver.vel_step();
            solver.dens_step();

            //zeros out prev_density - This prevents runaway densities
            solver.getDensityPrev().data = Enumerable.Repeat(0f, (gridSize + 2) * (gridSize + 2)).ToArray();

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
                solver.getDensityPrev()[cursorX, cursorY] = drawValue;
            }
            if (Input.GetMouseButton(1)) //RMB
            {
                solver.getDensityPrev()[cursorX, cursorY] = -drawValue;
            }

            solver.vel_step();
            solver.dens_step();

            //Zeros out prev_density - this prevents runaway densities
            solver.getDensityPrev().data = Enumerable.Repeat(0f, (gridSize + 2) * (gridSize + 2)).ToArray();


            if (Input.GetKey(KeyCode.Y)) drawDensity(solver.getDensityPrev(), ref densTex);
            else drawDensity(solver.getDensity(), ref densTex);

        }
        return getCurrentTexture();
    }
    public RenderTexture getCurrentTexture()
    {
        if (Input.GetKey(KeyCode.O))
        {
            Graphics.Blit(objectTex, renderTexture);
        }
        else if (Input.GetKey(KeyCode.V))
        {
            if (drawBoth) Graphics.Blit(bothTex, renderTexture);
            else Graphics.Blit(velTex, renderTexture);
        }
        else
        {
            Graphics.Blit(densTex, renderTexture);
        }
        return renderTexture;
    }
    public RenderTexture getGurrentExportableTexture()
    {
        Graphics.Blit(densTex, renderTexture);
        return renderTexture;
    }

    void drawDensity(in PackedArray<float> density, ref Texture2D drawTex)
    {
        Color col;
        for (int simCellX = 1; simCellX <= gridSize; simCellX++) for (int simCellY = 1; simCellY <= gridSize; simCellY++)
            {
                //This was two calls to ArrayFuncs.AccessArray1DAs2D(..), but the function calls were a big time hog
                int colIndex = (simCellX - 1) + (simCellY - 1)*gridSize;
                int denIndex = (simCellX) +(simCellY)*(gridSize + 2);
                col.r = density[denIndex];
                col.g = density[denIndex];
                col.b = density[denIndex];
                col.a = 1f;
                densColour[colIndex] = col;
            }
        drawTex.SetPixels(densColour.scaleArrayAs2D(scale).data);
        drawTex.Apply();

    }
    void drawVelocity(in PackedArray<float> velocityX, in PackedArray<float> velocityY, ref Texture2D drawTex, Color background, Color foreground)
    {
        int maxLength = (scale - 1) / 2;

        //Makes a texture of one colour
        velColour.data = Enumerable.Repeat(background, gridSize * scale * gridSize * scale).ToArray();
        drawTex.SetPixels(velColour.data);
        Vector2 normalised;
        Vector2 velocity;
        int xCoord, yCoord;
        for (int i = 1; i < N; i++)
        {
            for (int j = 1; j < N; j++)
            {
                velocity.x = velocityX[i, j];
                velocity.y = velocityY[i, j];

                normalised = velocity.normalized;

                xCoord = (i - 1) * scale + 2;
                yCoord = (j - 1) * scale + 2;
                line(ref velColour, xCoord, yCoord, (int)Math.Round((normalised * maxLength).x) + xCoord, (int)Math.Round((normalised * maxLength).y) + yCoord, foreground, gridSize * scale, gridSize * scale);
            }
        }
        drawTex.SetPixels(velColour.data);
        drawTex.Apply();
    }
    /* FIXME: Use ArrayFuncs.scaleArray1Das2D
     * - Leave bothtex as the big version
     * - make new colour array for bothtex and use scaling to copy to it.
     */
    public void drawDensityAndVelocity(in PackedArray<float> density, in PackedArray<float> velocityX, in PackedArray<float> velocityY, ref Texture2D bothTex, ref Texture2D densTex, Color foreground)
    {
        Color col;
        for (int simCellX = 1; simCellX <= gridSize; simCellX++) for (int simCellY = 1; simCellY <= gridSize; simCellY++)
            {
                //This was two calls to ArrayFuncs.AccessArray1DAs2D(..), but the function calls were a big time hog
                int colIndex = (simCellX - 1) + (simCellY - 1) * gridSize;
                int denIndex = (simCellX) + (simCellY) * (gridSize + 2);
                col.r = density[denIndex];
                col.g = density[denIndex];
                col.b = density[denIndex];
                col.a = 1f;
                densColour[colIndex] = col;
            }
        //GPUTextureScaler.Scale(bothTex, gridSize * scale, gridSize * scale, FilterMode.Point);
        bothColour = densColour.scaleArrayAs2D(scale);
        densTex.SetPixels(bothColour.data);
        
        //Makes a texture of one colour

        int maxLength = (scale - 1) / 2;

        Vector2 normalised;
        Vector2 velocity;
        int xCoord, yCoord;
        for (int i = 1; i < N; i++)
        {
            for (int j = 1; j < N; j++)
            {
                velocity.x = velocityX[i + j * N];
                velocity.y = velocityY[i + j * N];

                normalised = velocity.normalized;

                xCoord = (i - 1) * scale + 2;
                yCoord = (j - 1) * scale + 2;
                line(ref bothColour, xCoord, yCoord, (int)Math.Round((normalised * maxLength).x) + xCoord, (int)Math.Round((normalised * maxLength).y) + yCoord, foreground, gridSize * scale, gridSize * scale);

            }
        }
        bothTex.SetPixels(bothColour.data);
        bothTex.Apply();
    }
    public void line(ref PackedArray<Color> tex, int x, int y, int x2, int y2, Color color, int width, int height)
    {
        if (x > width || x2 > width) { return; }
        if (x < 0 || x2 < 0) { return; }
        if (y > height || y2 > height) {  return; }
        if (y < 0 || y2 < 0) { return; }

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
            tex[x + y * width] = color;
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
    
    public void runSimulationObjects()
    {
        foreach (SimulationObject obj in objects)
        {
            if (obj is VelocityForceField)
            {
                ((VelocityForceField)obj).tick(ref solver.getVelocityX(), ref solver.getVelocityY(), 1, 0);
            } else if (obj is DensityEnforcer)
            {
                ((DensityEnforcer)obj).tick(ref solver.getDensity(), 1);
            }
        }
    }
    public void drawSimulationObjects()
    {
        objectColour.data = Enumerable.Repeat(Color.black, gridSize * scale * gridSize * scale).ToArray();
        foreach (SimulationObject obj in objects)
        {
            //TODO: draw a coloured box for each object
            for (int x = obj.x; x < obj.width + obj.x; x++) for (int y = obj.y; y < obj.height + obj.y; y++)
                {
                    for (int scaleX = 0; scaleX < scale;  scaleX++) for (int scaleY = 0; scaleY < scale; scaleY++)
                        {
                            int idx = ((x * scale) + scaleX) + ((y * scale) + scaleY) * (gridSize * scale);
                            if (idx < objectColour.length)
                            {
                                objectColour[idx] = obj.debugColor;
                            }
                            
                        }
                }
        }

        objectTex.SetPixels(objectColour.data);
        objectTex.Apply();
    }

    /*IN-EDITOR DEBUG BUTTONS*/
    [Button("Print Density")]
    void printDensity() { Debug.Log(solver.getDensity().To2DString()); }
    [Button("Print Previous Density")]
    void printPrevDensity() { Debug.Log(solver.getDensityPrev().To2DString()); }
    [Button("Add New Simulation Object (Velocity Force Field)")]
    void addVelocityField(int x, int y, int w, int h)
    {
        objects.Add(new VelocityForceField(x,y,w,h,UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0f, 1f, 1f, 1f)));
    }
    [Button("Add New Simulation Object (Density Enforcer)")]
    void addDensityEnforcer(int x, int y, int w, int h)
    {
        objects.Add(new DensityEnforcer(x, y, w, h, UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0f, 1f, 1f, 1f)));
    }

}