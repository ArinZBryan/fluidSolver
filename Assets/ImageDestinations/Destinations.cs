﻿using System.Collections.Generic;
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
    public class ImageSequence : IImageDestination
    {
        Texture2D texture;
        int imageWidth, imageHeight;
        Destinations.FileFormat fmt;
        string fpath = "";
        List<byte[]> unsavedImages = new List<byte[]>();
        RenderTexture tempRT;
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
        public void init(string filePath, Destinations.FileFormat format)
        {
            fmt = format;
            fpath = filePath;
        }
        public string destroy()
        {
            for (int imageIndex = 1; imageIndex < unsavedImages.Count; imageIndex++)
            {
                string path = fpath.Substring(0, fpath.Length - 4) + (imageIndex - 1).ToString().PadLeft(4, '0');
                switch (fmt)
                {
                    case FileFormat.PNG: path += ".png"; break;
                    case FileFormat.JPG: path += ".jpg"; break;
                    case FileFormat.TGA: path += ".tga"; break;
                    default: Debug.LogError("Why? You changed the file format during execution, and now it's all fucked. i_i"); break;
                }
                File.WriteAllBytes(path, unsavedImages[imageIndex]);
            }
            return fpath;
        }
    }
}