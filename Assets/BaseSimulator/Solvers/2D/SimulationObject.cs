using Unity.VisualScripting;

abstract class SimulationObject
{
    public int x, y;
    public int width, height;
    public UnityEngine.Color debugColor;
}

class VelocityForceField : SimulationObject
{
    public VelocityForceField(int x, int y, int width, int height, UnityEngine.Color debugColor)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.debugColor = debugColor;
    }
    public void tick(ref PackedArray<float> velX, ref PackedArray<float> velY, float valueX, float valueY)
    {
        for (int i = x; i < width + x; i++) for (int j = y; j < height + y; j++)
            {
                velX[i,j] = valueX;
                velY[i,j] = valueY;
            }
    }
}
class DensityEnforcer : SimulationObject
{
    public DensityEnforcer(int x, int y, int width, int height, UnityEngine.Color debugColor)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.debugColor = debugColor;
    }
    public void tick(ref PackedArray<float> density, float value)
    {
        for (int i = x; i < width + x; i++) for (int j = y; j< height + y; j++)
            {
                density[i, j] = value;
            }
    }
}