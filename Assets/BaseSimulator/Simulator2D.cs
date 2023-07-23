using FluidSolver;
using Unity.VisualScripting;
using UnityEngine;

public class Simulator2D : MonoBehaviour
{
    public int N = 64;
    public int stepsPerSecond = 50;
    public float diffusionRate = 0.0001f;
    public float viscosity = 0.0001f;
    public bool showDensity = true;
    public bool showVelocity = false;

    FluidSolver.Solver solver;
    Vector2[,] velocityField;
    float[,] densityField;

    public int penSize = 20;
    int mouseX;
    int mouseY;

    // Start is called before the first frame update
    void Start()
    {
        velocityField = new Vector2[N + 2, N + 2];
        densityField = new float[N + 2, N + 2];
        Time.fixedDeltaTime = 1 / stepsPerSecond;
        solver = new Solver(N, 1 / stepsPerSecond, diffusionRate, viscosity);
    }

    void FixedUpdate()
    {
        mouseX = (int)Input.mousePosition.x;
        mouseY = (int)Input.mousePosition.y;
        if (Input.GetMouseButton(0))
        {
            for (int i = 0; i < penSize; i++)
            {
                for (int j = 0; j < penSize; j++)
                {
                    velocityField[mouseX + i, mouseY + j] = new Vector2(10, 0);
                }
            }

        }
        if (Input.GetMouseButton(1))
        {
            for (int i = 0; i < penSize; i++)
            {
                for (int j = 0; j < penSize; j++)
                {
                    densityField[mouseX + i, mouseY + j] += 0.01f;
                }
            }
        }

        solver.step(velocityField, densityField);
        
    }

    void OnGUI()
    {
        Texture2D tex = densityTexture(solver.currentState);
        Graphics.DrawTexture(new Rect(0, 0, N * 10, N * 10), tex);
    }

    Texture2D densityTexture(Solver.State solveState)
    {
        Texture2D tex = new Texture2D(N, N);
        tex.filterMode = FilterMode.Point;
        Color[] colors = new Color[N * N];

        for (int i = 1; i < N; i++)
        {
            for (int j = 1; j < N; j++)
            {
                colors[i * N + j] = new Color(solveState.density[i, j], solveState.density[i, j], solveState.density[i, j], 1f);
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }
}
