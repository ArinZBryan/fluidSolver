using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.UIElements;
using static UserInput;
using System.Threading;

public class ResultDispatcher : MonoBehaviour
{
    public GameObject simulatorPrefab;
    public FluidSimulator simulator;
    public MenuManager menuManager;
    public List<IImageDestination> destinations = new List<IImageDestination>();
    bool doHaveViewportAsTarget = false;
    public Destinations.FileFormat fmt;
    public string folder;
    public string fileName;
    public int time;

    Texture2D inputTex;
#nullable enable
    List<PlaybackFrame>? playbackFrames;
#nullable disable
    int playbackFrameNo;
    public UnityEngine.UI.RawImage viewport;

    KeyFrame firstFrame;
    bool writingToSaveFile = false;
    bool readingFromSaveFile = false;

    float mouseX = 0;
    float mouseY = 0;
    float mouseVelocityX = 0;
    float mouseVelocityY = 0;

    // Start is called before the first frame update
    void Start()
    {
        simulator = Instantiate(simulatorPrefab).GetComponent<FluidSimulator>();
        simulator.viewport = viewport.rectTransform;
        simulator.init();

        destinations.Add(new Destinations.Viewport(viewport, simulator.gridSize * simulator.scale));
        
        doHaveViewportAsTarget = destinations.OfType<Destinations.Viewport>().Any();
        //Instantiate gameobject to use for simulation rendering if needed
        if (doHaveViewportAsTarget)
        {
            int pixWidth = simulator.gridSize * simulator.scale;
            int pixHeight = simulator.gridSize * simulator.scale;
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
        }
        else
        {
            //remap xy coords to be same as screen UV coords
            RectTransform rectTransform = viewport.rectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out Vector2 localPoint);
            mouseX = localPoint.x;
            mouseY = localPoint.y;
            mouseX = Math.Clamp(mouseX, -rectTransform.rect.width / 2, rectTransform.rect.width / 2);
            mouseY = Math.Clamp(mouseY, -rectTransform.rect.height / 2, rectTransform.rect.height / 2);
            mouseX += rectTransform.rect.width / 2;
            mouseY += rectTransform.rect.height / 2;

            //get grid pos of cursor
            int cursorX = (int)(mouseX * simulator.gridSize / rectTransform.rect.width);
            int cursorY = (int)(mouseY * simulator.gridSize / rectTransform.rect.height);

            //get mouse velocity
            mouseVelocityX = Input.GetAxis("Mouse X") * simulator.force;
            mouseVelocityY = Input.GetAxis("Mouse Y") * simulator.force;


            if (Input.GetKey(KeyCode.V) && Input.GetMouseButton(0))
            {
                inputThisFrame.Add(new UserInput(cursorX, cursorY, mouseVelocityX, fieldToWriteTo.VELX));
                inputThisFrame.Add(new UserInput(cursorX, cursorY, mouseVelocityY, fieldToWriteTo.VELY));
            }
            else if (Input.GetMouseButton(0))
            {
                inputThisFrame.Add(new UserInput(cursorX, cursorY, simulator.drawValue, fieldToWriteTo.DENS));
            }
            else if (Input.GetMouseButton(1))
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

        var settingsPanel = menuManager.menus.OfType<MainSettings>().First();

#nullable enable
        SliderInt? field_size = (SliderInt?)Menu.getElementByRelativeNamePathLogged(settingsPanel.document.rootVisualElement, "root/scroll_menu/simulation_settings/field_size");
        SliderInt? tick_rate = (SliderInt?)Menu.getElementByRelativeNamePathLogged(settingsPanel.document.rootVisualElement, "root/scroll_menu/simulation_settings/tick_rate");
        Slider? fluid_viscosity = (Slider?)Menu.getElementByRelativeNamePathLogged(settingsPanel.document.rootVisualElement, "root/scroll_menu/simulation_settings/fluid_viscosity");
        Slider? fluid_diffusion_rate = (Slider?)Menu.getElementByRelativeNamePathLogged(settingsPanel.document.rootVisualElement, "root/scroll_menu/simulation_settings/fluid_diffusion_rate");
        Slider? mouse_density = (Slider?)Menu.getElementByRelativeNamePathLogged(settingsPanel.document.rootVisualElement, "root/scroll_menu/interaction_settings/mouse_density");
        Slider? mouse_force = (Slider?)Menu.getElementByRelativeNamePathLogged(settingsPanel.document.rootVisualElement, "root/scroll_menu/interaction_settings/mouse_force");
        SliderInt? mouse_brush_size = (SliderInt?)Menu.getElementByRelativeNamePathLogged(settingsPanel.document.rootVisualElement, "root/scroll_menu/interaction_settings/mouse_brush_size");
#nullable disable
        simulator.gridSize = field_size?.value ?? simulator.gridSize;
        simulator.deltaTime = 1f / tick_rate?.value ?? simulator.deltaTime;
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
        Destinations.Image image = new Destinations.Image(folder, fileName, fmt, 1);
        destinations.Add(image);
    }
    [Button("Begin Recording")]
    public void beginRecording()
    {
        writingToSaveFile = true;
        if (playbackFrames != null)
        {
            return;
        }
        playbackFrames = new List<PlaybackFrame>();
    }
    [Button("Stop Recording")]
    public void stopRecording()
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
    public void loadSaveFile(string path = "./saves/save.simsave")
    {
        var f = System.IO.File.Open(path, FileMode.Open);           //Open the file
        var b = new BinaryFormatter();
        PlaybackFile p = (PlaybackFile)b.Deserialize(f);            //Deserialise the save file 
        playbackFrames = p.frames.ToList();                         //Grab the update frames
        firstFrame = p.startFrame;                                  //Grab the first frame (the keyframe)
        playbackFrameNo = 0;
        Destroy(simulator.gameObject);   //Destroy exising simulator if it exists
        simulator = Instantiate(simulatorPrefab).GetComponent<FluidSimulator>();
        simulator.initFromKeyframe(firstFrame);
        simulator.viewport = viewport.rectTransform;
        destinations.Add(new Destinations.Viewport(viewport, simulator.gridSize * simulator.scale));
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
