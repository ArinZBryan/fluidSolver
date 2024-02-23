using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
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
                default: UnityEngine.Debug.LogError("Cannot Use Fileformat with ImageSequence"); break;
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
                    default: UnityEngine.Debug.LogError("Why? You changed the file format during execution, and now it's all fucked. i_i"); break;
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
            if (lifetimeRemaining <= 0) { return; }
            
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
                default: UnityEngine.Debug.LogError("Cannot Use Fileformat with ImageSequence"); break;
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
                    default: UnityEngine.Debug.LogError("Why? You changed the file format during execution, and now it's all fucked. i_i"); break;
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
                default: UnityEngine.Debug.LogError("Cannot Use Fileformat with ImageSequence"); break;
            }
            lifetimeRemaining--;

        }
        public string destroy()
        {
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
                        UnityEngine.Debug.LogError("Why? You changed the file format during execution, and now it's all fucked. i_i");
                        image = new byte[1];
                        break;
                }
                File.WriteAllBytes(path, image);
            }
            return folder;
        }
    }
    public class Video : IImageDestination
    {
        Texture2D texture;
        int imageWidth, imageHeight;
        Destinations.FileFormat fmt;
        int frameRate;
        string folder, filename, ffmpegPath;
        List<Texture2D> unsavedImages = new List<Texture2D>();
        RenderTexture tempRT;
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public Video(string Folder, string Filename, int Framerate, Destinations.FileFormat Format, string ffmpegPath)
        {
            this.folder = Folder;
            this.filename = Filename;
            this.fmt = Format;
            this.frameRate = Framerate;
            this.ffmpegPath = ffmpegPath;
        }

        public void setImage(RenderTexture img)
        {

            Texture2D texture = new Texture2D(img.width, img.height, TextureFormat.RGBA32, 1, true);

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
            string format = "";
            switch (fmt)
            {
                case FileFormat.GIF:
                    format = ".gif";
                    break;
                case FileFormat.MP4:
                    format = ".mp4";
                    break;
                case FileFormat.MOV:
                    format = ".mov";
                    break;
                default:
                    UnityEngine.Debug.Log("You can't use this format for a video you silly goose");
                    return "";
            }

            if (Directory.Exists(folder + "\\imgsequence")) Directory.Delete(folder + "\\imgsequence", true);
            if (File.Exists(folder + "\\" + filename + format)) File.Delete(folder + "\\" + filename + format);
            
            Directory.CreateDirectory(folder + "\\imgsequence");
            string path = folder + "\\imgsequence\\"+ filename;
            for (int imageIndex = 1; imageIndex < unsavedImages.Count; imageIndex++)
            {
                byte[]  image = unsavedImages[imageIndex].EncodeToPNG();
                File.WriteAllBytes(path + (imageIndex).ToString().PadLeft(4, '0') + ".png", image);
            }
            
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = ffmpegPath;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;     //For some reason FFMPEG writes all its logs to the error stream, don't ask me why
            startInfo.WorkingDirectory = folder + "\\imgsequence\\";
            startInfo.Arguments = $"-framerate {frameRate} -i {filename}%04d.png -pix_fmt yuv420p {folder + "\\imgsequence\\" + filename + format}";
            
            Process ffmpegProcess = Process.Start(startInfo);
            string ffmpegResult = ffmpegProcess.StandardError.ReadToEnd();
            
            if (!ffmpegProcess.WaitForExit(5000)) { UnityEngine.Debug.Log("FFMPEG process was killed for taking too long. Set a higher timout if this is in error."); }
            
            UnityEngine.Debug.Log(ffmpegResult);
            UnityEngine.Debug.Log("Process Exited");
            
            File.Move(folder + "\\imgsequence\\" + filename + format, folder + "\\" + filename + format);
            Directory.Delete(folder + "\\imgsequence", true);
            
            return folder;
        }
    }
    public class TimedVideo : IImageDestination
    {
        Texture2D texture;
        int imageWidth, imageHeight;
        Destinations.FileFormat fmt;
        int frameRate;
        string folder, filename, ffmpegPath;
        List<Texture2D> unsavedImages = new List<Texture2D>();
        RenderTexture tempRT;
        int time;
        public int lifetimeRemaining { get; set; } = 0;
        public TimedVideo(string Folder, string Filename, int Framerate, int Time, Destinations.FileFormat Format, string ffmpegPath)
        {
            this.folder = Folder;
            this.filename = Filename;
            this.fmt = Format;
            this.frameRate = Framerate;
            this.ffmpegPath = ffmpegPath;
            this.lifetimeRemaining = Time + 1;
        }

        public void setImage(RenderTexture img)
        {
            if (lifetimeRemaining <= 0) { return; }

            Texture2D texture = new Texture2D(img.width, img.height, TextureFormat.RGBA32, 1, true);

            //This might be a bit slow;
            tempRT = RenderTexture.active;
            RenderTexture.active = img;
            texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
            texture.Apply();
            RenderTexture.active = tempRT;

            imageWidth = img.width;
            imageHeight = img.height;
            unsavedImages.Add(texture);

            lifetimeRemaining--;
        }
        public string destroy()
        {
            string format = "";
            switch (fmt)
            {
                case FileFormat.GIF:
                    format = ".gif";
                    break;
                case FileFormat.MP4:
                    format = ".mp4";
                    break;
                case FileFormat.MOV:
                    format = ".mov";
                    break;
                default:
                    UnityEngine.Debug.Log("You can't use this format for a video you silly goose");
                    return "";
            }

            if (Directory.Exists(folder + "\\imgsequence")) Directory.Delete(folder + "\\imgsequence", true);
            if (File.Exists(folder + "\\" + filename + format)) File.Delete(folder + "\\" + filename + format);

            Directory.CreateDirectory(folder + "\\imgsequence");
            string path = folder + "\\imgsequence\\" + filename;
            for (int imageIndex = 1; imageIndex < unsavedImages.Count; imageIndex++)
            {
                byte[] image = unsavedImages[imageIndex].EncodeToPNG();
                File.WriteAllBytes(path + (imageIndex).ToString().PadLeft(4, '0') + ".png", image);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = ffmpegPath;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;     //For some reason FFMPEG writes all its logs to the error stream, don't ask me why
            startInfo.WorkingDirectory = folder + "\\imgsequence\\";
            startInfo.Arguments = $"-framerate {frameRate} -i {filename}%04d.png -pix_fmt yuv420p {folder + "\\imgsequence\\" + filename + format}";

            Process ffmpegProcess = Process.Start(startInfo);
            string ffmpegResult = ffmpegProcess.StandardError.ReadToEnd();

            if (!ffmpegProcess.WaitForExit(5000)) { UnityEngine.Debug.Log("FFMPEG process was killed for taking too long. Set a higher timout if this is in error."); }

            UnityEngine.Debug.Log(ffmpegResult);
            UnityEngine.Debug.Log("Process Exited");

            File.Move(folder + "\\imgsequence\\" + filename + format, folder + "\\" + filename + format);
            Directory.Delete(folder + "\\imgsequence", true);

            return folder;
        }
    }
}