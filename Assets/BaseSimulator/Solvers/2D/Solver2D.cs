using System;
using System.Diagnostics;

class Solver2D
{
    //Vector Fields
    public PackedArray<float> velocity_horizontal;
    public PackedArray<float> velocity_vertical;
    public PackedArray<float> prev_velocity_horizontal;
    public PackedArray<float> prev_velocity_vertical;
    public PackedArray<float> density;
    public PackedArray<float> prev_density;

    //Constants
    int N;
    float diffusion_rate, viscosity, sim_delta_time;


    public Solver2D(int N, float diffusionRate, float viscosity, float deltaTime)
    {
        int size = (N + 2) * (N + 2);

        velocity_horizontal = new PackedArray<float>(new int[] { N+2, N+2 });               //Velocity (Horizontal)
        velocity_vertical = new PackedArray<float>(new int[] { N + 2, N + 2 });             //Velocity (Vertical)
        prev_velocity_horizontal = new PackedArray<float>(new int[] { N + 2, N + 2 });      //Previous Velocity (Horizontal)
        prev_velocity_vertical = new PackedArray<float>(new int[] { N + 2, N + 2 });        //Previous Velocity (Vertical)
        density = new PackedArray<float>(new int[] { N + 2, N + 2 });                       //Density
        prev_density = new PackedArray<float>(new int[] { N + 2, N + 2 });                  //Previous Density

        this.diffusion_rate = diffusionRate;
        this.viscosity = viscosity;
        this.sim_delta_time = deltaTime;
        this.N = N;
    }

    enum Boundary
    {
        NONE, HORIZONTAL, VERTICAL
    }
    void SWAP<T>(ref T a, ref T b) { T temp = a; a = b; b = temp; }

    void add_source(int N, ref PackedArray<float> x, ref PackedArray<float> s, float dt)
    {
        int i, size = (N + 2) * (N + 2);
        for (i = 0; i < size; i++)
        {
            x[i] += dt * s[i];
        }
    }

    void set_bnd(int N, Boundary b, ref PackedArray<float> x)
    {
        int i;

        for (i = 1; i <= N; i++)
        {
            x[((0) + (N + 2) * (i))] = b == Boundary.HORIZONTAL ? -x[((1) + (N + 2) * (i))] : x[((1) + (N + 2) * (i))];
            x[((N + 1) + (N + 2) * (i))] = b == Boundary.HORIZONTAL ? -x[((N) + (N + 2) * (i))] : x[((N) + (N + 2) * (i))];
            x[((i) + (N + 2) * (0))] = b == Boundary.VERTICAL ? -x[((i) + (N + 2) * (1))] : x[((i) + (N + 2) * (1))];
            x[((i) + (N + 2) * (N + 1))] = b == Boundary.VERTICAL ? -x[((i) + (N + 2) * (N))] : x[((i) + (N + 2) * (N))];
        }
        x[((0) + (N + 2) * (0))] = 0.5f * (x[((1) + (N + 2) * (0))] + x[((0) + (N + 2) * (1))]);
        x[((0) + (N + 2) * (N + 1))] = 0.5f * (x[((1) + (N + 2) * (N + 1))] + x[((0) + (N + 2) * (N))]);
        x[((N + 1) + (N + 2) * (0))] = 0.5f * (x[((N) + (N + 2) * (0))] + x[((N + 1) + (N + 2) * (1))]);
        x[((N + 1) + (N + 2) * (N + 1))] = 0.5f * (x[((N) + (N + 2) * (N + 1))] + x[((N + 1) + (N + 2) * (N))]);
    }

