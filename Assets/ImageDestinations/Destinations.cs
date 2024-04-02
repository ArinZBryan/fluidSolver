using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine;
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
        //Square blur kernel
        public Kernel? kernel { get; set; }

        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public Viewport(RawImage viewport, int texSize)
        {
            rawImage = viewport;
            texture = new RenderTexture(texSize, texSize, 0);
        }
        public void setImage(Texture2D img)
        {
            if (kernel == null)
            {
                Graphics.Blit(img, texture);
                return;
            }
            Graphics.Blit(kernel.apply(img), texture);
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
    public class Video : IImageDestination
    {
        Destinations.FileFormat fmt;
        int frameRate;
        string folder, filename, ffmpegPath;
        List<Texture2D> unsavedImages = new List<Texture2D>();
        public Kernel? kernel { get; set; }
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public Video(string Folder, string Filename, int Framerate, int lifetime, Destinations.FileFormat Format, string ffmpegPath, Kernel kernel)
        {
            this.folder = Folder;
            this.filename = Filename;
            this.fmt = Format;
            this.frameRate = Framerate;
            this.ffmpegPath = ffmpegPath;
            this.lifetimeRemaining = lifetime;
            this.kernel = kernel;
        }

        public void setImage(Texture2D img)
        {
            Texture2D tex = new Texture2D(img.width, img.height);
            Graphics.CopyTexture(img, tex);
            unsavedImages.Add(tex);
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
            GameObject.Find("Messages").GetComponent<MessageManager>().Log("Beginning Export");
            if (Directory.Exists(folder + "\\imgsequence"))
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Detected directory with same name as temporary directory, deleting...");
                Directory.Delete(folder + "\\imgsequence", true);
            }
            if (File.Exists(folder + "\\" + filename + format))
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Warn("File with same path as output file detected, deleting...");
                File.Delete(folder + "\\" + filename + format);
            }

            Directory.CreateDirectory(folder + "\\imgsequence");
            string path = folder + "\\imgsequence\\" + filename;
            for (int imageIndex = 1; imageIndex < unsavedImages.Count; imageIndex++)
            {
                if (kernel != null)
                {
                    unsavedImages[imageIndex] = kernel.apply(unsavedImages[imageIndex]);
                }
                try
                {
                    File.WriteAllBytes(path + (imageIndex).ToString().PadLeft(4, '0') + ".png", unsavedImages[imageIndex].EncodeToPNG());
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
            GameObject.Find("Messages").GetComponent<MessageManager>().Log("Finished Writing Files");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = ffmpegPath;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;     //For some reason FFMPEG writes all its logs to the error stream, don't ask me why
            startInfo.WorkingDirectory = folder + "\\imgsequence\\";
            startInfo.Arguments = $"-framerate {frameRate} -i {filename}%04d.png -pix_fmt yuv420p {folder + "\\imgsequence\\" + filename + format}";
            GameObject.Find("Messages").GetComponent<MessageManager>().Log("Starting FFmpeg");
            Process ffmpegProcess;
            try
            {
                ffmpegProcess = Process.Start(startInfo);
            } catch (Exception e)
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Error("FFMPEG failed to start. Check that the path is correct and that the file exists");
                GameObject.Find("Messages").GetComponent<MessageManager>().Error(e.ToString());
                return "";
            }
            GameObject.Find("Messages").GetComponent<MessageManager>().Log("Started FFmpeg");
            string ffmpegResult = ffmpegProcess.StandardError.ReadToEnd();

            if (!ffmpegProcess.WaitForExit(Config.getInt("ffmpeg_timeout")))
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Warn("FFMPEG process was killed for taking too long. Set a higher timeout if this is an error");
            }


            UnityEngine.Debug.Log(ffmpegResult);
            GameObject.Find("Messages").GetComponent<MessageManager>().Log("FFmpeg finished");

            File.Move(folder + "\\imgsequence\\" + filename + format, folder + "\\" + filename + format);
            Directory.Delete(folder + "\\imgsequence", true);

            return folder;
        }
    }
    public class Image : IImageDestination
    {
        Destinations.FileFormat fmt;
        string folder, name;
        List<Texture2D> unsavedImages = new List<Texture2D>();
        public Kernel? kernel { get; set; }
        public int lifetimeRemaining { get; set; } = int.MaxValue;
        public Image(string folder, string name, Destinations.FileFormat format, int lifetime, Kernel? kernel)
        {
            this.folder = folder;
            this.name = name;
            this.fmt = format;
            this.lifetimeRemaining = lifetime;
            this.kernel = kernel;
        }

        public void setImage(Texture2D img)
        {
            Texture2D tex = new Texture2D(img.width, img.height);
            Graphics.CopyTexture(img, tex);
            switch (fmt)
            {
                case FileFormat.PNG: unsavedImages.Add(tex); break;
                case FileFormat.JPG: unsavedImages.Add(tex); break;
                case FileFormat.TGA: unsavedImages.Add(tex); break;
                default:
                    GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Invalid file format used for this ImageDestination");
                    break;
            }
        }
        public string destroy()
        {
            for (int imageIndex = 1; imageIndex < unsavedImages.Count; imageIndex++)
            {
                if (kernel != null)
                {
                    unsavedImages[imageIndex] = kernel.apply(unsavedImages[imageIndex]);
                }
                string path = folder + "/" + name + (imageIndex - 1).ToString().PadLeft(4, '0');
                switch (fmt)
                {
                    case FileFormat.PNG: 
                        path += ".png";
                        saveImage(unsavedImages[imageIndex].EncodeToPNG(), path);
                        break;
                    case FileFormat.JPG: 
                        path += ".jpg";
                        saveImage(unsavedImages[imageIndex].EncodeToJPG(), path);
                        break;
                    case FileFormat.TGA: 
                        path += ".tga";
                        saveImage(unsavedImages[imageIndex].EncodeToTGA(), path);
                        break;
                    default:
                        GameObject.Find("Messages").GetComponent<MessageManager>().Error("File format changed during save process... Files may be corrupted");
                        break;
                }
            }
            return folder;
        }
        bool saveImage(byte[] img, string path)
        {
            try
            {
                File.WriteAllBytes(path, img);
                return true;
            }
            catch (ArgumentException)
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Error("Calculated file path is not valid");
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Error("Directory not found");
                return false;
            }
            catch (PathTooLongException)
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Error("Path too long");
                return false;
            }
            catch (IOException)
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Error("IO Error");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Error("Unauthorized Access");
                return false;
            }
            catch (NotSupportedException)
            {
                GameObject.Find("Messages").GetComponent<MessageManager>().Error("Path is in an invalid format");
                return false;
            }
        }
    }

}