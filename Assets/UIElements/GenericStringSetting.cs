using UnityEngine;
using UnityEngine.UIElements;
using System;

public class GenericStringSetting : MonoBehaviour
{
    string settingName;
    string currentValue;
    TextField textField;
    Label label;
    public delegate string? verificationFunction(string input);
    verificationFunction verifyInput;

    private void Awake()
    {
        label = transform.Find("SettingTitle").GetComponent<Label>();
        textField = transform.Find("InputField").GetComponent<TextField>();
        this.gameObject.SetActive(false);
    }

    public void setup(string defaultValue, verificationFunction f, string name )
    {

        this.settingName = name;
        textField.value = defaultValue;
        currentValue = defaultValue;
        label.text = settingName;
        verifyInput = f;

        textField.RegisterValueChangedCallback((evt) =>
        {
            currentValue = verifyInput(evt.newValue) ?? "";
        });

        this.gameObject.SetActive(true);
    }

    public string getValue()
    {
        return currentValue;
    }
    public string setValue(string value) 
    {
        currentValue = verifyInput(value) ?? "";
        textField.value = currentValue;
        return currentValue;
    }
}