    void lin_solve(int N, Boundary b, ref PackedArray<float> x, ref PackedArray<float> x0, float a, float c)
    {
        int i, j, k;

        for (k = 0; k < 20; k++)
        {
            for (i = 1; i <= N; i++)
            {
                for (j = 1; j <= N; j++)
                {
                    x[((i) + (N + 2) * (j))] = (x0[((i) + (N + 2) * (j))] + a * (x[((i - 1) + (N + 2) * (j))] + x[((i + 1) + (N + 2) * (j))] + x[((i) + (N + 2) * (j - 1))] + x[((i) + (N + 2) * (j + 1))])) / c;
                }
            }

            set_bnd(N, b, ref x);
        }
    }

    void diffuse(int N, Boundary b, ref PackedArray<float> x, ref PackedArray<float> x0, float diff, float dt)
    {
        float a = dt * diff * N * N;
        lin_solve(N, b, ref x, ref x0, a, 1 + 4 * a);
    }

    void advect(int N, Boundary b, ref PackedArray<float> d, ref PackedArray<float> d0, ref PackedArray<float> u, ref PackedArray<float> v, float dt)
    {
        int i, j, i0, j0, i1, j1;
        float x, y, s0, t0, s1, t1, dt0;

        dt0 = dt * N;
        for (i = 1; i <= N; i++)
        {
            for (j = 1; j <= N; j++)
            {
                x = i - dt0 * u[((i) + (N + 2) * (j))]; y = j - dt0 * v[((i) + (N + 2) * (j))];
                if (x < 0.5f) x = 0.5f; if (x > N + 0.5f) x = N + 0.5f; i0 = (int)x; i1 = i0 + 1;
                if (y < 0.5f) y = 0.5f; if (y > N + 0.5f) y = N + 0.5f; j0 = (int)y; j1 = j0 + 1;
                s1 = x - i0; s0 = 1 - s1; t1 = y - j0; t0 = 1 - t1;
                d[((i) + (N + 2) * (j))] = s0 * (t0 * d0[((i0) + (N + 2) * (j0))] + t1 * d0[((i0) + (N + 2) * (j1))]) +
                    s1 * (t0 * d0[((i1) + (N + 2) * (j0))] + t1 * d0[((i1) + (N + 2) * (j1))]);
            }
        }

        set_bnd(N, b, ref d);
    }

    void project(int N, ref PackedArray<float> u, ref PackedArray<float> v, ref PackedArray<float> p, ref PackedArray<float> div)
    {
        int i, j;

        for (i = 1; i <= N; i++)
        {
            for (j = 1; j <= N; j++)
            {
                div[((i) + (N + 2) * (j))] = -0.5f * (u[((i + 1) + (N + 2) * (j))] - u[((i - 1) + (N + 2) * (j))] + v[((i) + (N + 2) * (j + 1))] - v[((i) + (N + 2) * (j - 1))]) / N;
                p[((i) + (N + 2) * (j))] = 0;
            }
        }

        set_bnd(N, 0, ref div); set_bnd(N, 0, ref p);

        lin_solve(N, 0, ref p, ref div, 1, 4);

        for (i = 1; i <= N; i++)
        {
            for (j = 1; j <= N; j++)
            {
                u[((i) + (N + 2) * (j))] -= 0.5f * N * (p[((i + 1) + (N + 2) * (j))] - p[((i - 1) + (N + 2) * (j))]);
                v[((i) + (N + 2) * (j))] -= 0.5f * N * (p[((i) + (N + 2) * (j + 1))] - p[((i) + (N + 2) * (j - 1))]);
            }
        }

        set_bnd(N, Boundary.HORIZONTAL, ref u); set_bnd(N, Boundary.VERTICAL, ref v);
    }

    public void dens_step()
    {
        add_source(N, ref density, ref prev_density, sim_delta_time);
        SWAP(ref prev_density, ref density); 
        diffuse(N, Boundary.NONE, ref density, ref prev_density, diffusion_rate, sim_delta_time);
        SWAP(ref prev_density, ref density); 
        advect(N, Boundary.NONE, ref density, ref prev_density, ref velocity_horizontal, ref velocity_vertical, sim_delta_time);
    }

