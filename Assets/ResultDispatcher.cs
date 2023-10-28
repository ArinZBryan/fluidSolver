using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class ResultDispatcher : MonoBehaviour
{
    public GameObject simulatorPrefab;
    GameObject simulatorGameObject;
    ISimulator simulator;
    public List<IImageDestination> destinations = new List<IImageDestination>();
    bool doHaveViewportAsTarget = false;

    RenderTexture inputTex;


    // Start is called before the first frame update
    void Start()
    {
        simulatorGameObject = Instantiate(simulatorPrefab);
        simulator = simulatorGameObject.GetComponent<FluidSimulator>();

        destinations.Add(new Destinations.Viewport());


        foreach (IImageDestination destination in destinations)
        {
            destination.init("", Destinations.FileFormat.PNG);
        }

        doHaveViewportAsTarget = destinations.OfType<Destinations.Viewport>().Any();

    }

    // Update is called once per tick
    void FixedUpdate()
    {
        inputTex = simulator.getNextTexture();
        foreach (var destination in destinations)
        {
            destination.setImage(inputTex);
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

}
