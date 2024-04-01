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

        DropdownField? maybe_kernel_type = (DropdownField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/kernel_type");
        if (maybe_kernel_type == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else { maybe_kernel_type.RegisterValueChangedCallback((e) => 
            { 
                TextField? maybe_kernel_text = (TextField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/kernel_text");
                if (maybe_kernel_text == null) { Debug.LogError("An error occured while connecting to the UI"); return; }

                switch (e.newValue)
                {
                    case "Box Blur (3x3)":
                        maybe_kernel_text.value = "[[0.111,0.111,0.111],[0.111,0.111,0.111],[0.111,0.111,0.111]]";
                        break;
                    case "Box Blur (5x5)":
                        maybe_kernel_text.value = "[[0.04,0.04,0.04,0.04,0.04],[0.04,0.04,0.04,0.04,0.04],[0.04,0.04,0.04,0.04,0.04],[0.04,0.04,0.04,0.04,0.04],[0.04,0.04,0.04,0.04,0.04]]";
                        break;
                    case "Box Blur (7x7)":
                        maybe_kernel_text.value = "[[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02]]";
                        break;
                    case "Gaussian Blur (3x3)":
                        maybe_kernel_text.value = "[[0.0625,0.125,0.0625],[0.125,0.25,0.125],[0.0625,0.125,0.0625]]";
                        break;
                    case "Gaussian Blur (5x5)":
                        maybe_kernel_text.value = "[[0.003663,0.014652,0.025641,0.014652,0.003663],[0.014652,0.058608,0.095238,0.058608,0.014652],[0.025641,0.095238,0.150183,0.095238,0.025641],[0.014652,0.058608,0.095238,0.058608,0.014652],[0.003663,0.014652,0.025641,0.014652,0.003663]]";
                        break;
                    case "Gaussian Blur (7x7)":
                        maybe_kernel_text.value = "[[0.0009,0.006,0.0124,0.0187,0.0124,0.006,0.0009],[0.006,0.0406,0.0856,0.121,0.0856,0.0406,0.006],[0.0124,0.0856,0.147,0.185,0.147,0.0856,0.0124],[0.0187,0.121,0.185,0.198,0.185,0.121,0.0187],[0.0124,0.0856,0.147,0.185,0.147,0.0856,0.0124],[0.006,0.0406,0.0856,0.121,0.0856,0.0406,0.006],[0.0009,0.006,0.0124,0.0187,0.0124,0.006,0.0009]]";
                        break;
                    case "Sharpen":
                        maybe_kernel_text.value = "[[0,-1,0],[-1,5,-1],[0,-1,0]]";
                        break;
                    case "Custom":
                        maybe_kernel_text.value = "[[0,0,0],[0,1,0],[0,0,0]]";
                        break;
                    case "None":
                        maybe_kernel_text.value = "";
                        break;
                    default:
                        maybe_kernel_text.value = "";
                        return;
                } 
            });
        }
        TextField? maybe_kernel_text = (TextField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/kernel_text");
        if (maybe_kernel_text == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else { maybe_kernel_text.RegisterValueChangedCallback((e) => 
            {
                DropdownField? maybe_kernel_type = (DropdownField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/kernel_type");
                if (maybe_kernel_type == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                switch (maybe_kernel_text.value)
                {
                    case "[[0.111,0.111,0.111],[0.111,0.111,0.111],[0.111,0.111,0.111]]":
                    case "[[0.04,0.04,0.04,0.04,0.04],[0.04,0.04,0.04,0.04,0.04],[0.04,0.04,0.04,0.04,0.04],[0.04,0.04,0.04,0.04,0.04],[0.04,0.04,0.04,0.04,0.04]]":
                    case "[[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02],[0.02,0.02,0.02,0.02,0.02,0.02,0.02]]":
                    case "[[0.0625,0.125,0.0625],[0.125,0.25,0.125],[0.0625,0.125,0.0625]]":
                    case "[[0.003663,0.014652,0.025641,0.014652,0.003663],[0.014652,0.058608,0.095238,0.058608,0.014652],[0.025641,0.095238,0.150183,0.095238,0.025641],[0.014652,0.058608,0.095238,0.058608,0.014652],[0.003663,0.014652,0.025641,0.014652,0.003663]]":
                    case "[[0.0009,0.006,0.0124,0.0187,0.0124,0.006,0.0009],[0.006,0.0406,0.0856,0.121,0.0856,0.0406,0.006],[0.0124,0.0856,0.147,0.185,0.147,0.0856,0.0124],[0.0187,0.121,0.185,0.198,0.185,0.121,0.0187],[0.0124,0.0856,0.147,0.185,0.147,0.0856,0.0124],[0.006,0.0406,0.0856,0.121,0.0856,0.0406,0.006],[0.0009,0.006,0.0124,0.0187,0.0124,0.006,0.0009]]":
                    case "[[0,-1,0],[-1,5,-1],[0,-1,0]]":
                    case "":
                        break;
                    default:
                        maybe_kernel_type.value = "Custom";
                        break;
                }
            });
        }

        Toggle? maybe_toggle_viewport = (Toggle?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/toggle_viewport");
        if (maybe_toggle_viewport == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else { maybe_toggle_viewport.RegisterValueChangedCallback((e) => 
            {
                if (e.newValue == false) { menuManager.resultDispatcher.destinations.OfType<Destinations.Viewport>().First().kernel = null; }
                else
                {
                    Kernel kernel;
                    DropdownField? maybe_kernel_type = (DropdownField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/kernel_type");
                    if (maybe_kernel_type == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                    switch (maybe_kernel_type.value)
                    {
                        case "Box Blur (3x3)": kernel = Kernel.boxBlur3x3; break;
                        case "Box Blur (5x5)": kernel = Kernel.boxBlur5x5; break;
                        case "Box Blur (7x7)": kernel = Kernel.boxBlur7x7; break;
                        case "Gaussian Blur (3x3)": kernel = Kernel.gaussianBlur3x3; break;
                        case "Gaussian Blur (5x5)": kernel = Kernel.gaussianBlur5x5; break;
                        case "Gaussian Blur (7x7)": kernel = Kernel.gaussianBlur7x7; break;
                        case "Sharpen": kernel = Kernel.sharpen3x3; break;
                        case "None": kernel = null; break;
                        default:
                            TextField? maybe_kernel_text = (TextField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/kernel_text");
                            if (maybe_kernel_text == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                            kernel = new Kernel(maybe_kernel_text.value);
                            break;
                    }
                    menuManager.resultDispatcher.destinations.OfType<Destinations.Viewport>().First().kernel = kernel;
                }
            });
        }

        Toggle? maybe_toggle_export = (Toggle?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/toggle_export");
        if (maybe_toggle_export == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        else
        {
            maybe_toggle_export.RegisterValueChangedCallback((e) =>
            {
                if (e.newValue == false) { menuManager.resultDispatcher.destinations.Where(x => !(x is Destinations.Viewport)).First().kernel = null; }
                else
                {
                    Kernel kernel;
                    DropdownField? maybe_kernel_type = (DropdownField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/kernel_type");
                    if (maybe_kernel_type == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                    switch (maybe_kernel_type.value)
                    {
                        case "Box Blur (3x3)": kernel = Kernel.boxBlur3x3; break;
                        case "Box Blur (5x5)": kernel = Kernel.boxBlur5x5; break;
                        case "Box Blur (7x7)": kernel = Kernel.boxBlur7x7; break;
                        case "Gaussian Blur (3x3)": kernel = Kernel.gaussianBlur3x3; break;
                        case "Gaussian Blur (5x5)": kernel = Kernel.gaussianBlur5x5; break;
                        case "Gaussian Blur (7x7)": kernel = Kernel.gaussianBlur7x7; break;
                        case "Sharpen": kernel = Kernel.sharpen3x3; break;
                        case "None": kernel = null; break;
                        default:
                            TextField? maybe_kernel_text = (TextField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/kernel_text");
                            if (maybe_kernel_text == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
                            kernel = new Kernel(maybe_kernel_text.value);
                            break;
                    }
                    var targets = menuManager.resultDispatcher.destinations.Where(x => !(x is Destinations.Viewport));
                    foreach (var target in targets) { target.kernel = kernel; }
                }
            });
        }

        
        SliderInt? maybe_fluid_col_r = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/fluid_color/red");
        SliderInt? maybe_fluid_col_g = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/fluid_color/green");
        SliderInt? maybe_fluid_col_b = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/fluid_color/blue");
        SliderInt? maybe_fluid_col_a = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/fluid_color/alpha");
        if (maybe_fluid_col_r == null || maybe_fluid_col_g == null || maybe_fluid_col_b == null | maybe_fluid_col_a == null ) { Debug.LogError("An error occured while connecting to the UI"); return; }
        maybe_fluid_col_r.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.fluidColor.r = (byte)e.newValue; });
        maybe_fluid_col_g.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.fluidColor.g = (byte)e.newValue; });
        maybe_fluid_col_b.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.fluidColor.b = (byte)e.newValue; });
        maybe_fluid_col_a.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.fluidColor.a = (byte)e.newValue; });

        SliderInt? maybe_base_col_r = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/base_color/red");
        SliderInt? maybe_base_col_g = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/base_color/green");
        SliderInt? maybe_base_col_b = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/base_color/blue");
        SliderInt? maybe_base_col_a = (SliderInt?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/scroll_menu/kernel_settings/base_color/alpha");
        if (maybe_base_col_r == null || maybe_base_col_g == null || maybe_base_col_b == null | maybe_base_col_a == null) { Debug.LogError("An error occured while connecting to the UI"); return; }
        maybe_base_col_r.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.baseColor.r = (byte)e.newValue; });
        maybe_base_col_g.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.baseColor.g = (byte)e.newValue; });
        maybe_base_col_b.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.baseColor.b = (byte)e.newValue; });
        maybe_base_col_a.RegisterValueChangedCallback((e) => { menuManager.resultDispatcher.simulator.baseColor.a = (byte)e.newValue; });
        
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

                if (maybe_file_folder.value.EndsWith('/') || maybe_file_folder.value.EndsWith("\\")) { maybe_file_folder.value = maybe_file_folder.value.Substring(0, maybe_file_folder.value.Length - 1); }

                int lifetime = maybe_file_time.value;
                if (maybe_file_time.value == 0) { lifetime = int.MaxValue; }

                switch (maybe_file_type.value)
                {
                    case "PNG":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Image(maybe_file_folder.value, maybe_file_name.value, Destinations.FileFormat.PNG, lifetime, null)
                        );
                        break;
                    case "JPEG":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Image(maybe_file_folder.value, maybe_file_name.value, Destinations.FileFormat.JPG, lifetime, null)
                        );
                        break;
                    case "TGA":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Image(maybe_file_folder.value, maybe_file_name.value, Destinations.FileFormat.TGA, lifetime, null)
                        );
                        break;
                    case "GIF":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Video(maybe_file_folder.value, maybe_file_name.value, maybe_frame_rate.value, lifetime, Destinations.FileFormat.GIF, ffmpegPath, null)
                        );
                        break;
                    case "MP4":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Video(maybe_file_folder.value, maybe_file_name.value, maybe_frame_rate.value, lifetime, Destinations.FileFormat.MP4, ffmpegPath, null)
                        );
                        break;
                    case "MOV":
                        menuManager.resultDispatcher.destinations.Add(
                            new Destinations.Video(maybe_file_folder.value, maybe_file_name.value, maybe_frame_rate.value, lifetime, Destinations.FileFormat.MOV, ffmpegPath, null)
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
 *      |-> kernel_settings
 *      |   |-> Dropdown kernel_type
 *      |   |-> TextField kernel_text
 *      |   |-> Toggle toggle_viewport
 *      |   |-> Toggle toggle_export
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
