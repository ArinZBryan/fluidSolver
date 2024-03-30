using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using System;

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
    public class Image : IImageDestination
    {
        Destinations.FileFormat fmt;
        string folder, name;
        List<byte[]> unsavedImages = new List<byte[]>();
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public Image(string folder, string name, Destinations.FileFormat format, int lifetime)
        {
            this.folder = folder;
            this.name = name;
            this.fmt = format;
            this.lifetimeRemaining = lifetime;
        }

        public void setImage(Texture2D img)
        {
            switch (fmt)
            {
                case FileFormat.PNG: unsavedImages.Add(img.EncodeToPNG()); break;
                case FileFormat.JPG: unsavedImages.Add(img.EncodeToJPG()); break;
                case FileFormat.TGA: unsavedImages.Add(img.EncodeToTGA()); break;
                default:
                    GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Invalid file format used for this ImageDestination");
                    break;
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
                    default: 
                        GameObject.Find("Messages").GetComponent<MessageManager>().Error("File format changed during save process... Files may be corrupted");
                        break;
                }
                try
                {
                    File.WriteAllBytes(path, unsavedImages[imageIndex]);
                }
                catch (ArgumentException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("Calculated file path is not valid");
                }
                catch (DirectoryNotFoundException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("Directory not found");
                }
                catch (PathTooLongException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("Path too long");
                }
                catch (IOException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("IO Error");
                }
                catch (UnauthorizedAccessException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("Unauthorized Access");
                }
                catch (NotSupportedException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("Path is in an invalid format");
                }
            }
            return folder;
        }
    }
    public class Video : IImageDestination
    {
        Destinations.FileFormat fmt;
        int frameRate;
        string folder, filename, ffmpegPath;
        List<byte[]> unsavedImages = new List<byte[]>();
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public Video(string Folder, string Filename, int Framerate, int lifetime, Destinations.FileFormat Format, string ffmpegPath)
        {
            this.folder = Folder;
            this.filename = Filename;
            this.fmt = Format;
            this.frameRate = Framerate;
            this.ffmpegPath = ffmpegPath;
            this.lifetimeRemaining = lifetime;
        }

        public void setImage(Texture2D img)
        {
            unsavedImages.Add(img.EncodeToPNG());
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

            if (Directory.Exists(folder + "\\imgsequence"))
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Detected directory with same name as temporary directory, deleting...");
                Directory.Delete(folder + "\\imgsequence", true);
            }
            if (File.Exists(folder + "\\" + filename + format))
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Error("File with same path as output file detected, deleting...");
                File.Delete(folder + "\\" + filename + format);
            }

            Directory.CreateDirectory(folder + "\\imgsequence");
            string path = folder + "\\imgsequence\\" + filename;
            for (int imageIndex = 1; imageIndex < unsavedImages.Count; imageIndex++)
            {
                try
                {
                    File.WriteAllBytes(path + (imageIndex).ToString().PadLeft(4, '0') + ".png", unsavedImages[imageIndex]);
                }
                catch (ArgumentException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("Calculated file path is not valid");
                }
                catch (DirectoryNotFoundException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("Directory not found");
                }
                catch (PathTooLongException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("Path too long");
                }
                catch (IOException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("IO Error");
                }
                catch (UnauthorizedAccessException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("Unauthorized Access");
                }
                catch (NotSupportedException)
                {
                    GameObject.Find("Messages").GetComponent<MessageManager>().Error("Path is in an invalid format");
                }
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