using System;
[System.Serializable]
public abstract class SimulationObject
{
    public int x, y;
    public int width, height;
    public UnityEngine.Color debugColor;
}
[System.Serializable]
public class VelocityForceField : SimulationObject
{
    float valueX, valueY;
    public VelocityForceField(int x, int y, int width, int height, float valueX, float valueY, UnityEngine.Color debugColor)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.valueX = valueX;
        this.valueY = valueY;
        this.debugColor = debugColor;
    }
    public void tick(ref PackedArray<float> velX, ref PackedArray<float> velY)
    {
        for (int i = x; i < width + x; i++) for (int j = y; j < height + y; j++)
            {
                velX[i,j] = valueX;
                velY[i,j] = valueY;
            }
    }
}
[System.Serializable]
public class DensityEnforcer : SimulationObject
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
[System.Serializable]
public class PhysPoint : SimulationObject
{
    float realX, realY;
    public PhysPoint(int x, int y, UnityEngine.Color debugColor)
    {
        this.x = x;
        this.y = y;
        this.realX = x;
        this.realY = y;
        this.width = 3;
        this.height = 3;
        this.debugColor = debugColor;
    }
    public void tick(ref PackedArray<float> velocityX, ref PackedArray<float> velocityY, float deltaTime)
    {
        float dx = velocityX[x, y] * deltaTime * 10;
        float dy = velocityY[x, y] * deltaTime * 10;
        if (this.realX > velocityX.dimensions[0] || this.realX < 0) { dx = -dx; }
        if (this.realY > velocityY.dimensions[1] || this.realY < 0) { dy = -dy; }
        this.realX += dx;
        this.realY += dy;
        this.x = (int)Math.Round(realX);
        this.y = (int)Math.Round(realY);
        if (this.realX > velocityX.dimensions[0]) { this.realX = velocityX.dimensions[0] - 1; }
        if (this.realY > velocityY.dimensions[1]) { this.realY = velocityY.dimensions[1] - 1; }
        if (this.realX < 0) { this.realX = 0; }
        if (this.realY < 0) { this.realY = 0; }
        UnityEngine.Debug.Log((this.realX, this.realY));
    }
}
[System.Serializable]
public class CollidableCell : SimulationObject
{
    public Solver2D.Boundary collidableFaces;
    public bool currentlyInSolver = false;
    public CollidableCell(int x, int y, int width, int height, Solver2D.Boundary faces, UnityEngine.Color debugColor)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.debugColor = debugColor;
        this.collidableFaces = faces;
    }
}