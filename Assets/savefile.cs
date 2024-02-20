using System.Collections.Generic;
using System.Linq;
[System.Serializable]
public class PlaybackFile
{
    public PlaybackFrame[] frames;
    public KeyFrame startFrame;
    public int recordingTime;

    public PlaybackFile(IEnumerable<PlaybackFrame> frames, KeyFrame startFrame)
    {
        this.frames = frames.ToArray();
        recordingTime = frames.Count();
        this.startFrame = startFrame;
    }
}
[System.Serializable]
public class PlaybackFrame
{
    public List<UserInput> input;
    public List<SimulationObject> objects;
    public PlaybackFrame(IEnumerable<UserInput> inputs, IEnumerable<SimulationObject> objects)
    {
        this.input = (List<UserInput>)inputs;
        this.objects = (List<SimulationObject>)objects;
    }
}
[System.Serializable]
public class KeyFrame
{
    public int N;
    public float diffusion_rate;
    public float viscosity;
    public float sim_delta_time;
    public PackedArray<float> velocity_horizontal;
    public PackedArray<float> velocity_vertical;
    public PackedArray<float> prev_velocity_horizontal;
    public PackedArray<float> prev_velocity_vertical;
    public PackedArray<float> density;
    public PackedArray<float> prev_density;
    public List<CollidableCell> physicsObjects;
    public KeyFrame(Solver2D solver)
    {
        solver.getAll(out density, out prev_density, out velocity_horizontal, out prev_velocity_horizontal, out velocity_vertical, out prev_velocity_vertical);
        (diffusion_rate, viscosity, sim_delta_time, N) = solver.getConstants();
        physicsObjects = solver.getPhysicsObjects();
    }
}
[System.Serializable]
public struct UserInput
{
    public int x;
    public int y;
    public float value;
    public enum fieldToWriteTo
    {
        VELX, VELY, DENS
    }
    public fieldToWriteTo field;
    public UserInput(int x, int y, float value, fieldToWriteTo field)
    {
        this.x = x;
        this.y = y;
        this.value = value;
        this.field = field;

    }
}