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

    public string ffmpegPath = ".\\Assets\\ffmpeg.exe";

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
                initialiseMenuValues();

                Button? maybe_restart_button = (Button?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/simulation_settings/action_restart");
                if (maybe_restart_button == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                else
                {
                    maybe_restart_button.clicked += () =>
                    {
                        Debug.Log("Resetting Simulation");
                        simulation.GetComponent<ResultDispatcher>().makeSimulatorFromSettings();
                    };
                }

                Button? maybe_load_button = (Button?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/savefile_settings/action_load_file");
                if (maybe_load_button == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                else
                {
                    maybe_load_button.clicked += () =>
                    {
                        Debug.Log("Loading File");
                        TextField? maybe_file_path = (TextField?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/savefile_settings/file_path");
                        if (maybe_file_path == null) { Debug.LogError("An error occured while connecting to the UI"); }
                        else
                        {
                            simulation.GetComponent<ResultDispatcher>().loadSaveFile(maybe_file_path.value);
                            initialiseMenuValues();
                        }
                    };
                }

                Button? maybe_start_save_button = (Button?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/savefile_settings/action_save_file_start");
                if (maybe_start_save_button == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                else
                {
                    maybe_start_save_button.clicked += () =>
                    {
                        Debug.Log("Beginning Playback Recording");
                        simulation.GetComponent<ResultDispatcher>().beginRecording();
                    };
                }

                Button? maybe_end_save_button = (Button?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/savefile_settings/action_save_file_end");
                if (maybe_end_save_button == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                else
                {
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

                Slider? maybe_mouse_density = (Slider?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/interaction_settings/mouse_density");
                if (maybe_mouse_density == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                else { maybe_mouse_density.RegisterValueChangedCallback((e) => { simulation.GetComponent<ResultDispatcher>().simulator.drawValue = e.newValue; }); }

                Slider? maybe_mouse_force = (Slider?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/interaction_settings/mouse_force");
                if (maybe_mouse_force == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                else { maybe_mouse_force.RegisterValueChangedCallback((e) => { simulation.GetComponent<ResultDispatcher>().simulator.force = e.newValue; }); }

                /*
                 * This currently does not work, as the feature has not been implemented yet
                 * When it has (if it has), this will need to be uncommented
                SliderInt? maybe_mouse_brush_size = (SliderInt?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/interaction_settings/mouse_brush_size");
                if (maybe_mouse_brush_size == null) { Debug.LogError("An error occured while connecting to the UI"); }
                else { maybe_mouse_brush_size.RegisterValueChangedCallback((e) => { simulation.GetComponent<ResultDispatcher>().simulator.penSize = e.newValue; }); }
                */

                Button? maybe_export_button = (Button?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/export_settings/action_begin_export");
                if (maybe_export_button == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                else
                {
                    maybe_export_button.clicked += () =>
                    {
                        Debug.Log("Beginning Export");
                        TextField? maybe_file_folder = (TextField?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/export_settings/file_folder_path");
                        if (maybe_file_folder == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                        TextField? maybe_file_name = (TextField?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/export_settings/file_name");
                        if (maybe_file_name == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                        SliderInt? maybe_file_time = (SliderInt?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/export_settings/recording_time");
                        if (maybe_file_name == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                        DropdownField? maybe_file_type = (DropdownField?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/export_settings/file_format");
                        if (maybe_file_type == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                        SliderInt? maybe_frame_rate = (SliderInt?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/simulation_settings/tick_rate");
                        if (maybe_frame_rate == null) { Debug.LogError("An error occured while connecting to the UI"); return; }

                        int lifetime = maybe_file_time.value;
                        if (maybe_file_time.value == 0) { lifetime = int.MaxValue; }

                        switch (maybe_file_type.value)
                        {
                            case "PNG":
                                simulation.GetComponent<ResultDispatcher>().destinations.Add(
                                    new Destinations.Image(maybe_file_folder.value, maybe_file_name.value, Destinations.FileFormat.PNG, lifetime)
                                );
                                break;
                            case "JPEG":
                                simulation.GetComponent<ResultDispatcher>().destinations.Add(
                                    new Destinations.Image(maybe_file_folder.value, maybe_file_name.value, Destinations.FileFormat.JPG, lifetime)
                                );
                                break;
                            case "TGA":
                                simulation.GetComponent<ResultDispatcher>().destinations.Add(
                                    new Destinations.Image(maybe_file_folder.value, maybe_file_name.value, Destinations.FileFormat.TGA, lifetime)
                                );
                                break;
                            case "GIF":
                                simulation.GetComponent<ResultDispatcher>().destinations.Add(
                                    new Destinations.Video(maybe_file_folder.value, maybe_file_name.value, maybe_frame_rate.value, lifetime, Destinations.FileFormat.GIF, ffmpegPath)
                                );
                                break;
                            case "MP4":
                                simulation.GetComponent<ResultDispatcher>().destinations.Add(
                                    new Destinations.Video(maybe_file_folder.value, maybe_file_name.value, maybe_frame_rate.value, lifetime, Destinations.FileFormat.MP4, ffmpegPath)
                                );
                                break;
                            case "MOV":
                                simulation.GetComponent<ResultDispatcher>().destinations.Add(
                                    new Destinations.Video(maybe_file_folder.value, maybe_file_name.value, maybe_frame_rate.value, lifetime, Destinations.FileFormat.MOV, ffmpegPath)
                                );
                                break;
                            default:
                                Debug.LogError("Invalid File Type");
                                return;

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
    void initialiseMenuValues()
    {
        var (diffusion_rate, viscosity, sim_delta_time, N) = simulation.GetComponent<ResultDispatcher>().simulator.solver.getConstants();
        SliderInt? maybe_field_size = (SliderInt?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/simulation_settings/field_size");
        if (maybe_field_size == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_field_size.value = N; }
        SliderInt? maybe_tick_rate = (SliderInt?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/simulation_settings/tick_rate");
        if (maybe_tick_rate == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_tick_rate.value = (int)(1f / sim_delta_time) + 1; } //This is a wierd bug, and I can't be bothered to fix that it consistantly reports 1 less than it should be
        Slider? maybe_fluid_viscosity = (Slider?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/simulation_settings/fluid_viscosity");
        if (maybe_fluid_viscosity == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_fluid_viscosity.value = viscosity; }
        Slider? maybe_fluid_diffusion_rate = (Slider?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/simulation_settings/fluid_diffusion_rate");
        if (maybe_fluid_diffusion_rate == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_fluid_diffusion_rate.value = diffusion_rate; }
        Slider? maybe_mouse_density = (Slider?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/interaction_settings/mouse_density");
        if (maybe_mouse_density == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_mouse_density.value = simulation.GetComponent<ResultDispatcher>().simulator.drawValue; }
        Slider? maybe_mouse_force = (Slider?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/interaction_settings/mouse_force");
        if (maybe_mouse_force == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_mouse_force.value = simulation.GetComponent<ResultDispatcher>().simulator.force; }
        SliderInt? maybe_mouse_brush_size = (SliderInt?)getElementByRelativeNamePathLogged(getRootElement(), "root/scroll_menu/interaction_settings/mouse_brush_size");
        if (maybe_mouse_brush_size == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_mouse_brush_size.value = simulation.GetComponent<ResultDispatcher>().simulator.penSize; }
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
 *      |   |-> Dropdown file_format
 *      |   |-> TextField file_folder_path
 *      |   |-> TextField file_name
 *      |   |-> SliderInt recording_time
 *      |   |-> Button action_begin_export
 *      |
 *      |-> savefile_settings
 *      |   |-> TextField file_path
 *      |   |-> Button action_load_file
 *      |   |-> Button action_save_file_start
 *      |   |-> Button action_save_file_end
 */
