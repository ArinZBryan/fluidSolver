using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class MessageManager : MonoBehaviour
{
    public GameObject errorPrefab;
    public GameObject warningPrefab;
    public GameObject infoPrefab;

    public void Error(string message)
    {
        GameObject error = Instantiate(errorPrefab, transform);
        error.GetComponentInChildren<UnityEngine.UI.Text>().text = "Error: " + message;
    }
    public void Warn(string message)
    {
        GameObject warning = Instantiate(warningPrefab, transform);
        warning.GetComponentInChildren<UnityEngine.UI.Text>().text = "Warning: " + message;
    }
    public void Log(string message)
    {
        GameObject log = Instantiate(infoPrefab, transform);
        log.GetComponentInChildren<UnityEngine.UI.Text>().text = "Info: " + message;
    }
}
