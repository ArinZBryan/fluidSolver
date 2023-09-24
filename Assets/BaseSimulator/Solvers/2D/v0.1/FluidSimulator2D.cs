using System;
using UnityEngine;

public class FluidSimulator2D : MonoBehaviour
{
    public int texWidth = 32;
    public int texHeight = 32;

    public Color penColor;
    public Color baseColor;
    Vector4 penVector;
    Vector4 baseVector;

    public int penSize = 1;

    public static Texture2D drawTex;
    public static Vector4[,] drawVecs;

    int mouseX;
    int mouseY;

    public float drawModifier = 1.0f;
    public float viscosity;
    public float diffusionRate;
    public float deltaTime;
    Solver2D solver;


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

        drawVecs = new Vector4[texWidth, texHeight];
        penVector = (Vector4)penColor;
        baseVector = (Vector4)baseColor;
        for (int i = 0; i < texWidth; i++) for (int j = 0; j < texHeight; j++) drawVecs[i, j] = baseVector;

        solver = new Solver2D(texWidth, diffusionRate, viscosity, deltaTime);

    }

    // Update is called once per frame
    void Update()
    {
        mouseX = (int)Input.mousePosition.x;
        mouseY = (int)Input.mousePosition.y - (Screen.height - texWidth);
        
        mouseX = Math.Clamp(mouseX, 0, texWidth - 1);
        mouseY = Math.Clamp(mouseY, 0, texHeight - 1);

        if (Input.GetMouseButton(0))
        {
            vector4Paint(ref drawVecs, penVector, mouseX, mouseY, penSize);
        }

        if (Input.GetMouseButton(1))
        {
            vector4Paint(ref drawVecs, baseVector, mouseX, mouseY, penSize);
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

    Texture2D vector4sToTexture(Vector4[,] vecs)
    {
        int vecWidth = vecs.GetLength(0);
        int vecHeight = vecs.GetLength(1);

        Color[] cols = new Color[vecWidth * vecHeight];

        for (int i = 0; i < vecWidth; i++)
        {
            for (int j = 0; j < vecHeight; j++)
            {
                cols[i + j * vecWidth].r = vecs[i, j].x;
                cols[i + j * vecWidth].g = vecs[i, j].y;
                cols[i + j * vecWidth].b = vecs[i, j].z;
                cols[i + j * vecWidth].a = vecs[i, j].w;
            }
        }

        Texture2D tex = new Texture2D(vecWidth, vecHeight);
        tex.SetPixels(cols);
        tex.Apply();
        return tex;
    }
    void vector4Paint(ref Vector4[,] vec, Vector4 value, int x, int y, int brushSize)
    {
        int vecWidth = vec.GetLength(0);
        int vecHeight = vec.GetLength(1);
        for (int i = x - (brushSize - 1); i < x + brushSize; i++)
        {
            if (i >= vecWidth || i < 0) { continue; }
            for (int j = y - (brushSize - 1); j < y + brushSize; j++)
            {
                if (j >= vecHeight || j < 0) { continue; }
                vec[i, j] = value * drawModifier;
            }
        }
    }
    void solverDensityToVecs()
    {
        float[,] density = solver.getDensity();
        for (int i = 0; i < texWidth; i++)
        {
            for (int j = 0; j < texHeight; j++)
            {
                drawVecs[i, j].Set(density[i, j], density[i, j], density[i, j], 1f);
            }
        }
    }
    void vecsToSolverDensity()
    {
        for (int i = 0;i < texWidth; i++)
            for (int j = 0;j < texHeight; j++)
            {
                solver.density[i, j] = drawVecs[i, j].x;
            }
    }

}
