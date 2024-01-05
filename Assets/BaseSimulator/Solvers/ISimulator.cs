using UnityEngine;

interface ISimulator
{
    public RenderTexture getCurrentTexture();
    public RenderTexture getNextTexture();
    public RenderTexture getGurrentExportableTexture();
    public int getScale();
    public int setScale(int scale);
    public Vector2Int getSimulationSize();
}