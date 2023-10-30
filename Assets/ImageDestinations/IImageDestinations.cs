using UnityEngine;

public interface IImageDestination
{
    public void setImage(RenderTexture img);
    public string destroy();
    public int lifetimeRemaining { get; }
}