using UnityEngine;

public interface IImageDestination
{
    public void setImage(RenderTexture img);
    public void init(string fpath, Destinations.FileFormat format);
    public string destroy();
}