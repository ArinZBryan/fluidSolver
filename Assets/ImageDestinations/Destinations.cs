using UnityEngine;
using UnityEngine.EventSystems;

public class Destinations
{
    public enum FileFormat
    {
        PNG, JPG, EXA, TGA, GIF, MP4, MOV, NONE
    }

    public class Viewport : IImageDestination
    {
        Texture2D texture;
        int imageWidth, imageHeight;
        public Viewport() 
        {

        }
        public void setImage(RenderTexture img)
        {
            if (texture == null)
            {
                texture = new Texture2D(img.width, img.height, TextureFormat.RGBA32, 1, true);
            }
            Graphics.CopyTexture(img, texture);
            imageWidth = img.width;
            imageHeight = img.height;
        }
        public void init(string fpath, Destinations.FileFormat format)
        {

        }
        public string destroy()
        {
            return "";
        }
        public void renderImageNow()
        {
            Graphics.DrawTexture(new Rect(0, 0, imageWidth, imageHeight), texture);
        }
    }
}