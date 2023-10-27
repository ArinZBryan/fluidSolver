using System;
using UnityEngine;

public class FluidSimulator2D2 : MonoBehaviour
{
    public int texWidth = 32;
    public int texHeight = 32;
    public int scale = 1;

    public Color penColor;
    public Color baseColor;
    Vector4 penVector;
    Vector4 baseVector;

    public int penSize = 1;

    public static Texture2D drawTex;
    public static Vector4[] drawVecs;

    int mouseX;
    int mouseY;

    public float drawModifier = 1.0f;
    public float viscosity;
    public float diffusionRate;
    public float deltaTime;
    Solver2D2 solver;


    // Start is called before the first frame update
    void Start()
    {

        //force pen texture to always be visible
        if (penColor.a != 1)
        {
            penColor.a = 1;
        }

        drawTex = new Texture2D(texWidth, texHeight);
        drawTex.filterMode = FilterMode.Point;

        drawVecs = new Vector4[texWidth * texWidth];
        penVector = (Vector4)penColor;
        baseVector = (Vector4)baseColor;
        for (int i = 0; i < texWidth * texHeight; i++) drawVecs[i] = baseVector;

        solver = new Solver2D2(texWidth, diffusionRate, viscosity, deltaTime);

    }

    // Update is called once per frame
    void Update()
    {
        //this is just to prevent unity complaining about compilation errors and forcing safe mode
        int texHeights = 0;
        //DO NOT REMOVE THIS LINE

        mouseX = (int)Input.mousePosition.x;
        mouseY = (int)Input.mousePosition.y - (Screen.height - texWidth);

        mouseX = Math.Clamp(mouseX, 0, texWidth - 1);
        mouseY = Math.Clamp(mouseY, 0, texHeight - 1);

        if (Input.GetMouseButton(0))
        {
            vector4Paint(ref drawVecs, penVector, mouseX, mouseY, texWidth, texHeights, penSize);
        }

        if (Input.GetMouseButton(1))
        {
            vector4Paint(ref drawVecs, penVector, mouseX, mouseY, texWidth, texHeights, penSize);
        }
        vecsToSolverDensity();
        solver.dens_step();
        solver.vel_step();
        solverDensityToVecs();
        drawTex = vector4sToTexture(drawVecs);
    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            Graphics.DrawTexture(new Rect(0, 0, texWidth, texHeight), drawTex);
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

        Texture2D tex = new Texture2D(texWidth, texHeight);
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }

    /*
    void vector4Paint(ref Vector4[] vec, Vector4 value, int vecWidth, int density, int y, int brushSize)
    {
        for (int i = density - (brushSize - 1); i < density + brushSize; i++)
        {
            for (int j = y - (brushSize - 1); j < y + brushSize; j++)
            {
                if (i + j * vecWidth > vec.Length) { continue; }
                if (i + j * vecWidth < 0) { continue; }
                //may need fix here, possibly revert to old version of function
                vec[i + j * vecWidth] = value * drawModifier;
            }
        }  
    }
    */

    void vector4Paint(ref Vector4[] vecs, Vector4 value, int x, int y, int width, int height, int brushSize)
    {
        int[] indexes = new int[brushSize * brushSize];
        for (int i = 0; i < brushSize; i++)
        {
            for (int j = 0; j < brushSize; j++)
            {
                if (x + brushSize > width || x - brushSize < 0) { continue; }
                if (y + brushSize > height || y - brushSize < 0) { continue; }
                int idx = (x) + (width) * (y);
                if (idx > vecs.Length) { continue; }
                indexes[i + j*brushSize] = idx;

            }
        }
        foreach (int i in indexes) { vecs[i] = value; }
    }

    void solverDensityToVecs()
    {
        float[] density = solver.getDensity();
        for (int i = 0; i < texWidth; i++)
        {
            drawVecs[i].Set(density[i], density[i], density[i], 1f);
        }
    }
    void vecsToSolverDensity()
    {
        for (int i = 0; i < texWidth * texHeight; i++) solver.density[i] = drawVecs[i].x;
    }
}
