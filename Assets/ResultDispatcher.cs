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
    GameObject simulatorGameObject;
    FluidSimulator simulator;

    public List<IImageDestination> destinations = new List<IImageDestination>();
    bool doHaveViewportAsTarget = false;
    public Destinations.FileFormat fmt;
    public string folder;
    public string fileName;
    public int time;

    RenderTexture inputTex;
    
    List<PlaybackFrame>? playbackFrames;
    IEnumerator<PlaybackFrame>? playbackFramesEnumerator;
    KeyFrame firstFrame;
    bool writingToSaveFile = true;
    bool readingFromSaveFile = false;



    // Start is called before the first frame update
    void Start()
    {
        simulatorGameObject = Instantiate(simulatorPrefab);
        simulator = simulatorGameObject.GetComponent<FluidSimulator>();

        destinations.Add(new Destinations.Viewport());
        //destinations.Add(new Destinations.TimedImageSequence(folder, fileName, fmt, time));

        doHaveViewportAsTarget = destinations.OfType<Destinations.Viewport>().Any();

    }

    // Update is called once per tick
    void Update()
    {
        List<UserInput> inputThisFrame = new List<UserInput>();
        //If currently reading from save file
        if (playbackFrames != null && readingFromSaveFile)
        {
            // Apply simulation objects to the simulator and solver
            simulator.simulationObjects = playbackFramesEnumerator.Current.objects;
            simulator.solver.setPhysicsObjects(playbackFramesEnumerator.Current.objects.OfType<CollidableCell>());
            
            // Step the simulation with loaded input
            inputTex = simulator.computeNextTexture(playbackFramesEnumerator.Current.input);
            playbackFramesEnumerator.MoveNext();
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

    /* Becuase of the way that Graphics.DrawTexture works, it can only be called in OnGUI() because 
     * it draws the texture immediately. This means it will not make it to the screen if it is called before the drawing step.
     * 
     * This means that the Destinations.Viewport class cannot have setImage() also draw the image to the screen
     * as it happens during FixedUpdate().
     * 
     * So, as a quick fix, during Start() we set a flag if we are rendering to a Destinations.Viewport(), and if that is set
     * during the OnGUI(), we call a unique renderImageNow() function on Destinations.Viewport(). This just calls 
     * Graphics.DrawTexture(...) with the currently stored RenderTexture.
     * 
     * -- Note --
     * Some places will say you can call it in OnPostRender(), however, this function only works in URP / HDRP, not in SRP
     * (what we are using here). I tried a workaround, but it just was not doing it. So OnGUI() it is.
    */
    void OnGUI()
    {
        if (doHaveViewportAsTarget)
        {
            if (Event.current.type == EventType.Repaint) 
            {
                destinations.OfType<Destinations.Viewport>().First().renderImageNow();
            }
        }
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
        foreach (var frame in playbackFrames)
        {
            Debug.Log(frame.input.Count);
        }
        firstFrame = p.startFrame;                                  //Grab the first frame (the keyframe)
        playbackFramesEnumerator = playbackFrames.GetEnumerator();  //Make an enumerator for the update frames
        if (simulatorGameObject != null) { Destroy(simulatorGameObject); }  //Destroy exising simulator if it exists
        simulatorGameObject = Instantiate(simulatorPrefab);
        simulator = simulatorGameObject.GetComponent<FluidSimulator>();

        destinations.Add(new Destinations.Viewport());
       
        doHaveViewportAsTarget = destinations.OfType<Destinations.Viewport>().Any();
    }
}
