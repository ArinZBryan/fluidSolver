using System;
using UnityEngine;
using UnityEngine.UI;

public class GenericEnumSetting : MonoBehaviour
{
    //Define Setting 
    public string[] enumerator;
    public string defaultValue;
    public string settingName;

    //Keep track of current value
    string currentValue;

    //UI Elements
    Text label;
    public GameObject labelObject;
    Dropdown dropdown;
    public GameObject dropdownObject;

    private void Awake()
    {
        label = labelObject.GetComponent<Text>();
        dropdown = dropdownObject.GetComponent<Dropdown>();
        label.text = settingName;
        dropdown.options.Clear();
        foreach (string item in enumerator)
        {
            dropdown.options.Add(new Dropdown.OptionData(item));
        }
        this.currentValue = defaultValue;
        this.dropdown.value = Array.IndexOf(enumerator, defaultValue);
    }

    public void dropdownChanged()
    {
        currentValue = dropdown.options[dropdown.value].text;
    }

    public string getValue()
    {
        return currentValue;
    }
    public string setValue(string value) 
    {
        if (Array.Exists(enumerator, (x) => x == value)) {
            currentValue = value;
            dropdown.value = Array.IndexOf(enumerator, value);
            return value;
        }
        return "";
    }
}
