using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsPanel : MonoBehaviour
{
    bool showSettings = false;
    public GameObject settingsPanel;
    public GameObject simulation;
    public VisualElement rootElement;

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

            if (showSettings)
            {
                Button? maybe_restart_button = (Button?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/simulation_settings/action_restart");
                if (maybe_restart_button == null) { Debug.LogError("An error occured while connecting to the UI"); }
                else {
                    maybe_restart_button.clicked += () =>
                    {
                        Debug.Log("Resetting Simulation");
                        simulation.GetComponent<ResultDispatcher>().makeSimulatorFromSettings();
                    };
                }
                
                Button? maybe_load_button = (Button?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/savefile_settings/action_load_file");
                if (maybe_load_button == null) { Debug.LogError("An error occured while connecting to the UI"); }
                else {
                    maybe_load_button.clicked += () =>
                    {
                        Debug.Log("Loading File");
                        TextField? maybe_file_path = (TextField?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/savefile_settings/file_path");
                        if (maybe_file_path == null) { Debug.LogError("An error occured while connecting to the UI"); }
                        else
                        {
                            simulation.GetComponent<ResultDispatcher>().loadSaveFile(maybe_file_path.value);
                        }
                    };
                }
                
                Button? maybe_start_save_button = (Button?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/savefile_settings/action_save_file_start");
                if (maybe_start_save_button == null) { Debug.LogError("An error occured while connecting to the UI"); }
                else {
                    maybe_start_save_button.clicked += () =>
                    {
                        Debug.Log("Beginning Playback Recording");
                        simulation.GetComponent<ResultDispatcher>().beginRecording();
                    };
                }
                
                Button? maybe_end_save_button = (Button?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/savefile_settings/action_save_file_end");
                if (maybe_end_save_button == null) { Debug.LogError("An error occured while connecting to the UI"); }
                else {
                    maybe_end_save_button.clicked += () =>
                    {
                        Debug.Log("Ending Playback Recording");
                        TextField? maybe_file_path = (TextField?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/savefile_settings/file_path");
                        if (maybe_file_path == null) { Debug.LogError("An error occured while connecting to the UI"); }
                        else
                        {
                            simulation.GetComponent<ResultDispatcher>().stopRecording();
                        }
                    };
                }
            }
        }
    }
#nullable enable
    public VisualElement? getElementByRelativeNamePath(VisualElement rootElement, string path)
    {
        for (int i = 0; i < rootElement.childCount; i++)
        {
            VisualElement child = rootElement[i];
            if (child.name == path)
            {
                return child;
            }
            else
            {
                if (child.childCount == 0)
                {
                    continue;
                }
                var newPathArr = path.Split('/').Skip(1);
                if (newPathArr.Count() == 0)
                {
                    continue;
                }
                var newPath = newPathArr.Count() > 1 ? newPathArr.Aggregate((a, b) => a + "/" + b) : newPathArr.First();
                VisualElement? result = getElementByRelativeNamePath(child, newPath);
                if (result != null)
                {
                    return result;
                }
            }
        }
        return null;
    }
    public VisualElement? getElementByRelativeNamePathLogged(VisualElement? rootElement, string path)
    {
        if (rootElement == null) { return null; }
        var elem = getElementByRelativeNamePath(rootElement, path);
        if (elem != null) Debug.Log("Found Element: " + elem.name + " at: " + path);
        else Debug.Log("Element not found at: " + path);
        return elem;
    }
#nullable disable
    public VisualElement getRootElement()
    {
        settingsPanel.SetActive(true);
        rootElement = settingsPanel.GetComponent<UIDocument>().rootVisualElement;
        settingsPanel.SetActive(showSettings);
        return rootElement;
    }
}

/* root
 * |-> padding
 * |-> scroll_menu
 *      |-> simulation_settings
 *      |   |-> SliderInt field_size
 *      |   |-> SliderInt tick_rate
 *      |   |-> Slider fluid_viscosity
 *      |   |-> Slider fluid_diffusion_rate
 *      |   |-> Button action_restart
 *      |
 *      |-> interaction_settings
 *      |   |-> Slider mouse_density
 *      |   |-> Slider mouse_force
 *      |   |-> SliderInt mouse_brush_size
 *      |
 *      |-> export_settings
 *      |   |-> file_format
 *      |   |-> file_folder_path
 *      |   |-> file_name
 *      |   |-> SliderInt recording_time
 *      |   |-> action_begin_export
 *      |
 *      |-> savefile_settings
 *      |   |-> file_path
 *      |   |-> SliderInt recording_time
 *      |   |-> action_load_file
 *      |   |-> action_save_file
 */
