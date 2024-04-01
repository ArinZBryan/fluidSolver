using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using UnityEngine;

public class Config
{
    public static string getRaw(string key)
    {
        string path = Application.dataPath + "/config.txt";
        string[] lines;
        try { lines = File.ReadAllLines(path); }
        catch (FileNotFoundException) { GameObject.Find("Messages").GetComponent<MessageManager>().Error("Could not find config file."); return null; }
        catch (DirectoryNotFoundException) { GameObject.Find("Messages").GetComponent<MessageManager>().Error("Could not find config file."); return null; }
        catch (IOException) { GameObject.Find("Messages").GetComponent<MessageManager>().Error("Could not read config file."); return null; }
        if (lines.Count() == 0) { GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Config file is empty."); return null; }
        var settings = new Dictionary<string, string>();
        foreach (string line in lines)
        {
            string[] parts = line.Split('=');
            if (parts.Count() != 2) { GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Config line (" + line + ") is invalid."); continue; }
            settings[parts[0]] = parts[1];
        }
        try { return settings[key]; }
        catch (KeyNotFoundException) { GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Config key (" + key + ") not found."); return null; }
    }
    public static bool getBool(string key)
    {
        var res = getRaw(key);
        if (res == "true")
        {
            return true;
        }
        else if (res == "false")
        {
            return false;
        }
        else
        {
            GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Config key (" + key + ") is not a boolean.");
            return false;
        }
    }
    public static int getInt(string key)
    {
        var res = getRaw(key);
        int resInt;
        if (int.TryParse(res, out resInt))
        {
            return resInt;
        }
        else
        {
            GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Config key (" + key + ") is not an integer.");
            return 0;
        }
    }
    public static string getString(string key)
    {
        var res = getRaw(key);
        if (res[0] != '"' && res[res.Length - 1] != '"') GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Config key (" + key + ") is not a string.");
        return res.Substring(1, res.Length - 2); //strip quotes
    }
    public static float getFloat(string key)
    {
        var res = getRaw(key);
        float resFloat;
        if (float.TryParse(res, out resFloat))
        {
            return resFloat;
        }
        else
        {
            GameObject.Find("Messages").GetComponent<MessageManager>().Warn("Config key (" + key + ") is not a float.");
            return 0;
        }
    }
}
