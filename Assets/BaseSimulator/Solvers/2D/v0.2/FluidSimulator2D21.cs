﻿using System;
using System.Linq;
using AdvancedEditorTools.Attributes;
using UnityEngine;

class FluidSimulator2D21 : MonoBehaviour
{
    public int gridSize = 32;
    int N;
    public int scale = 5;
    public float viscosity = 0;
    public float diffusionRate = 0;
    public float deltaTime = 0.1f;
    
    public float drawValue = 100f;
    public int penSize = 1;
    
    public static Texture2D densTex;
    Color[] densColour;
    public static Texture2D velTex;
    bool drawBoth = false;
    
    

    Solver2D2 solver;

    float mouseX = 0;
    float mouseY = 0;
    float mouseVelocityX = 0;
    float mouseVelocityY = 0;


    private void Start()
    {
        densTex = new Texture2D(gridSize, gridSize);
        velTex = new Texture2D(gridSize * scale, gridSize * scale);

        densTex.filterMode = FilterMode.Point;
        velTex.filterMode = FilterMode.Point;

        solver = new Solver2D2(gridSize, diffusionRate, viscosity, deltaTime);
        N = gridSize + 2;
        densColour = new Color[gridSize * gridSize];
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
        int cursorY = gridSize - (int)(mouseY / scale) - 1;

        //get mouse velocity
        mouseVelocityX = Input.GetAxis("Mouse X");
        mouseVelocityY = Input.GetAxis("Mouse Y");

        if (Input.GetKeyUp(KeyCode.T))
        {
            drawBoth = !drawBoth;
        }

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
            
            if (drawBoth)
            {
                drawDensity(solver.getDensity(), ref densTex);
                drawVelocity(solver.getVelocityX(), solver.getVelocityY(), ref velTex, Color.clear, Color.green);
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
                //Debug.Log(ArrayFuncs.printArray2DMatrix(ArrayFuncs.array1Dto2D(solver.getDensity(), gridSize+2, gridSize+2)));
                //Debug.Log(ArrayFuncs.paintTo1DArrayAs2D(ref solver.getDensityPrev(), 10000f, cursorY, cursorX, gridSize, gridSize, penSize));
            }
            if (Input.GetMouseButton(1)) //RMB
            {
                ArrayFuncs.edit1DArrayAs2D(ref solver.getDensityPrev(), -drawValue, cursorX, cursorY, gridSize + 2, gridSize + 2);
                //Debug.Log(ArrayFuncs.printArray2DMatrix(ArrayFuncs.array1Dto2D(solver.getDensity(), gridSize + 2, gridSize + 2)));
                //ArrayFuncs.paintTo1DArrayAs2D(ref solver.getDensityPrev(), -10000f, cursorY, cursorX, gridSize, gridSize, penSize);
            }

            solver.vel_step();
            solver.dens_step();

            if (Input.GetKey(KeyCode.Y)) drawDensity(solver.getDensityPrev(), ref densTex);
            else drawDensity(solver.getDensity(), ref densTex);

        }
    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            if (Input.GetKey(KeyCode.V))
            {
                if (drawBoth) { Graphics.DrawTexture(new Rect(0, 0, gridSize * scale, gridSize * scale), densTex); }
                Graphics.DrawTexture(new Rect(0, 0, gridSize * scale, gridSize * scale), velTex);
            }
            else
            {
                Graphics.DrawTexture(new Rect(0, 0, gridSize * scale, gridSize * scale), densTex);
            }
        }
    }

    void drawDensity(in float[] density, ref Texture2D drawTex)
    {
        for (int simCellX = 1; simCellX <= gridSize; simCellX++) for (int simCellY = 1; simCellY <= gridSize; simCellY++)
            {
                int colIndex = ArrayFuncs.accessArray1DAs2D(simCellX - 1, simCellY - 1, gridSize, gridSize);
                int denIndex = ArrayFuncs.accessArray1DAs2D(simCellX, simCellY, gridSize + 2, gridSize + 2);
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
    void printDensity() { Debug.Log(ArrayFuncs.printArray2DMatrix<float>(ArrayFuncs.array1Dto2D(solver.getDensity(), gridSize + 2, gridSize + 2))); }
    [Button("Print Previous Density")]
    void printPrevDensity() { Debug.Log(ArrayFuncs.printArray2DMatrix<float>(ArrayFuncs.array1Dto2D(solver.getDensityPrev(), gridSize + 2, gridSize + 2))); }
}