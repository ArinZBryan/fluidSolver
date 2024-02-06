using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsPanel : MonoBehaviour
{
    bool showSettings = false;
    public GameObject settingsPanel;
    public GameObject simulation;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            showSettings = !showSettings;
            settingsPanel.SetActive(showSettings);
            simulation.SetActive(!showSettings);
        }
    }
}
