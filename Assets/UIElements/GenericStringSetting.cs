using UnityEngine;
using UnityEngine.UI;
using System;

public class GenericStringSetting : MonoBehaviour
{
    public string settingName;
    public string defaultValue;
    string currentValue;
    InputField textField;
    public GameObject textFieldGameObject;
    Text label;
    public GameObject labelGameObject;

    public void Awake()
    {
        textField = textFieldGameObject.GetComponent<InputField>();
        label = labelGameObject.GetComponent<Text>();

        this.settingName = name;
        textField.text = defaultValue;
        currentValue = defaultValue;
        label.text = settingName;

        this.gameObject.SetActive(true);
    }
    public void textFieldChanged()
    {
        currentValue = textField.text;
    }

    public string getValue()
    {
        return currentValue;
    }
    public string setValue(string value) 
    {
        currentValue = value;
        textField.text = currentValue;
        return currentValue;
    }
    public void invalidateValue()
    {
        currentValue = defaultValue;
        textField.text = currentValue;
    }
}
