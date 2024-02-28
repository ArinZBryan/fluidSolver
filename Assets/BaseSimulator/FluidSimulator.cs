using System;
using System.Collections.Generic;
using System.Linq;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.UIElements;
using static UserInput;

public class FluidSimulator : MonoBehaviour
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

    public List<SimulationObject> simulationObjects = new List<SimulationObject>();
    public Texture2D objectTex;
    PackedArray<Color> objectColour;

    public Texture2D densTex;
    PackedArray<Color> densColour;
    public Texture2D velTex;
    PackedArray<Color> velColour;

    bool drawBoth = true;
    public Texture2D bothTex;
    PackedArray<Color> bothColour;
    public RenderTexture renderTexture;
    public RectTransform viewport;
    public Solver2D solver;

    public bool drawVelocityField = false;
    public bool drawObjectField = false;


    float mouseX = 0;
    float mouseY = 0;
    float mouseVelocityX = 0;
    float mouseVelocityY = 0;

    public void initFromKeyframe(KeyFrame k)
    {
        gridSize = k.N;
        deltaTime = k.sim_delta_time;
        init();
        solver.density = k.density;
        solver.velocity_horizontal = k.velocity_horizontal;
        solver.velocity_vertical = k.velocity_vertical;
        solver.prev_density = k.prev_density;
        solver.prev_velocity_horizontal = k.prev_velocity_horizontal;
        solver.prev_velocity_vertical = k.prev_velocity_vertical;
    }
    

    public void init()
    {
        densTex = new Texture2D(gridSize * scale, gridSize * scale);
        velTex = new Texture2D(gridSize * scale, gridSize * scale);
        bothTex = new Texture2D(gridSize * scale, gridSize * scale);
        objectTex = new Texture2D(gridSize * scale, gridSize * scale);
        renderTexture = new RenderTexture(gridSize * scale, gridSize * scale, 0);

        densTex.filterMode = FilterMode.Point;
        velTex.filterMode = FilterMode.Point;

        solver = new Solver2D(gridSize, diffusionRate, viscosity, deltaTime, false);
        N = gridSize + 2;
        densColour = new PackedArray<Color>(new int[] { gridSize, gridSize });
        velColour = new PackedArray<Color>(new int[] { gridSize * scale, gridSize * scale });
        bothColour = new PackedArray<Color>(new int[] { gridSize * gridSize, gridSize * gridSize });
    }

    public Texture2D computeNextTexture(List<UserInput> userInputs) 
    {

        runSimulationObjects();

        foreach (UserInput i in userInputs)
        {
            if (i.field == fieldToWriteTo.VELX)
            {
                solver.getVelocityX()[i.x, i.y] = i.value;
            }
            else if (i.field == fieldToWriteTo.VELY)
            {
                solver.getVelocityY()[i.x, i.y] = i.value;
            }
            else if (i.field == fieldToWriteTo.DENS)
            {
                solver.getDensity()[i.x, i.y] = i.value;
            }
        }

        solver.vel_step();
        solver.dens_step();

        //zeros out prev_density - This prevents runaway densities
        solver.getDensityPrev().data = Enumerable.Repeat(0f, (gridSize + 2) * (gridSize + 2)).ToArray();


        if (Input.GetKey(KeyCode.O) || drawObjectField)
        {
            drawSimulationObjects();
        } 
        else if (Input.GetKey(KeyCode.V) || drawVelocityField)
        {   
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
            drawDensity(solver.getDensity(), ref densTex);
        }
        return getCurrentTexture();
    }
    public Texture2D getCurrentTexture()
    {
        if (Input.GetKey(KeyCode.O) || drawObjectField)
        {
            return objectTex;
        }
        else if (Input.GetKey(KeyCode.V) || drawVelocityField)
        {
            if (drawBoth) return bothTex;
            else return velTex;
        }
        else
        {
            return densTex;
        }
    }
    public Texture2D getGurrentExportableTexture()
    {
        return densTex;
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
        foreach (SimulationObject obj in simulationObjects)
        {
            if (obj is VelocityForceField)
            {
                ((VelocityForceField)obj).tick(ref solver.getVelocityX(), ref solver.getVelocityY());
            } else if (obj is DensityEnforcer)
            {
                ((DensityEnforcer)obj).tick(ref solver.getDensity(), 1);
            } else if (obj is PhysPoint)
            {
                ((PhysPoint)obj).tick(ref solver.getVelocityX(), ref solver.getVelocityY(), deltaTime);
            }
        }
    }
    public void drawSimulationObjects()
    {
        objectColour.data = Enumerable.Repeat(Color.black, gridSize * scale * gridSize * scale).ToArray();
        foreach (SimulationObject obj in simulationObjects)
        {
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
    void addVelocityField(int x, int y, int w, int h,float valX, float valY)
    {
        if (w < 2 || h < 2) { Debug.LogError("Cannot create new collidable cell: Dimensions must be greater than 1"); return; }
        if (x < 0 || y < 0) { Debug.LogError("Cannot create new collidable cell: Position not in simulation bounds"); return; }
        if (x + w > gridSize || y + h > gridSize) { Debug.LogError("Cannot create new collidable cell: Dimensions cause effects outside of simulation range"); return; }
        simulationObjects.Add(new VelocityForceField(x,y,w,h,valX,valY,UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0f, 1f, 1f, 1f)));
    }
    [Button("Add New Simulation Object (Density Enforcer)")]
    void addDensityEnforcer(int x, int y, int w, int h)
    {
        if (w < 2 || h < 2) { Debug.LogError("Cannot create new collidable cell: Dimensions must be greater than 1"); return; }
        if (x < 0 || y < 0) { Debug.LogError("Cannot create new collidable cell: Position not in simulation bounds"); return; }
        if (x + w > gridSize || y + h > gridSize) { Debug.LogError("Cannot create new collidable cell: Dimensions cause effects outside of simulation range"); return; }
        simulationObjects.Add(new DensityEnforcer(x, y, w, h, UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0f, 1f, 1f, 1f)));
    }
    [Button("Add New Simulation Object (Physics Particle)")]
    void addPhysParticle(int x, int y)
    {
        if (x < 0 || y < 0) { Debug.LogError("Cannot create new collidable cell: Position not in simulation bounds"); return; }
        simulationObjects.Add(new PhysPoint(x, y, UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0f, 1f, 1f, 1f)));
    }
    [Button("Add New Simulation Object (Collidable Cell")]
    void addCollidableCell(int x, int y, int w, int h)
    {
        if (w < 2 || h < 2) { Debug.LogError("Cannot create new collidable cell: Dimensions must be greater than 1"); return; }
        if (x < 0 || y < 0) { Debug.LogError("Cannot create new collidable cell: Position not in simulation bounds"); return; }
        if (x + w > gridSize || y + h > gridSize) { Debug.LogError("Cannot create new collidable cell: Dimensions cause effects outside of simulation range"); return; }
        CollidableCell c = new CollidableCell(x, y, w, h, 
                                    Solver2D.Boundary.TOP | 
                                    Solver2D.Boundary.BOTTOM | 
                                    Solver2D.Boundary.LEFT | 
                                    Solver2D.Boundary.RIGHT, 
                                    UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0f, 1f, 1f, 1f));
        simulationObjects.Add(c);
        solver.addPhysicsObject(c);
    }
    [Button("Print total volume")]
    void printSimVolume()
    {
        float volume = solver.getDensity().data.Sum();
        Debug.Log(volume * gridSize * gridSize);
    }
}