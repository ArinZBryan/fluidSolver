using UnityEngine;

public class Destinations
{
    public enum FileFormat
    {
        PNG, JPG, EXA, TGA, GIF, MP4, MOV, NONE
    }

    public class Viewport : IImageDestination
    {
        RenderTexture currentImage;
        public Viewport() { }
        public RenderTexture sendCurrentImage()
        {

        }
        public void init(string fpath, Destinations.FileFormat format)
        {

        }
        public string destroy()
        {

        }
    }
}