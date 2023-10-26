## Summary
A 3D fluid simulation using the Navier-Stokes equations to accurately simulate an incompressible fluid in a grid. This simulation can be tweaked for no additional cost to provide a 2D fluid simulation (by making the grid thickness 1). This simulation will be based off methods detailed in the paper [*"Real-Time Fluid Dynamics for Games"*](https://web.archive.org/web/20200805215025/https://pdfs.semanticscholar.org/847f/819a4ea14bd789aca8bc88e85e906cfc657c.pdf) presented at the 2003 *Game Developer's Conference* by *Jos Stam*. These methods were subsequently used as part of 3DS max's MAYA animation toolset.
# Introduction

## The Problem
In 3D-Graphics workloads, such as in CGI, one of the most complex and computationally expensive actions is the simulation of fluids in a realistic manner. Further, it is highly complex to set up such a simulation, as highly accurate simulations have a tendency to 'blow up', or otherwise act erratically, wasting much time computing simulations that will ultimately be scrapped. It is for this reason, that it is highly useful to use a dedicated program to view and tweak the simulation, in realtime. This allows for the later composition of the resulting images into such projects.

## My Solution
Using the simulation method outlined in the 2003 paper 'Real-Time Fluid Dynamics for games', I can simulate almost any situation involving incompressible fluids. The simulation method outlined in the paper is optimised for both speed of computation, and also stability. This makes it exceedingly difficult to create a simulation where the final output does not look plausibly realistic. The main program will consist of a rendered view, using a lower resolution version of the simulation. To allow for easy rendering of 2D/3D scenes, the Unity game engine will be used to provide easy CPU and GPU rendering capabilities. This also allows for significant portability, which is important, as artists may choose to work on their preferred platform, without disrupting their workflow. The simulation can finally be exported, at any size and file format using integrated command-line tools (FFMPEG).

## Main features of the solution
My solution should be able to at a baseline, simulate stable fluid dynamics in 2D on the CPU, using managed C# code as part of Unity. This simulation should also be able to be tweaked in semi-realtime. This should then be able to be saved as an image sequence or video that can be used elsewhere. However, I intend to implement both 3D simulation and GPU acceleration using compute shaders that can be run in parallel. I would also like to have a fully interactable and possibly scriptable simulation, such that velocities and densities can be added and changed as the simulation runs.

## Stakeholders
The main stakeholders for my program would be animators and video editors. It allows for complexly simulated effects to be easily included in animations / videos. This is done by outputting a video that can be easily composited with any standard non-linear editor.

### Stakeholder #1

### Stakeholder Interview #1

### Stakeholder #2

### Stakeholder Interview #2

## Review of similar solutions
### Blender
Blender is a full 3D modelling and animation package. It also contains a highly sophisticated simulator for many types of body, from rigidbodies and softbodies to gasses and incompressible fluids. As such, it is able to more closely integrate the simulation into the animation workflow. However, it is not non-trivial to export just a simulation, even as an image sequence, as a full camera setup and rendering pipeline must be set up beforehand.

### Realflow for Cinema 4D
Realflow is an addon for Cinema 4D that allows for the simulation of many types of body. Like Blender, it is easily integrated into a highly capable and extensible 3D modelling and animation software package (in this case, Maxon Cinema 4D). This means that these complex simulations can be recomputed around the animation quickly. The main downside of this package, is that both it and the 

- SPlisH SPlasH
