using UnityEngine;
using UnityEngine.UI;

public class GenericBooleanSetting : MonoBehaviour
{
    bool currentValue;
    public bool defaultValue;
    public string settingName;

    Toggle toggle;
    public GameObject toggleObject;
    Text label;
    public GameObject labelObject;

    public void Awake()
    {
        toggle = toggleObject.GetComponent<Toggle>();
        label = labelObject.GetComponent<Text>();

        this.currentValue = defaultValue;
        this.settingName = name;

        label.text = settingName;
        toggle.isOn = currentValue;

    }

    public void toggleChanged()
    {
        currentValue = toggle.isOn;
    }
    public bool getValue()
    {
        return currentValue;
    }
    public bool setValue(bool value) 
    {
        currentValue = value;
        toggle.isOn = currentValue;
        return currentValue;
    }
}
