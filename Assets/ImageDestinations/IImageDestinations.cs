using UnityEngine;

public interface IImageDestination
{
    public void setImage(Texture2D img);
    public string destroy();
    public int lifetimeRemaining { get; }
    public Kernel? kernel { get; set; }
}