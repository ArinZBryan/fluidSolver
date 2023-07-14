using FluidSolver;
using UnityEngine;

public class simulator : MonoBehaviour
{
    public int N = 64;
    public int stepsPerSecond = 50;
    public float diffusionRate = 0.0001f;
    public float viscosity = 0.0001f;

    FluidSolver.Solver solver;
    Vector2[,] velocityField;
    float[,] densityField;

    public int penSize = 20;
    int mouseX;
    int mouseY;

    // Start is called before the first frame update
    void Start()
    {
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

    }
}
