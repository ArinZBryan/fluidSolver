using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;

class FluidSimulator2D21 : MonoBehaviour
{
    public int gridSize = 32;
    int N;
    public int scale = 5;
    public float viscosity = 0;
    public float diffusionRate = 0;
    public float deltaTime = 0.1f;

    public static Texture2D densTex;
    Color[] densColour;
    public static Texture2D velTex;
    public int penSize = 1;

    Solver2D2 solver;

    float mouseX = 0;
    float mouseY = 0;
    float mouseVelocityX = 0;
    float mouseVelocityY = 0;


    private void Start()
    {
        densTex = new Texture2D(gridSize * scale, gridSize * scale);
        velTex = new Texture2D(gridSize * scale, gridSize * scale);

        densTex.filterMode = FilterMode.Point;
        velTex.filterMode = FilterMode.Point;

        solver = new Solver2D2(gridSize, diffusionRate, viscosity, deltaTime);
        N = gridSize + 2;
        densColour = new Color[(gridSize) * scale * (gridSize) * scale];
    }

    private void Update()
    {
        //remap xy coords to be same as screen UV coords
        mouseX = Input.mousePosition.x;
        mouseY = Screen.height - Input.mousePosition.y;

        //clamp to area of simulation
        mouseX = Math.Clamp(mouseX, 0, gridSize * scale - 1);
        mouseY = Math.Clamp(mouseY, 0, gridSize * scale - 1);

        //get grid pos of cursor
        int cursorX = (int)(mouseX / scale);
        int cursorY = gridSize - (int)(mouseY / scale);

        //get mouse velocity
        mouseVelocityX = Input.GetAxis("Mouse X");
        mouseVelocityY = Input.GetAxis("Mouse Y");

        if (Input.GetKey(KeyCode.V))
        {
            if (Input.GetMouseButton(0)) //LMB
            {
                ArrayFuncs.paintTo1DArrayAs2D(ref solver.getVelocityX(), mouseVelocityX, cursorY, cursorX, gridSize, gridSize, penSize);
            }

            if (Input.GetMouseButton(1)) //RMB
            {
                ArrayFuncs.paintTo1DArrayAs2D(ref solver.getVelocityY(), mouseVelocityY, cursorY, cursorX, gridSize, gridSize, penSize);
            }

            solver.vel_step();
            solver.dens_step();
            

            drawVelocity(solver.getVelocityX(), solver.getVelocityY(), ref velTex);

            
        }
        else
        {
            if (Input.GetMouseButton(0)) //LMB
            {
                ArrayFuncs.paintTo1DArrayAs2D(ref solver.getDensityPrev(), 10000f, cursorY, cursorX, gridSize, gridSize, penSize);
            }

            if (Input.GetMouseButton(1)) //RMB
            {
                ArrayFuncs.paintTo1DArrayAs2D(ref solver.getDensityPrev(), -10000f, cursorY, cursorX, gridSize, gridSize, penSize);
            }



            solver.vel_step();
            solver.dens_step();
            

            drawDensity(solver.getDensity(), ref densTex);
            
        }
    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            if (Input.GetKey(KeyCode.V))
            {
                Graphics.DrawTexture(new Rect(0, 0, gridSize * scale, gridSize * scale), velTex);
            }
            else if (Input.GetKey(KeyCode.Y))
            {
                Graphics.DrawTexture(new Rect(0, 0, gridSize * scale, gridSize * scale), floatsToTexture(solver.getDensity()));
            }
            else
            {
                Graphics.DrawTexture(new Rect(0, 0, gridSize * scale, gridSize * scale), densTex);
            }
        }
    }

    void drawDensity(in float[] density, ref Texture2D drawTex)
    {
        //using modified implementation of ArrayFuncs.getSlice2DfromArray2D
        //Replacements are:
        // - 2D arrays -> 1D arrays
        // - 2D indicies -> ArrayFuncs.accessArray1DAs2D(i,j,w,h)

        int xStart, xEnd, yStart, yEnd;
        xStart = 1; yStart = 1;
        xEnd = gridSize + 1; yEnd = gridSize + 1; 

        int width = gridSize+2;
        int height = gridSize+2;
        for (int i = xStart; i < xEnd; i++) for (int j = yStart; j < yEnd-1; j++)
        {
            for (int scaleX = 0; scaleX < scale; scaleX++) for (int scaleY = 0; scaleY < scale; scaleY++)
            {
                float dens = density[ArrayFuncs.accessArray1DAs2D(i, j, gridSize + 2, gridSize + 2)];
                int iX = (i - xStart) * scale + scaleX;
                int iY = (i - yStart) * scale + scaleY;
                densColour[ArrayFuncs.accessArray1DAs2D(iX, iY, gridSize * scale, gridSize * scale)].r = dens;
                densColour[ArrayFuncs.accessArray1DAs2D(iX, iY, gridSize * scale, gridSize * scale)].g = dens;
                densColour[ArrayFuncs.accessArray1DAs2D(iX, iY, gridSize * scale, gridSize * scale)].b = dens;
                densColour[ArrayFuncs.accessArray1DAs2D(iX, iY, gridSize * scale, gridSize * scale)].a = 1f;
            }
                
        }
        drawTex.SetPixels(densColour);
        drawTex.Apply();
    
    }
    void drawVelocity(in float[] velocityX, in float[] velocityY, ref Texture2D drawTex)
    {
        Vector2 normalised;
        Vector2 velocity;
        int xCoord, yCoord;
        for (int i = 1; i < N; i++)
        {
            for (int j = 1; j < N; j++)
            {
                velocity.x = ArrayFuncs.accessArray1DAs2D(i, j, N, N, velocityX);
                velocity.y = ArrayFuncs.accessArray1DAs2D(i, j, N, N, velocityX);
                normalised = velocity.normalized;
                xCoord = (i - 1) * 5 + 2;
                yCoord = (j - 1) * 5 + 2;
                line(ref drawTex, xCoord, yCoord, (int)Math.Round((normalised * 2).x), (int)Math.Round((normalised * 2).y), Color.white);

            }
        }
        drawTex.Apply();
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
    Texture2D vector4sToTexture(Vector4[] vecs)
    {

        Color[] cols = new Color[vecs.Length];

        for (int i = 0; i < vecs.Length; i++)
        {
            cols[i].r = vecs[i].x;
            cols[i].g = vecs[i].y;
            cols[i].b = vecs[i].z;
            cols[i].a = vecs[i].w;
        }

        Texture2D tex = new Texture2D(32, 32);
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }
    Texture2D floatsToTexture(float[] vecs)
    {

        Color[] cols = new Color[vecs.Length];

        for (int i = 0; i < vecs.Length; i++)
        {
            cols[i].r = vecs[i];
            cols[i].g = vecs[i];
            cols[i].b = vecs[i];
            cols[i].a = 1f;
        }

        Texture2D tex = new Texture2D(32, 32);
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }
}