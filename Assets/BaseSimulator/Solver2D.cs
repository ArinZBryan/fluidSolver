using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Solver2D
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
    readonly bool USE_COMPLEX_BOUNDARIES;
    List<CollidableCell> physicsObjects;


    public Solver2D(int N, float diffusionRate, float viscosity, float deltaTime, bool complexBoundaries)
    {
        int size = (N + 2) * (N + 2);

        velocity_horizontal = new PackedArray<float>(new int[] { N + 2, N + 2 });               //Velocity (Horizontal)
        velocity_vertical = new PackedArray<float>(new int[] { N + 2, N + 2 });             //Velocity (Vertical)
        prev_velocity_horizontal = new PackedArray<float>(new int[] { N + 2, N + 2 });      //Previous Velocity (Horizontal)
        prev_velocity_vertical = new PackedArray<float>(new int[] { N + 2, N + 2 });        //Previous Velocity (Vertical)
        density = new PackedArray<float>(new int[] { N + 2, N + 2 });                       //Density
        prev_density = new PackedArray<float>(new int[] { N + 2, N + 2 });                  //Previous Density
        physicsObjects = new List<CollidableCell>();

        this.diffusion_rate = diffusionRate;
        this.viscosity = viscosity;
        this.sim_delta_time = deltaTime;
        this.N = N;
        this.USE_COMPLEX_BOUNDARIES = Config.getBool("complex_boundaries");
    }

    [Flags] // Sets this enum to work using bitwise operations, so that we can combine multiple flags into one variable
            // | = union, & = intersection, ^ = xor, ! = not
    public enum Boundary
    {
        NONE = 0b0000_0000,
        HORIZONTAL = 0b0000_0001,
        VERTICAL = 0b0000_0010,
        LEFT = 0b0000_0100,
        RIGHT = 0b0000_1000,
        TOP = 0b0001_0000,
        BOTTOM = 0b0010_0000
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
        if (USE_COMPLEX_BOUNDARIES)
        {
            if (b == Boundary.HORIZONTAL)
            {
                set_bnd_complex(N, Boundary.LEFT, ref x);
                set_bnd_complex(N, Boundary.RIGHT, ref x);
            }
            else if (b == Boundary.VERTICAL)
            {
                set_bnd_complex(N, Boundary.TOP, ref x);
                set_bnd_complex(N, Boundary.BOTTOM, ref x);
            }
            else
            {
                set_bnd_complex(N, b, ref x);
            }
        }
        else
        {
            set_bnd_compatibility(N, b, ref x);
        }
    }
    void set_bnd_compatibility(int N, Boundary b, ref PackedArray<float> x)
    {
        int i;

        //Deals with the boundaries of the simulation.
        for (i = 1; i <= N; i++)
        {
            if (b == Boundary.HORIZONTAL)
            {
                x[0, i] = -x[1, i];
                x[N + 1, i] = -x[N, i];
            }
            else
            {
                x[0, i] = x[1, i];
                x[N + 1, i] = x[N, i];
            }

            if (b == Boundary.VERTICAL)
            {
                x[i, 0] = -x[i, 1];
                x[i, N + 1] = -x[i, N];
            }
            else
            {
                x[i, 0] = x[i, 1];
                x[i, N + 1] = x[i, N];
            }
        }

        //corners are average of directly ajacent cells
        x[0, 0] = 0.5f * (x[1, 0] + x[0, 1]);
        x[0, N + 1] = 0.5f * (x[1, N + 1] + x[0, N]);
        x[N + 1, 0] = 0.5f * (x[N, 0] + x[N + 1, 1]);
        x[N + 1, N + 1] = 0.5f * (x[N, N + 1] + x[N + 1, N]);
    }
    void set_bnd_complex(int N, Boundary b, ref PackedArray<float> x)
    {
        int i;

        //Deals with the boundaries of the simulation.
        for (i = 1; i <= N; i++)
        {
            if (b == Boundary.LEFT) { x[0, i] = -x[1, i]; } else { x[0, i] = x[1, i]; }
            if (b == Boundary.RIGHT) { x[N + 1, i] = -x[N, i]; } else { x[N + 1, i] = x[N, i]; }
            if (b == Boundary.TOP) { x[i, 0] = -x[i, 1]; } else { x[i, 0] = x[i, 1]; }
            if (b == Boundary.BOTTOM) { x[i, N + 1] = -x[i, N]; } else { x[i, N + 1] = x[i, N]; }
        }


        //Iterate through all physics simulationObjects
        int physicsObjectX, physicsObjectY, physicsObjectWidth, physicsObjectHeight;
        foreach (var physicsObject in physicsObjects)
        {
            physicsObjectX = physicsObject.x;
            physicsObjectY = physicsObject.y;
            physicsObjectWidth = physicsObject.width;
            physicsObjectHeight = physicsObject.height;
            //handle edges
            if (physicsObject.collidableFaces.HasFlag(Boundary.LEFT))
            {
                for (i = physicsObjectY; i < physicsObjectY + physicsObjectHeight; i++)
                {
                    x[physicsObjectX, i] = -x[physicsObjectX - 1, i];
                }
            }
            if (physicsObject.collidableFaces.HasFlag(Boundary.RIGHT))
            {
                for (i = physicsObjectY; i < physicsObjectY + physicsObjectHeight; i++)
                {
                    x[physicsObjectX + physicsObjectWidth, i] = -x[physicsObjectX + physicsObjectWidth + 1, i];
                }
            }
            if (physicsObject.collidableFaces.HasFlag(Boundary.TOP))
            {
                for (i = physicsObjectX; i < physicsObjectX + physicsObjectWidth; i++)
                {
                    x[i, physicsObjectY] = -x[i, physicsObjectY - 1];
                }
            }
            if (physicsObject.collidableFaces.HasFlag(Boundary.BOTTOM))
            {
                for (i = physicsObjectX; i < physicsObjectX + physicsObjectWidth; i++)
                {
                    x[i, physicsObjectY + physicsObjectHeight] = -x[i, physicsObjectY + physicsObjectHeight + 1];
                }
            }
            //Handle corners by taking the average of the two directly adjacent cells
            if (physicsObject.collidableFaces.HasFlag(Boundary.LEFT | Boundary.BOTTOM))
            {
                x[physicsObjectX, physicsObjectY] = 0.5f * (x[physicsObjectX - 1, physicsObjectY] + x[physicsObjectX, physicsObjectY - 1]);
            }
            if (physicsObject.collidableFaces.HasFlag(Boundary.LEFT | Boundary.TOP))
            {
                x[physicsObjectX, physicsObjectY + physicsObjectHeight] = 0.5f * (x[physicsObjectX - 1, physicsObjectY + physicsObjectHeight] + x[physicsObjectX, physicsObjectY + physicsObjectHeight + 1]);
            }
            if (physicsObject.collidableFaces.HasFlag(Boundary.RIGHT | Boundary.BOTTOM))
            {
                x[physicsObjectX + physicsObjectWidth, physicsObjectY] = 0.5f * (x[physicsObjectX + physicsObjectWidth + 1, physicsObjectY] + x[physicsObjectX + physicsObjectWidth, physicsObjectY - 1]);
            }
            if (physicsObject.collidableFaces.HasFlag(Boundary.RIGHT | Boundary.TOP))
            {
                x[physicsObjectX + physicsObjectWidth, physicsObjectY + physicsObjectHeight] = 0.5f * (x[physicsObjectX + physicsObjectWidth + 1, physicsObjectY + physicsObjectHeight] + x[physicsObjectX + physicsObjectWidth, physicsObjectY + physicsObjectHeight + 1]);
            }
                

        }

        //corners are average of directly ajacent cells
        x[0, 0] = 0.5f * (x[1, 0] + x[0, 1]);
        x[0, N + 1] = 0.5f * (x[1, N + 1] + x[0, N]);
        x[N + 1, 0] = 0.5f * (x[N, 0] + x[N + 1, 1]);
        x[N + 1, N + 1] = 0.5f * (x[N, N + 1] + x[N + 1, N]);
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

        set_bnd_complex(N, Boundary.NONE, ref div);
        set_bnd_complex(N, Boundary.NONE, ref p);

        lin_solve(N, 0, ref p, ref div, 1, 4);

        for (i = 1; i <= N; i++)
        {
            for (j = 1; j <= N; j++)
            {
                u[((i) + (N + 2) * (j))] -= 0.5f * N * (p[((i + 1) + (N + 2) * (j))] - p[((i - 1) + (N + 2) * (j))]);
                v[((i) + (N + 2) * (j))] -= 0.5f * N * (p[((i) + (N + 2) * (j + 1))] - p[((i) + (N + 2) * (j - 1))]);
            }
        }

        set_bnd_complex(N, Boundary.LEFT, ref u);
        set_bnd_complex(N, Boundary.RIGHT, ref u);
        set_bnd_complex(N, Boundary.TOP, ref v);
        set_bnd_complex(N, Boundary.BOTTOM, ref v);
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
    /// <param fileName="density"></param>
    /// <param fileName="density_prev"></param>
    /// <param fileName="velocity_horizontal"></param>
    /// <param fileName="velocity_horizontal_prev"></param>
    /// <param fileName="velocity_vertical"></param>
    /// <param fileName="velocity_vertical_prev"></param>
    /// <param fileName="N"></param>
    public void getAll( out PackedArray<float> density, 
                        out PackedArray<float> density_prev, 
                        out PackedArray<float> velocity_horizontal, 
                        out PackedArray<float> velocity_horizontal_prev, 
                        out PackedArray<float> velocity_vertical, 
                        out PackedArray<float> velocity_vertical_prev)
    {
        density = this.density;
        density_prev = this.prev_density;
        velocity_horizontal = this.velocity_horizontal;
        velocity_vertical = this.velocity_vertical;
        velocity_horizontal_prev = this.prev_velocity_horizontal;
        velocity_vertical_prev = this.prev_velocity_vertical;
    }
    public void getCurrent( out PackedArray<float> density, 
                            out PackedArray<float> velocity_horizontal,
                            out PackedArray<float> velocity_vertical)
    {
        density = this.density;
        velocity_horizontal = this.velocity_horizontal;
        velocity_vertical = this.velocity_vertical;
    }
    public (float, float, float, int) getConstants()
    {
        return (diffusion_rate, viscosity, sim_delta_time, N);
    }
    public List<CollidableCell> getPhysicsObjects()
    {
        return physicsObjects;
    }
    public void addPhysicsObject(CollidableCell obj)
    {
        physicsObjects.Add(obj);
    }
    public void removePhysicsObject(CollidableCell obj)
    {
        physicsObjects.Remove(obj);
    }
    public void setPhysicsObjects(List<CollidableCell> objs)
    {
        physicsObjects = objs;
    }
}
