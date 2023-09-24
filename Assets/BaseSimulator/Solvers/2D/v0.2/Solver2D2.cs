class Solver2D2
{
    //Vector Fields
    public float[] u;
    public float[] v;
    public float[] u0;
    public float[] v0;
    public float[] x;
    public float[] x0;

    //Constants
    int N;
    float diff, visc, dt;


    public Solver2D2(int N, float diffusionRate, float viscosity, float deltaTime)
    {
        int size = (N + 2) * (N + 2);

        u = new float[size];    //Velocity (Horizontal)
        v = new float[size];    //Velocity (Vertical)
        u0 = new float[size];     //Previous Velocity (Horizontal)
        v0 = new float[size];   //Previous Velocity (Vertical)
        x = new float[size];    //Density
        x0 = new float[size];   //Previous Density

        this.diff = diffusionRate;
        this.visc = viscosity;
        this.dt = deltaTime;
        this.N = N;
    }

    enum Boundary
    {
        NONE, HORIZONTAL, VERTICAL
    }
    void SWAP<T>(ref T a, ref T b) { T temp = a; a = b; b = temp; }

    void add_source(int N, ref float[] x, ref float[] s, float dt)
    {
        int i, size = (N + 2) * (N + 2);
        for (i = 0; i < size; i++) x[i] += dt * s[i];
    }

    void set_bnd(int N, int b, ref float[] x)
    {
        int i;

        for (i = 1; i <= N; i++)
        {
            x[((0) + (N + 2) * (i))] = b == 1 ? -x[((1) + (N + 2) * (i))] : x[((1) + (N + 2) * (i))];
            x[((N + 1) + (N + 2) * (i))] = b == 1 ? -x[((N) + (N + 2) * (i))] : x[((N) + (N + 2) * (i))];
            x[((i) + (N + 2) * (0))] = b == 2 ? -x[((i) + (N + 2) * (1))] : x[((i) + (N + 2) * (1))];
            x[((i) + (N + 2) * (N + 1))] = b == 2 ? -x[((i) + (N + 2) * (N))] : x[((i) + (N + 2) * (N))];
        }
        x[((0) + (N + 2) * (0))] = 0.5f * (x[((1) + (N + 2) * (0))] + x[((0) + (N + 2) * (1))]);
        x[((0) + (N + 2) * (N + 1))] = 0.5f * (x[((1) + (N + 2) * (N + 1))] + x[((0) + (N + 2) * (N))]);
        x[((N + 1) + (N + 2) * (0))] = 0.5f * (x[((N) + (N + 2) * (0))] + x[((N + 1) + (N + 2) * (1))]);
        x[((N + 1) + (N + 2) * (N + 1))] = 0.5f * (x[((N) + (N + 2) * (N + 1))] + x[((N + 1) + (N + 2) * (N))]);
    }

    void lin_solve(int N, int b, ref float[] x, ref float[] x0, float a, float c)
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

    void diffuse(int N, int b, ref float[] x, ref float[] x0, float diff, float dt)
    {
        float a = dt * diff * N * N;
        lin_solve(N, b, ref x, ref x0, a, 1 + 4 * a);
    }

    void advect(int N, int b, ref float[] d, ref float[] d0, ref float[] u, ref float[] v, float dt)
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

    void project(int N, ref float[] u, ref float[] v, ref float[] p, ref float[] div)
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

        set_bnd(N, 1, ref u); set_bnd(N, 2, ref v);
    }

    public void dens_step()
    {
        add_source(N, ref x, ref x0, dt);
        SWAP(ref x0, ref x); diffuse(N, 0, ref x, ref x0, diff, dt);
        SWAP(ref x0, ref x); advect(N, 0, ref x, ref x0, ref u, ref v, dt);
    }

    public void vel_step()
    {
        add_source(N, ref u, ref u0, dt); add_source(N, ref v, ref v0, dt);
        SWAP(ref u0, ref u); diffuse(N, 1, ref u, ref u0, visc, dt);
        SWAP(ref v0, ref v); diffuse(N, 2, ref v, ref v0, visc, dt);
        project(N, ref u, ref v, ref u0, ref v0);
        SWAP(ref u0, ref u); SWAP(ref v0, ref v);
        advect(N, 1, ref u, ref u0, ref u0, ref v0, dt); advect(N, 2, ref v, ref v0, ref u0, ref v0, dt);
        project(N, ref u, ref v, ref u0, ref v0);
    }



    public ref float[] getDensity()
    {
        return ref x;
    }
    public ref float[] getDensityPrev()
    {
        return ref x0;
    }
    public ref float[] getVelocityX()
    {
        return ref u;
    }
    public ref float[] getVelocityY()
    {
        return ref v;
    }
    public ref float[] getVelocityXPrev()
    {
        return ref u0;
    }
    public ref float[] getVelocityYPrev()
    {
        return ref v0;
    }
    public void getAll(out float[] density, out float[] density_prev, out float[] velocity_horizontal, out float[] velocity_horizontal_prev, out float[] velocity_vertical, out float[] velocity_vertical_prev, out int N)
    {
        density = this.x;
        density_prev = this.x0;
        velocity_horizontal = this.u;
        velocity_vertical = this.v;
        velocity_horizontal_prev = this.u0;
        velocity_vertical_prev = this.v0;
        N = this.N;
    }

}




