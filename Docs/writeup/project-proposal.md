# Incompressible Fluid Simulation


## Tooling
* C#    (For writing general / engine code)
* HLSL  (For compute shaders, if necessary as part of optimisation)
    > Based off the C Programming Language, but for parallel compute on the GPU
* Unity (Window Management / Graphics API)
* ffmpeg (rendering videos / *.gif files)

## Example Stakeholder
VFX Studio / Animator
(friend)

## Functions
1. Fluid Simulation
    1. Compute Simulation vector field
        1. Grid Depth
        2. Self-adevction
        3. Projection
        4. Diffusion
        5. Add Forces
    2. Compute Simulation densities
        1. Grid Depth
        2. Self-adevction
        3. Projection
        4. Diffusion
        5. Add Forces
    3. Particle Motion through vector field
        1. Render Particles as quads
        2. Render Particles as points
        3. Render Particles as meshes
2. On-The-Fly Rendering
    1. Render vector field to texture
        1. Render as colours
        1. Render as arrows
    2. Render densities to texture
    3. User Interaction
        1. Add density to simulation with mouse
        2. Add forces to simulation with mouse
        3. Place density sources in simuation
        4. Place force fields in simulation
        5. Place objects in simulation
            1. Place cubes
            2. ***STRETCH GOAL*** Place Meshes (.obj / .fbx)
                1. Cubic volume approximation
3. Export Simulation
    1. Render Simulation to *.gif file
        1. Setup ffmpeg to draw from image sequence
        2. run ffmpeg in background
    2. Render Simulation to *.mp4 file
        1. Setup ffmpeg to draw from image sequence
        2. run ffmpeg in background
    3. ***STRETCH GOAL:*** Export Simulation to mesh (.obj / .fbx)
4. GUI
    1. GUI for simulation parameters
        1. Grid Size
        2. Render Options
        3. Mouse Options
            1. Add Force / Density toggle
            2. Add Obstacle Toggle
            3. Add generator toggle
        4. 2D / 3D Toggle
    2. GUI for simulation save / load / export
        1. Render Size
        2. Render FPS
        3. GPU Hardware Acceleration for Video Encoding
        4. Load premade simulation file
        5. Save current simulation to file


