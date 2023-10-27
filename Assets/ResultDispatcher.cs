using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu]
public class ResultDispatcher : ScriptableObject
{
    public GameObject simulatorPrefab;
    GameObject simulatorGameObject;
    ISimulator simulator;

    RenderTexture inputTex;

    // Start is called before the first frame update
    void Start()
    {
        simulatorGameObject = Instantiate(simulatorPrefab);
        simulator = simulatorGameObject.GetComponent<FluidSimulator>();
    }

    // Update is called once per tick
    void FixedUpdate()
    {
        inputTex = simulator.getNextTexture();

    }
}