    public void vel_step()
    {
        add_source(N, ref velocity_horizontal, ref prev_velocity_horizontal, sim_delta_time);
        add_source(N, ref velocity_vertical, ref prev_velocity_vertical, sim_delta_time);

        SWAP(ref prev_velocity_horizontal, ref velocity_horizontal);
        diffuse(N, Boundary.HORIZONTAL, ref velocity_horizontal, ref prev_velocity_horizontal, viscosity, sim_delta_time);
        SWAP(ref prev_velocity_vertical, ref velocity_vertical);
        diffuse(N, Boundary.VERTICAL, ref velocity_vertical, ref prev_velocity_vertical, viscosity, sim_delta_time);
        
        project(N, ref velocity_horizontal, ref velocity_vertical, ref prev_velocity_horizontal, ref prev_velocity_vertical);

        SWAP(ref prev_velocity_horizontal, ref velocity_horizontal);
        SWAP(ref prev_velocity_vertical, ref velocity_vertical);

        advect(N, Boundary.HORIZONTAL, ref velocity_horizontal, ref prev_velocity_horizontal, ref prev_velocity_horizontal, ref prev_velocity_vertical, sim_delta_time); 
        advect(N, Boundary.VERTICAL, ref velocity_vertical, ref prev_velocity_vertical, ref prev_velocity_horizontal, ref prev_velocity_vertical, sim_delta_time);
        
        project(N, ref velocity_horizontal, ref velocity_vertical, ref prev_velocity_horizontal, ref prev_velocity_vertical);
    }


    /// <summary>
    /// This returns the full (n+2)*(n+2) array 
    /// </summary>
    /// <returns></returns>
    public ref PackedArray<float> getDensity()
    {
        return ref density;
    }
    /// <summary>
    /// This returns the full (n+2)*(n+2) array*/
    /// </summary>
    /// <returns></returns>
    public ref PackedArray<float> getDensityPrev()
    {
        return ref prev_density;
    }
    /// <summary>
    /// This returns the full (n+2)*(n+2) array*/
    /// </summary>
    /// <returns></returns>
    public ref PackedArray<float> getVelocityX()
    {
        return ref velocity_horizontal;
    }
    /// <summary>
    /// This returns the full (n+2)*(n+2) array*/
    /// </summary>
    /// <returns></returns>
    public ref PackedArray<float> getVelocityY()
    {
        return ref velocity_vertical;
    }
    /// <summary>
    /// This returns the full (n+2)*(n+2) array*/
    /// </summary>
    /// <returns></returns>
    public ref PackedArray<float> getVelocityXPrev()
    {
        return ref prev_velocity_horizontal;
    }
    /// <summary>
    /// This returns the full (n+2)*(n+2) array*/
    /// </summary>
    /// <returns></returns>
    public ref PackedArray<float> getVelocityYPrev()
    {
        return ref prev_velocity_vertical;
    }
    /// <summary>
    /// This returns the full (n+2)*(n+2) array
    /// </summary>
    /// <param name="density"></param>
    /// <param name="density_prev"></param>
    /// <param name="velocity_horizontal"></param>
    /// <param name="velocity_horizontal_prev"></param>
    /// <param name="velocity_vertical"></param>
    /// <param name="velocity_vertical_prev"></param>
    /// <param name="N"></param>
    public void getAll(out PackedArray<float> density, out PackedArray<float> density_prev, out PackedArray<float> velocity_horizontal, out PackedArray<float> velocity_horizontal_prev, out PackedArray<float> velocity_vertical, out PackedArray<float> velocity_vertical_prev, out int N)
    {
        density = this.density;
        density_prev = this.prev_density;
        velocity_horizontal = this.velocity_horizontal;
        velocity_vertical = this.velocity_vertical;
        velocity_horizontal_prev = this.prev_velocity_horizontal;
        velocity_vertical_prev = this.prev_velocity_vertical;
        N = this.N;
    }

}




