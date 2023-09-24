/*
    Optimisations:
    - [ ] Replace two arrays, with one array of vector2. This may allow for SIMD instructions to be used
    - [ ] unsafe {} 
        In this case, swap for loops with pointer increments and simpler bounds checks.
        void SWAP<T>(ref T a, ref T b) -> void SWAP(void* a, void* b) or void SWAP<T>(T* a, T* b) 
*/

using System;

class Solver2D
{
    //Vector Fields
    public float[,] velocity_horizontal;
    public float[,] velocity_vertical;
    public float[,] velocity_horizontal_prev;
    public float[,] velocity_vertical_prev;
    public float[,] density;
    public float[,] density_prev;

    //Constants
    int N;
    float diffusionRate, viscosity, deltaTime;


    public Solver2D(int N, float diffusionRate, float viscosity, float deltaTime)
    {
        velocity_horizontal = new float[N + 2, N + 2];
        velocity_vertical = new float[N + 2, N + 2];
        velocity_horizontal_prev = new float[N + 2, N + 2];
        velocity_vertical_prev = new float[N + 2, N + 2];
        density = new float[N + 2, N + 2];
        density_prev = new float[N + 2, N + 2];

        this.diffusionRate = diffusionRate;
        this.viscosity = viscosity;
        this.deltaTime = deltaTime;
        this.N = N;
    }

    enum Boundary
    {
        NONE, HORIZONTAL, VERTICAL
    }
    void SWAP<T>(ref T a, ref T b) { T temp = a; a = b; b = temp; }

