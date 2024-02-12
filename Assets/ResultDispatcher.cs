using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.UIElements;

public class ResultDispatcher : MonoBehaviour
{
    public GameObject simulatorPrefab;
    FluidSimulator simulator;
    public SettingsPanel settingsPanel;
    public List<IImageDestination> destinations = new List<IImageDestination>();
    bool doHaveViewportAsTarget = false;
    public Destinations.FileFormat fmt;
    public string folder;
    public string name;
    public int time;
    RenderTexture inputTex;
    public UnityEngine.UI.RawImage viewport;

    // Start is called before the first frame update
    void Start()
    {
        simulator = Instantiate(simulatorPrefab).GetComponent<FluidSimulator>();
        simulator.viewport = viewport.rectTransform;    
        simulator.init();
#nullable enable
        settingsPanel.settingsPanel.gameObject.SetActive(true);
        Button? maybe_restart_button = (Button?)settingsPanel.getElementByRelativeNamePathLogged(settingsPanel.getRootElement(), "root/scroll_menu/simulation_settings/action_restart");
        if (maybe_restart_button == null) { Debug.LogError("An error occured while connecting to the UI"); }
        else
        {
            maybe_restart_button.clicked += () => 
                {
                    Debug.Log("Resetting Simulation");
                    makeSimulatorFromSettings();
                };
        }
        settingsPanel.settingsPanel.gameObject.SetActive(false);
#nullable disable
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

    public void makeSimulatorFromSettings()
    {
        DestroyImmediate(simulator.gameObject);
        simulator = Instantiate(simulatorPrefab).GetComponent<FluidSimulator>();
#nullable enable
        SliderInt? field_size =        (SliderInt?)settingsPanel.getElementByRelativeNamePathLogged(settingsPanel.getRootElement(), "root/scroll_menu/simulation_settings/field_size");
        SliderInt? tick_rate  =        (SliderInt?)settingsPanel.getElementByRelativeNamePathLogged(settingsPanel.getRootElement(), "root/scroll_menu/simulation_settings/tick_rate");
        Slider? fluid_viscosity =         (Slider?)settingsPanel.getElementByRelativeNamePathLogged(settingsPanel.getRootElement(), "root/scroll_menu/simulation_settings/fluid_viscosity");
        Slider? fluid_diffusion_rate =    (Slider?)settingsPanel.getElementByRelativeNamePathLogged(settingsPanel.getRootElement(), "root/scroll_menu/simulation_settings/fluid_diffusion_rate");
        Slider? mouse_density =           (Slider?)settingsPanel.getElementByRelativeNamePathLogged(settingsPanel.getRootElement(), "root/scroll_menu/interaction_settings/mouse_density");
        Slider? mouse_force =             (Slider?)settingsPanel.getElementByRelativeNamePathLogged(settingsPanel.getRootElement(), "root/scroll_menu/interaction_settings/mouse_force");
        SliderInt? mouse_brush_size =  (SliderInt?)settingsPanel.getElementByRelativeNamePathLogged(settingsPanel.getRootElement(), "root/scroll_menu/interaction_settings/mouse_brush_size");
#nullable disable
        simulator.gridSize = field_size?.value ?? simulator.gridSize;
        simulator.deltaTime = 1f/tick_rate?.value ?? simulator.deltaTime;
        simulator.viscosity = fluid_viscosity?.value ?? simulator.viscosity;
        simulator.diffusionRate = fluid_diffusion_rate?.value ?? simulator.diffusionRate;
        simulator.drawValue = mouse_density?.value ?? simulator.drawValue;
        simulator.force = mouse_force?.value ?? simulator.force;
        simulator.penSize = mouse_brush_size?.value ?? simulator.penSize;

        simulator.viewport = viewport.rectTransform;

        simulator.init();
    }
    [Button("Delete Media Folder Contents")]
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
    [Button("Take Single Image")]
    void screenshot()
    {
        Destinations.Image image = new Destinations.Image(folder, name, fmt);
        destinations.Add(image);
    }
}
