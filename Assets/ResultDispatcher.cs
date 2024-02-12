using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.UIElements;
using static UserInput;


public class ResultDispatcher : MonoBehaviour
{
    public GameObject simulatorPrefab;
    FluidSimulator simulator;
    public SettingsPanel settingsPanel;
    public List<IImageDestination> destinations = new List<IImageDestination>();
    bool doHaveViewportAsTarget = false;
    public Destinations.FileFormat fmt;
    public string folder;
    public string fileName;
    public int time;

    RenderTexture inputTex;
    public UnityEngine.UI.RawImage viewport;
#nullable enable
    List<PlaybackFrame>? playbackFrames;
#nullable disable
    int playbackFrameNo;

    KeyFrame firstFrame;
    bool writingToSaveFile = false;
    bool readingFromSaveFile = false;


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
        List<UserInput> inputThisFrame = new List<UserInput>();
        //If currently reading from save file
        if (playbackFrames != null && readingFromSaveFile)
        {
            // Apply simulation objects to the simulator and solver
            simulator.simulationObjects = playbackFrames[playbackFrameNo].objects;
            simulator.solver.setPhysicsObjects(playbackFrames[playbackFrameNo].objects.OfType<CollidableCell>().ToList());
            
            // Step the simulation with loaded input
            inputTex = simulator.computeNextTexture(playbackFrames[playbackFrameNo].input);
            playbackFrameNo++;
            if (playbackFrameNo >= playbackFrames.Count())
            {
                readingFromSaveFile = false;
                playbackFrames = null;
                playbackFrameNo = 0;
            }
        } else
        {
            //remap xy coords to be same as screen UV coords
            float mouseX = Input.mousePosition.x;
            float mouseY = Screen.height - Input.mousePosition.y;

            //clamp to area of simulation
            mouseX = Math.Clamp(mouseX, 0, simulator.gridSize * simulator.scale - 1);
            mouseY = Math.Clamp(mouseY, 0, simulator.gridSize * simulator.scale - 1);

            //get grid pos of cursor
            int cursorX = (int)(mouseX / simulator.scale);
            int cursorY = simulator.gridSize - (int)(mouseY / simulator.scale) - 1;

            //get mouse velocity
            float mouseVelocityX = Input.GetAxis("Mouse X") * simulator.force;
            float mouseVelocityY = Input.GetAxis("Mouse Y") * simulator.force;


            if (Input.GetKey(KeyCode.V) && Input.GetMouseButton(0)) 
            { 
                inputThisFrame.Add(new UserInput(cursorX, cursorY, mouseVelocityX, fieldToWriteTo.VELX));
                inputThisFrame.Add(new UserInput(cursorX, cursorY, mouseVelocityY, fieldToWriteTo.VELY));
            } else if (Input.GetMouseButton(0))
            {
                inputThisFrame.Add(new UserInput(cursorX, cursorY, simulator.drawValue, fieldToWriteTo.DENS));
            } else if (Input.GetMouseButton(1))
            {
                inputThisFrame.Add(new UserInput(cursorX, cursorY, -simulator.drawValue, fieldToWriteTo.DENS));
            }

            // Step the simulation with gathered input
            inputTex = simulator.computeNextTexture(inputThisFrame);
        }

        
        //If currently saving to a file
        if (playbackFrames != null && writingToSaveFile)
        {
            if (playbackFrames.Count() == 0)
            {
                firstFrame = new KeyFrame(simulator.solver);
            }
            playbackFrames.Add(new PlaybackFrame(inputThisFrame, simulator.simulationObjects));
            
        }


        sendImagesToDestinations();
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
    void sendImagesToDestinations()
    {
        // Send output images to image destinations 
        List<IImageDestination> thingsToRemove = new List<IImageDestination>();
        foreach (var destination in destinations)
        {
            if (destination is Destinations.Viewport)
            {
                destination.setImage(inputTex);
            }
            else
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
    void saveFile(string path)
    {
        var f = System.IO.File.Create(path);
        var b = new BinaryFormatter();
        var p = new PlaybackFile(playbackFrames, firstFrame);
        b.Serialize(f, p);
    }
    void saveFileJson(string path)
    {
        var f = System.IO.File.Create(path);
        //var b = new BinaryFormatter();
        string file = JsonUtility.ToJson(new PlaybackFile(playbackFrames, firstFrame));
        //var p = new PlaybackFile(playbackFrames, firstFrame);
        f.Write(System.Text.Encoding.UTF8.GetBytes(file));
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
        Destinations.Image image = new Destinations.Image(folder, fileName, fmt);
        destinations.Add(image);
    }
    [Button("Begin Recording")]
    void beginRecording()
    {
        writingToSaveFile = true;
        if (playbackFrames != null)
        {
            return;
        }
        playbackFrames = new List<PlaybackFrame>();
    }
    [Button("Stop Recording")]
    void stopRecording()
    {
        writingToSaveFile = false;
        saveFile("./saves/save.simsave");
        playbackFrames = null;
    }
    [Button("Stop Recording (JSON)")]
    void stopRecordingJson()
    {
        writingToSaveFile = false;
        saveFile("./saves/save.simsave");
        saveFileJson("./saves/save.json");
        playbackFrames = null;
    }
    [Button("Load Save File")]
    void loadSaveFile(string path = "./saves/save.simsave")
    {
        var f = System.IO.File.Open(path, FileMode.Open);           //Open the file
        var b = new BinaryFormatter();                              
        PlaybackFile p = (PlaybackFile)b.Deserialize(f);            //Deserialise the save file 
        playbackFrames = p.frames.ToList();                         //Grab the update frames
        firstFrame = p.startFrame;                                  //Grab the first frame (the keyframe)
        playbackFrameNo = 0;
        if (simulatorGameObject != null) { Destroy(simulatorGameObject); }  //Destroy exising simulator if it exists
        simulatorGameObject = Instantiate(simulatorPrefab);
        simulator = simulatorGameObject.GetComponent<FluidSimulator>();

        destinations.Add(new Destinations.Viewport());
        readingFromSaveFile = true;
        writingToSaveFile = false;
        doHaveViewportAsTarget = destinations.OfType<Destinations.Viewport>().Any();
    }
    [Button("Force End Loading From Save File")]
    void forceEndLoadingFromSaveFile()
    {
        readingFromSaveFile = false;
        playbackFrames = null;
        playbackFrameNo = 0;
    }

}
