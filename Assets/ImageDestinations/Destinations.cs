using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class Destinations
{
    public enum FileFormat
    {
        PNG, JPG, TGA, GIF, MP4, MOV, NONE
    }

    public class Viewport : IImageDestination
    {
        Texture2D texture;
        int imageWidth, imageHeight;
        public int lifetimeRemaining { get; set; } = int.MaxValue;
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
        public string destroy()
        {
            return "";
        }
        public void renderImageNow()
        {
            Graphics.DrawTexture(new Rect(0, 0, imageWidth, imageHeight), texture);
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
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public ImageSequence(string folder, string name, Destinations.FileFormat format)
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
            switch (fmt)
            {
                case FileFormat.PNG: unsavedImages.Add(texture.EncodeToPNG()); break;
                case FileFormat.JPG: unsavedImages.Add(texture.EncodeToJPG()); break;
                case FileFormat.TGA: unsavedImages.Add(texture.EncodeToTGA()); break;
                default: Debug.LogError("Cannot Use Fileformat with ImageSequence"); break;
            }
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
    public class TimedImageSequence : IImageDestination
    {
        Texture2D texture;
        int imageWidth, imageHeight;
        Destinations.FileFormat fmt;
        string folder, name;
        List<byte[]> unsavedImages = new List<byte[]>();
        RenderTexture tempRT;
        public int lifetimeRemaining { get; set; } = 0;
        public TimedImageSequence(string folder, string name, Destinations.FileFormat format, int lifetime)
        {
            this.folder = folder;
            this.name = name;
            this.fmt = format;
            lifetimeRemaining = lifetime + 1;
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
    public class Image : IImageDestination
    {
        Texture2D texture;
        Destinations.FileFormat fmt;
        string folder, name;
        RenderTexture tempRT;

        public int lifetimeRemaining { get; set; } = 2;

        public Image(string folder, string name, Destinations.FileFormat format)
        {
            this.folder = folder;
            this.name = name;
            this.fmt = format;
        }

        public void setImage(RenderTexture img)
        {

			texture = new Texture2D(img.width, img.height, TextureFormat.RGBA32, 1, true);
            string path = folder + name;
            //This might be a bit slow;
            tempRT = RenderTexture.active;
			RenderTexture.active = img;
			texture.ReadPixels(new Rect(0, 0, img.width, img.height), 0, 0);
			texture.Apply();
			RenderTexture.active = tempRT;

			switch (fmt)
			{
				case FileFormat.PNG: File.WriteAllBytesAsync(path + ".png", texture.EncodeToPNG()); break;
				case FileFormat.JPG: File.WriteAllBytesAsync(path + ".jpg", texture.EncodeToJPG()); break;
				case FileFormat.TGA: File.WriteAllBytesAsync(path + ".tga", texture.EncodeToTGA()); break;
				default: Debug.LogError("Cannot Use Fileformat with ImageSequence"); break;
			}
            lifetimeRemaining--;
            
        }
        public string destroy()
        {
            return folder;
        }
    }

}