    //Add Source to velocity field
    void add_source(ref float[,] destination, ref float[,] source)
    {
        int i, j = 0;
        
        //for (i = 0; i < (N + 2); i++) for (j = 0; j < (N + 2); j++) { destination[i, j] += deltaTime * source[i, j]; }
    }
    //Enforce boundary conditions for vector fields
    void set_bnd(Boundary boundaryType, ref float[,] valueField)
    {
        int i;

        for (i = 1; i <= N; i++)
        {
            valueField[0, i] = (boundaryType == Boundary.HORIZONTAL) ? -valueField[1, i] : valueField[1, i];
            valueField[N + 1, i] = (boundaryType == Boundary.HORIZONTAL) ? -valueField[N, i] : valueField[N, i];
            valueField[i, 0] = (boundaryType == Boundary.VERTICAL) ? -valueField[i, 1] : valueField[i, 1];
            valueField[i, N + 1] = (boundaryType == Boundary.VERTICAL) ? -valueField[i, N] : valueField[i, N];
        }
        valueField[0, 0] = 0.5f * (valueField[1, 0] + valueField[0, 1]);
        valueField[0, N + 1] = 0.5f * (valueField[1, N + 1] + valueField[0, N]);
        valueField[N + 1, 0] = 0.5f * (valueField[N, 0] + valueField[N + 1, 1]);
        valueField[N + 1, N + 1] = 0.5f * (valueField[N, N + 1] + valueField[N + 1, N]);
    }
    //Solving of set of linear equations
    void lin_solve(Boundary boundaryType, ref float[,] valueField, ref float[,] valueField_prev, float a, float c)
    {
        int i, j, k;

        for (k = 0; k < 20; k++)
        {
            for (i = 1; i <= N; i++) for (j = 1; j <= N; j++)
                {
                    valueField[i, j] = (valueField_prev[i, j] + a * (valueField[i - 1, j] + valueField[i + 1, j] + valueField[i, j - 1] + valueField[i, j + 1])) / c;
                }
            set_bnd(boundaryType, ref valueField);
        }
    }
    //Diffuse vector fields
    void diffuse(Boundary boundaryType, ref float[,] valueField, ref float[,] valueField_prev, float diffusion)
    {
        float a = deltaTime * diffusion * N * N; //Constant term used for system stability and optimisation from the original paper
        lin_solve(boundaryType, ref valueField, ref valueField_prev, a, 1 + 4 * a);
    }
    //Advect vector fields
    void advect(Boundary boundaryType)
    {
        int i, j, i0, j0, i1, j1;
        float x, y, s0, t0, s1, t1, deltaTime0;

        deltaTime0 = deltaTime * N;
        for (i = 1; i <= N; i++) for (j = 1; j <= N; j++)
            {

                x = i - deltaTime0 * velocity_horizontal[i, j];
                y = j - deltaTime0 * velocity_vertical[i, j];

                if (x < 0.5f) x = 0.5f;
                if (x > N + 0.5f) x = N + 0.5f;
                i0 = (int)x;
                i1 = i0 + 1;

                if (y < 0.5f) y = 0.5f;
                if (y > N + 0.5f) y = N + 0.5f;
                j0 = (int)y;
                j1 = j0 + 1;

                s1 = x - i0;
                s0 = 1 - s1;
                t1 = y - j0;
                t0 = 1 - t1;

                density[i, j] = s0 * (t0 * density_prev[i0, j0] + t1 * density_prev[i0, j1]) + s1 * (t0 * density_prev[i1, j0] + t1 * density_prev[i1, j1]);
            }
        set_bnd(boundaryType, ref density);
    }
    //Maintain mass conservation
    void project()
    {
        int i, j;

        for (i = 1; i <= N; i++) for (j = 1; j <= N; j++)
            {
                velocity_vertical_prev[i, j] = -0.5f * (velocity_horizontal[i + 1, j] - velocity_horizontal[i - 1, j] + velocity_vertical[i, j + 1] - velocity_vertical[i, j - 1]) / N;
                velocity_horizontal_prev[i, j] = 0;
            }
        set_bnd(Boundary.NONE, ref velocity_vertical_prev);
        set_bnd(Boundary.NONE, ref velocity_horizontal_prev);

        lin_solve(Boundary.NONE, ref velocity_horizontal_prev, ref velocity_vertical_prev, 1, 4);

        for (i = 1; i <= N; i++) for (j = 1; j <= N; j++)
            {
                velocity_horizontal[i, j] -= 0.5f * N * (velocity_horizontal_prev[i + 1, j] - velocity_horizontal_prev[i - 1, j]);
                velocity_vertical[i, j] -= 0.5f * N * (velocity_horizontal_prev[i, j + 1] - velocity_horizontal_prev[i, j - 1]);
            }

        set_bnd(Boundary.HORIZONTAL, ref velocity_horizontal);
        set_bnd(Boundary.VERTICAL, ref velocity_vertical);
    }
    public void dens_step()
    {
        add_source(ref density, ref density_prev);
        SWAP(ref density_prev, ref density); diffuse(Boundary.NONE, ref density, ref density_prev, diffusionRate);
        SWAP(ref density_prev, ref density); advect(Boundary.NONE);
    }
    public void vel_step()
    {
        add_source(ref velocity_horizontal, ref velocity_horizontal_prev);
        add_source(ref velocity_vertical, ref velocity_vertical_prev);

        SWAP(ref velocity_horizontal_prev, ref velocity_horizontal);
        diffuse(Boundary.HORIZONTAL, ref velocity_horizontal, ref velocity_horizontal_prev, viscosity);

        SWAP(ref velocity_vertical_prev, ref velocity_vertical);
        diffuse(Boundary.VERTICAL, ref velocity_vertical, ref velocity_vertical_prev, viscosity);

        project();

        SWAP(ref velocity_horizontal_prev, ref velocity_horizontal);
        SWAP(ref velocity_vertical_prev, ref velocity_vertical);

        advect(Boundary.HORIZONTAL);
        advect(Boundary.VERTICAL);

        project();
    }


    public ref float[,] getDensity()
    {
        return ref density;
    }
    public ref float[,] getDensityPrev()
    {
        return ref density_prev;
    }
    public ref float[,] getVelocityX()
    {
        return ref velocity_horizontal;
    }
    public ref float[,] getVelocityY()
    {
        return ref velocity_vertical;
    }
    public ref float[,] getVelocityXPrev()
    {
        return ref velocity_horizontal_prev;
    }
    public ref float[,] getVelocityYPrev()
    {
        return ref velocity_vertical_prev;
    }
    public void getAll (out float[,] density, out float[,] density_prev, out float[,] velocity_horizontal, out float[,] velocity_horizontal_prev, out float[,] velocity_vertical, out float[,] velocity_vertical_prev, out int N)
    {
        density = this.density;
        density_prev = this.density_prev;
        velocity_horizontal = this.velocity_horizontal;
        velocity_vertical = this.velocity_vertical;
        velocity_horizontal_prev = this.velocity_horizontal_prev;
        velocity_vertical_prev = this.velocity_vertical_prev;
        N = this.N;
    }

}
