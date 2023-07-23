using UnityEngine;

namespace FluidSolver
{
    public class Solver
    {
        public class State
        {
            public int N;
            public float dt, diff, visc;
            public float[,] velocityX;
            public float[,] velocityY;
            public float[,] velocity_prevX;
            public float[,] velocity_prevY;
            public float[,] density;
            public float[,] density_prev;

            public State(int N, float deltaTime, float diffusionRate, float viscosity)
            {
                this.N = N;
                this.dt = deltaTime;
                this.diff = diffusionRate;
                this.visc = viscosity;

                this.velocityX = new float[N + 2, N + 2];
                this.velocityY = new float[N + 2, N + 2];
                this.velocity_prevX = new float[N + 2, N + 2];
                this.velocity_prevY = new float[N + 2, N + 2];
                this.density = new float[N + 2, N + 2];
                this.density_prev = new float[N + 2, N + 2];
            }

            public Vector2[,] getVelocityField()
            {
                Vector2[,] velocity = new Vector2[N + 2, N + 2];
                for (int i = 0; i < N + 2; i++)
                    for (int j = 0; j < N + 2; j++)
                        velocity[i, j] = new Vector2(velocityX[i, j], velocityY[i, j]);
                return velocity;
            }

            public void setVelocityField(Vector2[,] velocities)
            {
                for (int i = 0; i < N + 2; i++)
                {
                    for (int j = 0; j < N + 2; j++)
                    {
                        velocityX[i, j] = velocities[i, j].x;
                        velocityY[i, j] = velocities[i, j].y;
                    }
                }
            }

        }

        public State currentState;
        enum BoundaryType { IGNORE, HORIZONTAL, VERTICAL }

        public Solver(int N, float deltaTime, float diffusionRate, float viscosity)
        {
            currentState = new State(N, deltaTime, diffusionRate, viscosity);
        }

        //Add arbritrary source array to arbritrary destination array
        float[,] add_source(int N, float[,] dest, float[,] sources, float dt)
        {
            for (int i = 0; i < N + 2; i++)
                for (int j = 0; j < N + 2; j++)
                    dest[i, j] += dt * sources[i, j];
            return dest;
        }

        //Set BoundaryType conditions for arbritrary array x
        float[,] set_bnd(int N, BoundaryType boundaryType, float[,] x)
        {
            for (int i = 1; i <= N; i++)    //Performs BoundaryType checks for all edges of the array
            {
                //NOTE: the boundaryType enum is used to ensure that BoundaryType conditions for a particular direction are only applied during the right passes
                //		This is to prevent the corners from being erroneously dealt with twice
                x[0, i] = (boundaryType == BoundaryType.HORIZONTAL) ? -x[1, i] : x[1, i];       //left edge
                x[N + 1, i] = (boundaryType == BoundaryType.HORIZONTAL) ? -x[N, i] : x[N, i];       //right edge
                x[i, 0] = (boundaryType == BoundaryType.VERTICAL) ? -x[i, 1] : x[i, 1];         //top edge
                x[i, N + 1] = (boundaryType == BoundaryType.VERTICAL) ? -x[i, N] : x[i, N];     //bottom edge
            }

            // Diffusion into corner cells
            // This is done now because the corner cells are only partially updated during the BoundaryType checks above
            x[0, 0] = 0.5f * (x[1, 0] + x[0, 1]);
            x[0, N + 1] = 0.5f * (x[1, N + 1] + x[0, N]);
            x[N + 1, 0] = 0.5f * (x[N, 0] + x[N + 1, 1]);
            x[N + 1, N + 1] = 0.5f * (x[N, N + 1] + x[N + 1, N]);

            return x;
        }

        //This is the meat of the diffusion function, and uses a method like the Gauss-Seidel relaxation method to diffuse the array stably
        float[,] lin_solve(int N, BoundaryType boundaryType, float[,] x, float[,] x0, float a, float c)
        {
            for (int k = 0; k < 20; k++)    //20 iterations
            {
                //For each cell in the array, set the value to the average of the surrounding cells
                for (int i = 1; i <= N; i++)
                    for (int j = 1; j <= N; j++)

                        //For each cell in the array, set the value to be the sum of the surrounding cells, times the diffusion rate a
                        //This method is derived from the 'diffuse_bad' function in the original stable fluids paper, but replacing the final -4x_n(i,j) with -4x_n+1(i,j)
                        //This allows for the method to be made more stable by using a more implicit method, which is common in fluid simulation
                        //The c term is just a remnant of the rearrangement, and comes to the RHS, when solving for x_n+1(i,j)

                        x[i, j] = (x0[i, j] + a * (x[i - 1, j] + x[i + 1, j] + x[i, j - 1] + x[i, j + 1])) / c;
                x = set_bnd(N, boundaryType, x);
            }
            return x;
        }

        //Diffuse Arbiritrary array x0 into x, with diffusion rate diff.
        float[,] diffuse(int N, BoundaryType boundaryType, float[,] x, float[,] x0, float diff, float dt)
        {
            float diffusion_rate = dt * diff * N * N;   //Why is N^2 multiplied here?
            return lin_solve(N, boundaryType, x, x0, diffusion_rate, 1 + 4 * diffusion_rate); //Why are we dividing by 1 + 4*a?
        }

