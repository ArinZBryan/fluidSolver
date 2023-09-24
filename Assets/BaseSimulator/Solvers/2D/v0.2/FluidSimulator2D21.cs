using System;
using UnityEngine;

class FluidSimulator2D21 : MonoBehaviour
{
    public int gridSize = 32;
    public int scale = 5;
    public float viscosity = 0;
    public float diffusionRate = 0;
    public float deltaTime = 0.01f;

    public static Texture2D densTex;
    public static Texture2D velTex;
    public int penSize = 1;

    Solver2D2 solver;

    int mouseX = 0;
    int mouseY = 0;

    private void Start()
    {
        densTex = new Texture2D(gridSize * scale, gridSize * scale);
        velTex = new Texture2D(gridSize * scale, gridSize * scale);

        densTex.filterMode = FilterMode.Point;
        velTex.filterMode = FilterMode.Point;

        solver = new Solver2D2(gridSize, diffusionRate, viscosity, deltaTime);
    }

    private void Update()
    {
        //remap xy coords to be same as screen UV coords
        mouseX = (int)Input.mousePosition.x;
        mouseY = (int)Input.mousePosition.y - Screen.height;

        //clamp to area of simulation
        mouseX = Math.Clamp(mouseX, 0, gridSize * scale -1);
        mouseY = Math.Clamp(mouseY, 0, gridSize * scale -1);

        //get grid pos of cursor
        int cursorX = mouseX / scale;
        int cursorY = mouseY / scale;

        if (Input.GetKey(KeyCode.V))
        {
            if (Input.GetMouseButton(0)) //LMB
            {
                // paint to velocity
            }

            if (Input.GetMouseButton(1)) //RMB
            {
                // paint to velocity
            }

            solver.dens_step();
            solver.vel_step();

            //Draw Velocityies

        } else
        {
            if (Input.GetMouseButton(0)) //LMB
            {
                // paint to density
            }

            if (Input.GetMouseButton(1)) //RMB
            {
                // paint to density
            }

            solver.dens_step();
            solver.vel_step();
        
            //Draw Densities
        }


    }

    public void line(int x, int y, int x2, int y2, int color)
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
            putpixel(x, y, color);
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


}