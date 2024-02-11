using System.Collections.Generic;
using System.IO;
using System.Linq;
//using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ResultDispatcher : MonoBehaviour
{
    public GameObject simulatorPrefab;
    public SettingsPanel settings;
    GameObject simulatorGameObject;
    FluidSimulator simulator;
    public List<IImageDestination> destinations = new List<IImageDestination>();
    bool doHaveViewportAsTarget = false;
    public Destinations.FileFormat fmt;
    public string folder;
    public string name;
    public int time;
    RenderTexture inputTex;
    public RawImage viewport;

    // Start is called before the first frame update
    void Start()
    {
        loadExportSettings();
        makeSimulatorFromSettings();

        var restart_button = (UnityEngine.UIElements.Button?)settings.getElementByRelativeNamePathLogged(settings.getRootElement(), "root/scroll_menu/simulation_settings/action_restart");
        if (restart_button != null)
        {
            restart_button.clicked += () =>
            {
                Destroy(simulatorGameObject);
                loadExportSettings();
                makeSimulatorFromSettings();
            };
        }
        
        var begin_export_button = (UnityEngine.UIElements.Button?)settings.getElementByRelativeNamePathLogged(settings.getRootElement(), "root/scroll_menu/export_settings/action_begin_export");
        if (begin_export_button != null)
        {
            begin_export_button.clicked += () =>
            {
                loadExportSettings();
                if (fmt != Destinations.FileFormat.NONE)
                {
                    switch (fmt)
                    {
                        case Destinations.FileFormat.PNG:
                        case Destinations.FileFormat.JPG:
                        case Destinations.FileFormat.TGA:
                            destinations.Add(new Destinations.ImageSequence(folder, name, fmt, time));
                            break;
                        case Destinations.FileFormat.GIF:
                        case Destinations.FileFormat.MP4:
                        case Destinations.FileFormat.MOV:
                            Debug.LogError("Not implemented yet");
                            break;
                    }
                    
                }
            };
        }


        destinations.Add(new Destinations.Viewport(viewport));
        //destinations.Add(new Destinations.TimedImageSequence(folder, name, fmt, time));

        doHaveViewportAsTarget = destinations.OfType<Destinations.Viewport>().Any();
        //Instantiate gameobject to use for simulation rendering if needed
        if (doHaveViewportAsTarget)
        {
            int pixWidth = simulator.getSimulationSize().x * simulator.getScale();
            int pixHeight = simulator.getSimulationSize().y * simulator.getScale();
            RectTransform rectTransform = viewport.gameObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(pixWidth, pixHeight);
            rectTransform.anchoredPosition = new Vector2(pixWidth / 2, -pixHeight / 2);
        }
    }

    // Update is called once per tick
    void Update()
    {
        List<IImageDestination> thingsToRemove = new List<IImageDestination>();
        inputTex = simulator.getNextTexture();
        foreach (var destination in destinations)
        {
            if (destination is Destinations.Viewport)
            {
                destination.setImage(inputTex);
            } else
            {
                destination.setImage(simulator.getGurrentExportableTexture());
            }
            if (destination.lifetimeRemaining <= 0) 
            {
                thingsToRemove.Add(destination);
            }
        }
        foreach (IImageDestination dest in thingsToRemove)
        {
            dest.destroy();
            destinations.Remove(dest);
        }

    }

    private void OnDestroy()
    {
        foreach (IImageDestination dest in destinations)
        {
            dest.destroy();
        }
    }

    void LateUpdate()
    {
        if (doHaveViewportAsTarget)
        {
            destinations.OfType<Destinations.Viewport>().First().renderImageNow();
        }
    }

    void makeSimulatorFromSettings()
    {
        //Cache relevant settings to prevent repeat traversals
        UnityEngine.UIElements.VisualElement? simulation_settings = settings.getElementByRelativeNamePathLogged(settings.getRootElement(), "root/scroll_menu/simulation_settings");
        UnityEngine.UIElements.VisualElement? interaction_settings = settings.getElementByRelativeNamePathLogged(settings.getRootElement(), "root/scroll_menu/interaction_settings");

        //Get the settings       
        UnityEngine.UIElements.SliderInt? field_size = (UnityEngine.UIElements.SliderInt?)settings.getElementByRelativeNamePathLogged(simulation_settings, "field_size");
        UnityEngine.UIElements.SliderInt? tick_rate = (UnityEngine.UIElements.SliderInt?)settings.getElementByRelativeNamePathLogged(simulation_settings, "tick_rate");
        UnityEngine.UIElements.Slider? fluid_viscosity = (UnityEngine.UIElements.Slider?)settings.getElementByRelativeNamePathLogged(simulation_settings, "fluid_viscosity");
        UnityEngine.UIElements.Slider? fluid_diffusion_rate = (UnityEngine.UIElements.Slider?)settings.getElementByRelativeNamePathLogged(simulation_settings, "fluid_diffusion_rate");
        UnityEngine.UIElements.Slider? mouse_density = (UnityEngine.UIElements.Slider?)settings.getElementByRelativeNamePathLogged(interaction_settings, "mouse_density");
        UnityEngine.UIElements.Slider? mouse_force = (UnityEngine.UIElements.Slider?)settings.getElementByRelativeNamePathLogged(interaction_settings, "mouse_force");
        UnityEngine.UIElements.SliderInt? mouse_brush_size = (UnityEngine.UIElements.SliderInt?)settings.getElementByRelativeNamePathLogged(interaction_settings, "mouse_brush_size");

        simulatorGameObject = Instantiate(simulatorPrefab);
        simulator = simulatorGameObject.GetComponent<FluidSimulator>();

        simulator.gridSize = field_size?.value ?? 32;
        simulator.deltaTime = 1.0f / tick_rate?.value ?? 1f/30f;
        simulator.viscosity = fluid_viscosity?.value ?? 1f;
        simulator.diffusionRate = fluid_diffusion_rate?.value ?? 1f;

        simulator.penSize = mouse_brush_size?.value ?? 1;
        simulator.force = mouse_force?.value ?? 1f;
        simulator.drawValue = mouse_density?.value ?? 1f;

    }
    void loadExportSettings()
    {
        UnityEngine.UIElements.DropdownField? file_format = (UnityEngine.UIElements.DropdownField?)settings.getElementByRelativeNamePathLogged(settings.getRootElement(), "root/scroll_menu/export_settings/file_format");
        UnityEngine.UIElements.TextField? file_folder_path = (UnityEngine.UIElements.TextField?)settings.getElementByRelativeNamePathLogged(settings.getRootElement(), "root/scroll_menu/export_settings/file_folder_path");
        UnityEngine.UIElements.TextField? file_name = (UnityEngine.UIElements.TextField?)settings.getElementByRelativeNamePathLogged(settings.getRootElement(), "root/scroll_menu/export_settings/file_name");
        UnityEngine.UIElements.SliderInt? recording_time = (UnityEngine.UIElements.SliderInt?)settings.getElementByRelativeNamePathLogged(settings.getRootElement(), "root/scroll_menu/export_settings/recording_time");

        switch (file_format?.value)
        {
            case "PNG":
                fmt = Destinations.FileFormat.PNG;
                break;
            case "JPEG":
                fmt = Destinations.FileFormat.JPG;
                break;
            case "TGA":
                fmt = Destinations.FileFormat.TGA;
                break;
            case "GIF":
                fmt = Destinations.FileFormat.GIF;
                break;
            case "MP4":
                fmt = Destinations.FileFormat.MP4;
                break;
            case "MOV":
                fmt = Destinations.FileFormat.MOV;
                break;
            default:
                fmt = Destinations.FileFormat.NONE;
                break;
        }


        folder = Directory.Exists(file_folder_path.value) ? file_folder_path.value : "";
        name = file_name.value;
        time = recording_time.value;
        if (time == 0) { time = int.MaxValue; }
    }

    //[Button("Delete Media Folder Contents")]
    void deleteMediaFolderContents()
    {
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folder);
        foreach (FileInfo file in di.EnumerateFiles())
        {
            file.Delete();
        }
        foreach (DirectoryInfo dir in di.EnumerateDirectories())
        {
            dir.Delete(true);
        }
    }
    //[Button("Take Single Image")]
    void screenshot()
    {
        Destinations.ImageSequence image = new Destinations.ImageSequence(folder, name, fmt, 1);
        destinations.Add(image);
    }
}
