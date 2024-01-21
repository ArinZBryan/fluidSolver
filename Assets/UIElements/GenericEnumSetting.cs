using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.UI;

public class GenericEnumSetting : MonoBehaviour
{
    //Define Setting 
    System.Type settingEnum;
    System.Enum defaultValue;
    string settingName;
    
    //Keep track of current value
    string currentValue;
    object currentEnum;

    //UI Elements
    Label label;
    Dropdown dropdown;

    private void Awake()
    {
        label = transform.Find("SettingTitle").GetComponent<Label>();
        dropdown = transform.Find("Dropdown").GetComponent<Dropdown>();
        this.gameObject.SetActive(false);
    }

    public void setup(System.Type enumerator, System.Enum defaultValue, string name )
    {

        this.settingName = name;
        dropdown.options.Clear();
        foreach (var item in System.Enum.GetValues(enumerator))
        {
            dropdown.options.Add(new Dropdown.OptionData(item.ToString()));
        }

        this.defaultValue = defaultValue;
        this.currentEnum = defaultValue;
        this.dropdown.value = dropdown.options.FindIndex((x) => x.text == defaultValue.ToString());

        dropdown.onValueChanged.AddListener((value) =>
        {
            dropdown.options[value].text = currentValue;
        });

        this.gameObject.SetActive(true);
    }

    public object getValue()
    {
        object res;
        System.Enum.TryParse(settingEnum.GetType(), currentValue, out res);
        return res;
    }
    public string setValue(string value) 
    {
        object e;
        if (System.Enum.TryParse(settingEnum.GetType(), value, out e)) { return ""; }
        currentEnum = e;
        currentValue = value;
        dropdown.value = dropdown.options.FindIndex((x) => x.text == currentValue);
        return value;
    }
}
