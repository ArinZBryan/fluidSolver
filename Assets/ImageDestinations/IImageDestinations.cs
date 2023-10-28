using UnityEngine;

interface IImageDestination
{
    public RenderTexture sendCurrentImage();
    public void init(string fpath, Destinations.FileFormat format);
    public string destroy();
}