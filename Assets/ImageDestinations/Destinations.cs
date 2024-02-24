using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.UIElements;


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
        public Viewport(RawImage viewport, int texSize)
        {
            rawImage = viewport;
            texture = new RenderTexture(texSize, texSize, 0);
        }
        public void setImage(Texture2D img)
        {
            Graphics.Blit(img, texture);
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
        Destinations.FileFormat fmt;
        string folder, name;
        List<byte[]> unsavedImages = new List<byte[]>();
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public ImageSequence(string folder, string name, Destinations.FileFormat format)
        {
            this.folder = folder;
            this.name = name;
            this.fmt = format;
        }

        public void setImage(Texture2D img)
        {
            switch (fmt)
            {
                case FileFormat.PNG: unsavedImages.Add(img.EncodeToPNG()); break;
                case FileFormat.JPG: unsavedImages.Add(img.EncodeToJPG()); break;
                case FileFormat.TGA: unsavedImages.Add(img.EncodeToTGA()); break;
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
        Destinations.FileFormat fmt;
        string folder, name;
        List<byte[]> unsavedImages = new List<byte[]>();
        public int lifetimeRemaining { get; set; } = 0;
        public TimedImageSequence(string folder, string name, Destinations.FileFormat format, int lifetime)
        {
            this.folder = folder;
            this.name = name;
            this.fmt = format;
            lifetimeRemaining = lifetime + 1;
        }

        public void setImage(Texture2D img)
        {
            if (lifetimeRemaining <= 0) { return; }
            

            switch (fmt)
            {
                case FileFormat.PNG: unsavedImages.Add(img.EncodeToPNG()); break;
                case FileFormat.JPG: unsavedImages.Add(img.EncodeToJPG()); break;
                case FileFormat.TGA: unsavedImages.Add(img.EncodeToTGA()); break;
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
        Destinations.FileFormat fmt;
        string folder, name;

        public int lifetimeRemaining { get; set; } = 2;

        public Image(string folder, string name, Destinations.FileFormat format)
        {
            this.folder = folder;
            this.name = name;
            this.fmt = format;
        }

        public void setImage(Texture2D img)
        {

            string path = folder + name;

            switch (fmt)
            {
                case FileFormat.PNG: File.WriteAllBytesAsync(path + ".png", img.EncodeToPNG()); break;
                case FileFormat.JPG: File.WriteAllBytesAsync(path + ".jpg", img.EncodeToJPG()); break;
                case FileFormat.TGA: File.WriteAllBytesAsync(path + ".tga", img.EncodeToTGA()); break;
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
        Destinations.FileFormat fmt;
        string folder, name;
        List<Texture2D> unsavedImages = new List<Texture2D>();
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public DeferredImageSequence(string folder, string name, Destinations.FileFormat format)
        {
            this.folder = folder;
            this.name = name;
            this.fmt = format;
        }

        public void setImage(Texture2D img)
        {

            unsavedImages.Add(img);
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
        Destinations.FileFormat fmt;
        int frameRate;
        string folder, filename, ffmpegPath;
        List<Texture2D> unsavedImages = new List<Texture2D>();
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public Video(string Folder, string Filename, int Framerate, Destinations.FileFormat Format, string ffmpegPath)
        {
            this.folder = Folder;
            this.filename = Filename;
            this.fmt = Format;
            this.frameRate = Framerate;
            this.ffmpegPath = ffmpegPath;
        }

        public void setImage(Texture2D img)
        {
            unsavedImages.Add(img);
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
        Destinations.FileFormat fmt;
        int frameRate;
        string folder, filename, ffmpegPath;
        List<Texture2D> unsavedImages = new List<Texture2D>();
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

        public void setImage(Texture2D img)
        {
            if (lifetimeRemaining <= 0) { return; }

            unsavedImages.Add(img);

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