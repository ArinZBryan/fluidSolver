using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GenericNumberSetting : MonoBehaviour
{
    float maxValue;
    float minValue;
    float currentValue;
    float stepSize;
    string settingName;
    Label label;
    Slider slider;
    TextField textField;
    private void Awake()
    {
        label = transform.Find("SettingTitle").GetComponent<Label>();
        slider = transform.Find("Slider").GetComponent<Slider>();
        textField = transform.Find("TextField").GetComponent<TextField>();
        this.gameObject.SetActive(false);
    }

    public void setup(float maxValue, float minValue, float defaultValue, float stepSize, string name)
    {
        this.maxValue = maxValue;
        this.minValue = minValue;
        this.currentValue = defaultValue;
        this.stepSize = stepSize;
        this.settingName = name;

        label.text = settingName;
        slider.value = currentValue;
        slider.highValue = maxValue;
        slider.lowValue = minValue;

        textField.value = currentValue.ToString();

        slider.RegisterValueChangedCallback((evt) =>
        {
            currentValue = evt.newValue;
            textField.value = currentValue.ToString();
        });

        textField.RegisterValueChangedCallback((evt) =>
        {
            if (float.TryParse(evt.newValue, out float result))
            {
                currentValue = result;
                slider.value = currentValue;
            }
        });

        this.gameObject.SetActive(true);
    }

    public float getValue()
    {
        return currentValue;
    }
    public void setValue(float value) 
    {
        currentValue = value;
        slider.value = currentValue;
        textField.value = currentValue.ToString();
    }
}
