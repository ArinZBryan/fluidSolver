using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Destinations
{
    public enum FileFormat
    {
        PNG, JPG, TGA, GIF, MP4, MOV, NONE
    }

    public class Viewport : IImageDestination
    {
        RenderTexture texture;
        public RawImage rawImage;
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public Viewport(RawImage viewport)
        {
            rawImage = viewport;
        }
        public void setImage(RenderTexture img)
        {
            texture = img;
        }
        public string destroy()
        {
            return "";
        }
        public void renderImageNow()
        {
            rawImage.texture = texture;
        }
    }
    public class ImageSequence : IImageDestination
    {
        Texture2D texture;
        int imageWidth, imageHeight;
        Destinations.FileFormat fmt;
        string folder, name;
        List<byte[]> unsavedImages = new List<byte[]>();
        RenderTexture tempRT;
        public int lifetimeRemaining { get; set; } = 0;
        public ImageSequence(string folder, string name, Destinations.FileFormat format, int lifetime)
        {
            this.folder = folder;
            this.name = name;
            this.fmt = format;
            lifetimeRemaining = lifetime;
        }

        public void setImage(RenderTexture img)
        {
            if (texture == null)
            {
                texture = new Texture2D(img.width, img.height, TextureFormat.RGBA32, 1, true);
            }

            //This might be a bit slow;
            tempRT = RenderTexture.active;
            RenderTexture.active = img;
            texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
            texture.Apply();
            RenderTexture.active = tempRT;

            imageWidth = img.width;
            imageHeight = img.height;
            switch (fmt)
            {
                case FileFormat.PNG: unsavedImages.Add(texture.EncodeToPNG()); break;
                case FileFormat.JPG: unsavedImages.Add(texture.EncodeToJPG()); break;
                case FileFormat.TGA: unsavedImages.Add(texture.EncodeToTGA()); break;
                default: Debug.LogError("Cannot Use Fileformat with ImageSequence"); break;
            }
            lifetimeRemaining--;
        }
        public string destroy()
        {
            for (int imageIndex = 1; imageIndex < unsavedImages.Count; imageIndex++)
            {
                string path = folder + name + (imageIndex - 1).ToString().PadLeft(4, '0');
                switch (fmt)
                {
                    case FileFormat.PNG: path += ".png"; break;
                    case FileFormat.JPG: path += ".jpg"; break;
                    case FileFormat.TGA: path += ".tga"; break;
                    default: Debug.LogError("Why? You changed the file format during execution, and now it's all fucked. i_i"); break;
                }
                File.WriteAllBytes(path, unsavedImages[imageIndex]);
            }
            return folder;
        }
    }
    public class DeferredImageSequence : IImageDestination
    {
        Texture2D texture;
        int imageWidth, imageHeight;
        Destinations.FileFormat fmt;
        string folder, name;
        List<Texture2D> unsavedImages = new List<Texture2D>();
        RenderTexture tempRT;
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public DeferredImageSequence(string folder, string name, Destinations.FileFormat format)
        {
            this.folder = folder;
            this.name = name;
            this.fmt = format;
        }

        public void setImage(RenderTexture img)
        {
            if (texture == null)
            {
                texture = new Texture2D(img.width, img.height, TextureFormat.RGBA32, 1, true);
            }

            //This might be a bit slow;
            tempRT = RenderTexture.active;
            RenderTexture.active = img;
            texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
            texture.Apply();
            RenderTexture.active = tempRT;

            imageWidth = img.width;
            imageHeight = img.height;
            unsavedImages.Add(texture);
        }
        public string destroy()
        {
            byte[] image;
            for (int imageIndex = 1; imageIndex < unsavedImages.Count; imageIndex++)
            {
                string path = folder + name + (imageIndex - 1).ToString().PadLeft(4, '0');
                switch (fmt)
                {
                    case FileFormat.PNG: 
                        path += ".png";
                        image = ImageConversion.EncodeToPNG(unsavedImages[imageIndex]);
                        break;
                    case FileFormat.JPG:
                        path += ".jpg";
                        image = ImageConversion.EncodeToJPG(unsavedImages[imageIndex]);
                        break;
                    case FileFormat.TGA:
                        path += ".tga";
                        image = ImageConversion.EncodeToTGA(unsavedImages[imageIndex]);
                        break;
                    default: 
                        Debug.LogError("Why? You changed the file format during execution, and now it's all fucked. i_i");
                        image = new byte[1];
                        break;
                }
                File.WriteAllBytes(path, image);
            }
            return folder;
        }
    }
}