using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdvancedEditorTools.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class ResultDispatcher : MonoBehaviour
{
    public GameObject simulatorPrefab;
    GameObject simulatorGameObject;
    ISimulator simulator;
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
        simulatorGameObject = Instantiate(simulatorPrefab);
        simulator = simulatorGameObject.GetComponent<FluidSimulator>();

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
    void LateUpdate()
    {
        if (doHaveViewportAsTarget)
        {
            destinations.OfType<Destinations.Viewport>().First().renderImageNow();
        }
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