        //Advect step always had same params, so function has been simplified, and made less generic.
        State advect(State cur_state, BoundaryType boundaryType)
        {
            int n = cur_state.N;
            float dt = cur_state.dt;


            float dt0 = dt * n;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    float x = i - dt0 * cur_state.velocityX[i, j];
                    float y = j - dt0 * cur_state.velocityY[i, j];
                    if (x < 0.5f) x = 0.5f; if (x > n + 0.5f) x = n + 0.5f; int i0 = (int)x; int i1 = i0 + 1;
                    if (y < 0.5f) y = 0.5f; if (y > n + 0.5f) y = n + 0.5f; int j0 = (int)y; int j1 = j0 + 1;
                    float s1 = x - i0; float s0 = 1 - s1; float t1 = y - j0; float t0 = 1 - t1;
                    cur_state.density[i, j] = s0 * (t0 * cur_state.density_prev[i0, j0] + t1 * cur_state.density_prev[i0, j1]) + s1 * (t0 * cur_state.density_prev[i1, j0] + t1 * cur_state.density_prev[i1, j1]);
                }
            }
            cur_state.density = set_bnd(n, boundaryType, cur_state.density);
            return cur_state;
        }

        //void project(int N, ref float[,] u, ref float[,] v, float[,] p , float[,] div)
        State project(State cur_state)
        {
            int N = cur_state.N;

            for (int i = 1; i <= N; i++)
            {
                for (int j = 1; j <= N; j++)
                {
                    cur_state.velocity_prevY[i, j] = -0.5f * (cur_state.velocityX[i + 1, j] - cur_state.velocityX[i - 1, j] + cur_state.velocityY[i, j + 1] - cur_state.velocityY[i, j - 1]) / N;
                    cur_state.density[i, j] = 0;
                }
            }
            cur_state.density = set_bnd(N, BoundaryType.IGNORE, cur_state.density);
            cur_state.velocity_prevY = set_bnd(N, BoundaryType.IGNORE, cur_state.velocity_prevY);
            cur_state.density = lin_solve(N, BoundaryType.IGNORE, cur_state.density, cur_state.velocity_prevY, 1, 4);
            for (int i = 1; i <= N; i++)
            {
                for (int j = 1; j <= N; j++)
                {
                    cur_state.velocityX[i, j] -= 0.5f * N * (cur_state.density[i + 1, j] - cur_state.density[i - 1, j]);
                    cur_state.velocityY[i, j] -= 0.5f * N * (cur_state.density[i, j + 1] - cur_state.density[i, j - 1]);
                }
            }
            cur_state.velocityX = set_bnd(N, BoundaryType.HORIZONTAL, cur_state.velocityX);
            cur_state.velocityY = set_bnd(N, BoundaryType.VERTICAL, cur_state.velocityY);

            return cur_state;
        }

        //Density Step - Takes current state and returns next state
        public State dens_step(State cur_state)
        {
            int N = cur_state.N;
            float dt = cur_state.dt;
            float diff = cur_state.diff;

            cur_state.density = add_source(N, cur_state.density, cur_state.density_prev, dt);
            swap(ref cur_state.density_prev, ref cur_state.density);
            cur_state.density = diffuse(N, BoundaryType.IGNORE, cur_state.density, cur_state.density_prev, diff, dt);
            swap(ref cur_state.density_prev, ref cur_state.density);
            cur_state = advect(cur_state, BoundaryType.IGNORE);
            return cur_state;
        }

        //Velocity Step - Takes current state and returns next state
        public State vel_step(State cur_state)
        {
            int N = cur_state.N;
            float dt = cur_state.dt;
            float visc = cur_state.visc;

            cur_state.velocityX = add_source(N, cur_state.velocityX, cur_state.velocity_prevX, dt);
            cur_state.velocityY = add_source(N, cur_state.velocityY, cur_state.velocity_prevY, dt);

            swap(ref cur_state.velocity_prevX, ref cur_state.velocityX);
            cur_state.velocityX = diffuse(N, BoundaryType.HORIZONTAL, cur_state.velocityX, cur_state.velocity_prevX, visc, dt);
            swap(ref cur_state.velocity_prevY, ref cur_state.velocityY);
            cur_state.velocityY = diffuse(N, BoundaryType.VERTICAL, cur_state.velocityY, cur_state.velocity_prevY, visc, dt);

            cur_state = project(cur_state);

            swap(ref cur_state.velocity_prevX, ref cur_state.velocityX);
            swap(ref cur_state.velocity_prevY, ref cur_state.velocityY);

            cur_state = advect(cur_state, BoundaryType.HORIZONTAL);
            cur_state = advect(cur_state, BoundaryType.VERTICAL);

            cur_state = project(cur_state);
            return cur_state;
        }
        
        public State step(Vector2[,] velocityField, float[,] densityField)
        {
            currentState.setVelocityField(velocityField);
            currentState.density = densityField;
            currentState = vel_step(currentState);
            currentState = dens_step(currentState);
            return currentState;
        }

        T swap<T>(ref T a, ref T b) { T t = a; a = b; b = t; return a; }
    }
}