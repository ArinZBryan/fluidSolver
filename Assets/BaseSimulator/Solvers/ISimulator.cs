using UnityEngine;

public interface ISimulator
{
    public RenderTexture getCurrentTexture();
    public RenderTexture getNextTexture();
}