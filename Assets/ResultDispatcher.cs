using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdvancedEditorTools.Attributes;
using UnityEngine;

public class ResultDispatcher : MonoBehaviour
{
    public GameObject simulatorPrefab;
    GameObject simulatorGameObject;
    ISimulator simulator;
    public List<IImageDestination> destinations = new List<IImageDestination>();
    bool doHaveViewportAsTarget = false;
    public Destinations.FileFormat fmt;
    public string path;
    RenderTexture inputTex;


    // Start is called before the first frame update
    void Start()
    {
        simulatorGameObject = Instantiate(simulatorPrefab);
        simulator = simulatorGameObject.GetComponent<FluidSimulator>();

        destinations.Add(new Destinations.Viewport());
        destinations.Add(new Destinations.ImageSequence());

        foreach (IImageDestination destination in destinations)
        {
            destination.init(path, fmt);
        }

        doHaveViewportAsTarget = destinations.OfType<Destinations.Viewport>().Any();

    }

    // Update is called once per tick
    void FixedUpdate()
    {
        inputTex = simulator.getNextTexture();
        foreach (var destination in destinations)
        {
            if (destination is Destinations.Viewport) destination.setImage(inputTex);
            else destination.setImage(simulator.getGurrentExportableTexture());
        }
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


    [Button("Delete Media Folder Contents")]
    void deleteMediaFolderContents()
    {
        string[] parts = path.Split('\\');
        parts[parts.Length - 1] = "";
        string folder = "";
        foreach (var part in parts)
        {
            folder += part;
            folder += "\\";
        }

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
}
