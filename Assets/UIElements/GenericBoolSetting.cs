using UnityEngine;
using UnityEngine.UIElements;
using System;

public class GenericBoolSetting : MonoBehaviour
{
    bool currentValue;
    bool defaultValue;
    string settingName;

    Toggle toggle;
    Label label;
    private void Awake()
    {

        GameObject toggleObj = transform.Find("Toggle").gameObject;
        toggle = toggleObj.GetComponent<Toggle>();
        label = toggleObj.GetComponentInChildren<Label>();
        this.gameObject.SetActive(false);
    }

    public void setup(bool defaultValue, string name)
    {
        this.currentValue = defaultValue;
        this.settingName = name;

        label.text = settingName;
        toggle.value = currentValue;

        toggle.RegisterValueChangedCallback((evt) =>
        {
            currentValue = evt.newValue;
        });

        this.gameObject.SetActive(true);
    }

    public bool getValue()
    {
        return currentValue;
    }
    public bool setValue(bool value) 
    {
        currentValue = value;
        toggle.value = currentValue;
        return currentValue;
    }
}
