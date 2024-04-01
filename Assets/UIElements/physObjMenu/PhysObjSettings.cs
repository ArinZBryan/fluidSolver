using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PhysObjSettings : Menu
{
    VisualTreeAsset? velocityFieldSettings;
    VisualTreeAsset? densityFieldSettings;
    VisualTreeAsset? physicsPointSettings;
    VisualTreeAsset? collidableCellSettings;
    VisualTreeAsset? simulationObjectPrefab;
    List<SimulationObject> simulationObjects = new List<SimulationObject>();
    public void Start()
    {
        // Define values for inherited fields
        menuManager = GameObject.Find("Canvas").GetComponent<MenuManager>();
        velocityFieldSettings = Resources.Load<VisualTreeAsset>("velocityFieldSettings");
        densityFieldSettings = Resources.Load<VisualTreeAsset>("densityFieldSettings");
        physicsPointSettings = Resources.Load<VisualTreeAsset>("physicsPointSettings");
        collidableCellSettings = Resources.Load<VisualTreeAsset>("collidableCellSettings");
        simulationObjectPrefab = Resources.Load<VisualTreeAsset>("physicsObject");

        // Add functionality to the menu here
        VisualElement root = document.rootVisualElement;
        Button? button_exit = (Button?)getElementByRelativeNamePathLogged(root, "root/content/confirm_options/action_exit");
        if (button_exit != null)
        {
            button_exit.clicked += () => { this.Close(); };
        }
        Button? button_add_object = (Button?)getElementByRelativeNamePathLogged(root, "root/content/confirm_options/action_add_object");
        if (button_add_object == null) { Debug.LogError("Something went wrong while mounting the UI"); return; }
        button_add_object.clicked += addPhysicsObject;
        DropdownField? dropdown_object_type = (DropdownField?)getElementByRelativeNamePathLogged(root, "root/content/content/left/object_selector");
        if (dropdown_object_type == null) { Debug.LogError("Something went wrong while mounting the UI"); return; }

        dropdown_object_type.RegisterValueChangedCallback((evt) =>
        {
            string selected = evt.newValue;
            VisualElement? setting_content = getElementByRelativeNamePathLogged(root, "root/content/content/left/setting_content");
            if (setting_content == null) { Debug.LogError("Something went wrong while mounting the UI"); return; }
            setting_content.Clear();
            VisualTreeAsset? visualTreeAsset;
            switch (selected)
            {
                case "Force Field":
                    visualTreeAsset = velocityFieldSettings;
                    break;
                case "Density Enforcer":
                    visualTreeAsset = densityFieldSettings;
                    break;
                case "Physics Point":
                    visualTreeAsset = physicsPointSettings;
                    break;
                case "Collidable Cell":
                    visualTreeAsset = collidableCellSettings;
                    break;
                default:
                    visualTreeAsset = null;
                    Debug.LogError("Could Not Find Associated UXML File");
                    break;
            }
            if (visualTreeAsset != null)
            {
                visualTreeAsset.CloneTree(setting_content);
                VisualElement actual_setting_content = setting_content[0];
            }

        });
    }

    void initialiseMenuValues()
    {

        ScrollView? scrollView = (ScrollView?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/content/content/right");
        if (scrollView == null) { Debug.LogError("Something went wrong while mounting the UI"); return; }
        scrollView.Clear();
        for (int i = 0; i < simulationObjects.Count; i++)
        {
            VisualElement? simulationObject = simulationObjectPrefab?.CloneTree();
            if (simulationObject == null) { Debug.LogError("Something went wrong while mounting the UI"); return; }
            simulationObject.name = simulationObjects[i].GetHashCode().ToString("X5");
            simulationObject.Q<Label>("name").text = simulationObjects[i].name + "\n" + simulationObject.name;
            simulationObject.Q<Button>("action_remove").clicked += () =>
            {
                simulationObjects.Remove(simulationObjects.Find((x) => x.GetHashCode().ToString("X5") == simulationObject.name));
                initialiseMenuValues();
            };
            scrollView.Add(simulationObject);
        }
    }

    public override void Open()
    {
        base.Open();
        simulationObjects = menuManager.resultDispatcher.simulator.simulationObjects;
        initialiseMenuValues();
        menuManager.resultDispatcher.gameObject.SetActive(false);
    }
    public override void Close()
    {
        base.Close();
        
        menuManager.resultDispatcher.gameObject.SetActive(true);
        menuManager.resultDispatcher.simulator.simulationObjects = simulationObjects;   
    }
    void addPhysicsObject()
    {
        DropdownField? dropdown_object_type = (DropdownField?)getElementByRelativeNamePathLogged(document.rootVisualElement, "root/content/content/left/object_selector");
        var selected = dropdown_object_type.value;
        VisualElement? setting_content = getElementByRelativeNamePathLogged(document.rootVisualElement, "root/content/content/left/setting_content");
        if (setting_content == null) { Debug.LogError("Something went wrong while mounting the UI"); return; }
        switch (selected)
        {
            case "Force Field":
                {
                    TextField posXField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/position/x");
                    TextField posYField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/position/y");
                    TextField dimXField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/width/x");
                    TextField dimYField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/width/y");
                    TextField valXField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/value/x");
                    TextField valYField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/value/y");
                    int posX, posY, dimX, dimY;
                    float valX, valY;
                    if (!int.TryParse(posXField.value, out posX)) { posXField.value = "0"; return; }
                    if (!int.TryParse(posYField.value, out posY)) { posYField.value = "0"; return; }
                    if (!int.TryParse(dimXField.value, out dimX)) { dimXField.value = "0"; return; }
                    if (!int.TryParse(dimYField.value, out dimY)) { dimYField.value = "0"; return; }
                    if (!float.TryParse(valXField.value, out valX)) { valXField.value = "0.0"; return; }
                    if (!float.TryParse(valYField.value, out valY)) { valYField.value = "0.0"; return; }

                    simulationObjects.Add(new VelocityForceField(posX, posY, dimX, dimY, valX, valY, UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0f, 1f, 1f, 1f)));
                    initialiseMenuValues();
                }
                break;
            case "Density Enforcer":
                {
                    TextField posXField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/position/x");
                    TextField posYField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/position/y");
                    TextField dimXField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/width/x");
                    TextField dimYField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/width/y");
                    TextField valField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/value/input");
                    int posX, posY, dimX, dimY;
                    float val;
                    if (!int.TryParse(posXField.value, out posX)) { posXField.value = "0"; return; }
                    if (!int.TryParse(posYField.value, out posY)) { posYField.value = "0"; return; }
                    if (!int.TryParse(dimXField.value, out dimX)) { dimXField.value = "0"; return; }
                    if (!int.TryParse(dimXField.value, out dimY)) { dimYField.value = "0"; return; }
                    if (!float.TryParse(valField.value, out val)) { valField.value = "0.0"; return; }

                    simulationObjects.Add(new DensityEnforcer(posX, posY, dimX, dimY, val, UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0f, 1f, 1f, 1f)));
                    initialiseMenuValues();
                }
                break;
            case "Physics Point":
                {
                    TextField posXField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/position/x");
                    TextField posYField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/position/y");
                    int posX, posY;
                    if (!int.TryParse(posXField.value, out posX)) { posXField.value = "0"; return; }
                    if (!int.TryParse(posXField.value, out posY)) { posYField.value = "0"; return; }
                    simulationObjects.Add(new PhysPoint(posX, posY, UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0f, 1f, 1f, 1f)));
                    initialiseMenuValues();
                }
                break;
            case "Collidable Cell":
                {
                    TextField posXField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/position/x");
                    TextField posYField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/position/y");
                    TextField dimXField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/width/x");
                    TextField dimYField = (TextField)getElementByRelativeNamePathLogged(setting_content, "root/width/y");
                    Toggle collideLeft = (Toggle)getElementByRelativeNamePathLogged(setting_content, "root/faces/collide_left");
                    Toggle collideRight = (Toggle)getElementByRelativeNamePathLogged(setting_content, "root/faces/collide_right");
                    Toggle collideTop = (Toggle)getElementByRelativeNamePathLogged(setting_content, "root/faces/collide_top");
                    Toggle collideBottom = (Toggle)getElementByRelativeNamePathLogged(setting_content, "root/faces/collide_bottom");
                    int posX, posY, dimX, dimY;
                    if (!int.TryParse(posXField.value, out posX)) { posXField.value = "0"; return; }
                    if (!int.TryParse(posYField.value, out posY)) { posYField.value = "0"; return; }
                    if (!int.TryParse(dimXField.value, out dimX)) { dimXField.value = "0"; return; }
                    if (!int.TryParse(dimYField.value, out dimY)) { dimYField.value = "0"; return; }
                    Solver2D.Boundary collidableFaces = Solver2D.Boundary.NONE;
                    if (collideLeft.value) { collidableFaces |= Solver2D.Boundary.LEFT; }
                    if (collideRight.value) { collidableFaces |= Solver2D.Boundary.RIGHT; }
                    if (collideTop.value) { collidableFaces |= Solver2D.Boundary.TOP; }
                    if (collideBottom.value) { collidableFaces |= Solver2D.Boundary.BOTTOM; }

                    simulationObjects.Add(new CollidableCell(posX, posY, dimX, dimY, collidableFaces, UnityEngine.Random.ColorHSV(0f, 1f, 0.5f, 1f, 0f, 1f, 1f, 1f)));

                    initialiseMenuValues();
                }
                break;
            default:
                break;
        }
    }
}

/*
 * VisualElement root
 * |--> VisualElement content
 *      |--> Label title
 *      |--> VisualElement content
 *      |    |--> VisualElement left
 *      |    |    |--> Dropdown object_selector
 *      |    |    |--> VisualElement setting_content
 *      |    |         |-->? VelocityFieldSettings.uxml / densityFieldSettings.uxml / physPoint.uxml / collidableCell.uxml
 *      |    |--> ScrollView right
 *      |         |-->? PhysObj.uxml (many)
 *      |--> VisualElement confirm_options
 *           |--> Button action_exit
 *           |--> Button action_add_object
 */