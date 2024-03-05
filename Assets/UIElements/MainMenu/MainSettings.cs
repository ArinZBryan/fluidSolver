using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MainSettings : Menu
{
    public string ffmpegPath = "./Assets/ffmpeg.exe";

    private void Start()
    {
        // Define values for inherited fields
        boundKey = KeyCode.Escape;
        menuManager = GameObject.Find("Canvas").GetComponent<MenuManager>();

        // Add functionality to the menu here
        Button? maybe_restart_button = (Button?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/simulation_settings/action_restart");
        if (maybe_restart_button == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else
        {
            maybe_restart_button.clicked += () =>
            {
                Debug.Log("Resetting Simulation");
                menuManager.resultDispatcher.makeSimulatorFromSettings();
            };
        }

        Button? maybe_load_button = (Button?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/savefile_settings/action_load_file");
        if (maybe_load_button == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else
        {
            maybe_load_button.clicked += () =>
            {
                Debug.Log("Loading File");
                TextField? maybe_file_path = (TextField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/savefile_settings/file_path");
                if (maybe_file_path == null) { Debug.LogError("An error occured while connecting to the UI"); }
                else
                {
                    menuManager.resultDispatcher.loadSaveFile(maybe_file_path.value);
                    initialiseMenuValues();
                }
            };
        }

        Button? maybe_start_save_button = (Button?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/savefile_settings/action_save_file_start");
        if (maybe_start_save_button == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else
        {
            maybe_start_save_button.clicked += () =>
            {
                Debug.Log("Beginning Playback Recording");
                menuManager.resultDispatcher.beginRecording();
            };
        }

        Button? maybe_end_save_button = (Button?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/savefile_settings/action_save_file_end");
        if (maybe_end_save_button == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else
        {
            maybe_end_save_button.clicked += () =>
            {
                Debug.Log("Ending Playback Recording");
                TextField? maybe_file_path = (TextField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/savefile_settings/file_path");
                if (maybe_file_path == null) { Debug.LogError("An error occured while connecting to the UI"); }
                else
                {
                    menuManager.resultDispatcher.stopRecording();
                }
            };
        }

        Slider? maybe_mouse_density = (Slider?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/interaction_settings/mouse_density");
        if (maybe_mouse_density == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else { maybe_mouse_density.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.drawValue = e.newValue; }); }

        Slider? maybe_mouse_force = (Slider?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/interaction_settings/mouse_force");
        if (maybe_mouse_force == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else { maybe_mouse_force.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.force = e.newValue; }); }

        /*
         * This currently does not work, as the feature has not been implemented yet
         * When it has (if it has), this will need to be uncommented
        SliderInt? maybe_mouse_brush_size = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/interaction_settings/mouse_brush_size");
        if (maybe_mouse_brush_size == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_mouse_brush_size.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.penSize = e.newValue; }); }
        */

        Button? maybe_export_button = (Button?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/export_settings/action_begin_export");
        if (maybe_export_button == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else
        {
            maybe_export_button.clicked += () =>
            {
                Debug.Log("Beginning Export");
                TextField? maybe_file_folder = (TextField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/export_settings/file_folder_path");
                if (maybe_file_folder == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                TextField? maybe_file_name = (TextField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/export_settings/file_name");
                if (maybe_file_name == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                SliderInt? maybe_file_time = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/export_settings/recording_time");
                if (maybe_file_name == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                DropdownField? maybe_file_type = (DropdownField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/export_settings/file_format");
                if (maybe_file_type == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                SliderInt? maybe_frame_rate = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/simulation_settings/tick_rate");
                if (maybe_frame_rate == null) { Debug.LogError("An error occured while connecting to the UI"); return; }

                int lifetime = maybe_file_time.value;
                if (maybe_file_time.value == 0) { lifetime = int.MaxValue; }

                switch (maybe_file_type.value)
                {
                    case "PNG":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Image(maybe_file_folder.value, maybe_file_name.value, Destinations.FileFormat.PNG, lifetime)
                        );
                        break;
                    case "JPEG":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Image(maybe_file_folder.value, maybe_file_name.value, Destinations.FileFormat.JPG, lifetime)
                        );
                        break;
                    case "TGA":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Image(maybe_file_folder.value, maybe_file_name.value, Destinations.FileFormat.TGA, lifetime)
                        );
                        break;
                    case "GIF":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Video(maybe_file_folder.value, maybe_file_name.value, maybe_frame_rate.value, lifetime, Destinations.FileFormat.GIF, ffmpegPath)
                        );
                        break;
                    case "MP4":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Video(maybe_file_folder.value, maybe_file_name.value, maybe_frame_rate.value, lifetime, Destinations.FileFormat.MP4, ffmpegPath)
                        );
                        break;
                    case "MOV":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Video(maybe_file_folder.value, maybe_file_name.value, maybe_frame_rate.value, lifetime, Destinations.FileFormat.MOV, ffmpegPath)
                        );
                        break;
                    default:
                        Debug.LogError("Invalid File Type");
                        return;

                }

            };
        }
    
        Button? maybe_physObj_menu = (Button?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/action_open_physObj_settings");
        if (maybe_physObj_menu == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else
        {
            maybe_physObj_menu.clicked += () =>
            {
                Debug.Log("Opening PhysObj Menu");
                menuManager.menus.OfType<PhysObjSettings>().First().Open();
            };
        }
    }

    void initialiseMenuValues()
    {
        var (diffusion_rate, viscosity, sim_delta_time, N) = menuManager.resultDispatcher.simulator.solver.getConstants();
        SliderInt? maybe_field_size = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/simulation_settings/field_size");
        if (maybe_field_size == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_field_size.value = N; }
        SliderInt? maybe_tick_rate = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/simulation_settings/tick_rate");
        if (maybe_tick_rate == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_tick_rate.value = (int)(1f / sim_delta_time) + 1; } //This is a wierd bug, and I can't be bothered to fix that it consistantly reports 1 less than it should be
        Slider? maybe_fluid_viscosity = (Slider?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/simulation_settings/fluid_viscosity");
        if (maybe_fluid_viscosity == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_fluid_viscosity.value = viscosity; }
        Slider? maybe_fluid_diffusion_rate = (Slider?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/simulation_settings/fluid_diffusion_rate");
        if (maybe_fluid_diffusion_rate == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_fluid_diffusion_rate.value = diffusion_rate; }
        Slider? maybe_mouse_density = (Slider?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/interaction_settings/mouse_density");
        if (maybe_mouse_density == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_mouse_density.value = menuManager.resultDispatcher.simulator.drawValue; }
        Slider? maybe_mouse_force = (Slider?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/interaction_settings/mouse_force");
        if (maybe_mouse_force == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_mouse_force.value = menuManager.resultDispatcher.simulator.force; }
        SliderInt? maybe_mouse_brush_size = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/interaction_settings/mouse_brush_size");
        if (maybe_mouse_brush_size == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else { maybe_mouse_brush_size.value = menuManager.resultDispatcher.simulator.penSize; }
    }

    public override void Open()
    {
        base.Open();
        initialiseMenuValues();
        menuManager.resultDispatcher.gameObject.SetActive(false);
    }
    public override void Close()
    {
        base.Close();
        menuManager.resultDispatcher.gameObject.SetActive(true);
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
 *      |-> Button action_open_physObj_settings
 */
