using UnityEngine;
using UnityEngine.UIElements;
using System;

public class GenericIntSetting : MonoBehaviour
{
    int maxValue;
    int minValue;
    int currentValue;
    int? stepSize;
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

    public void setup(int maxValue, int minValue, int defaultValue, int? stepSize, string name)
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
            currentValue = verifyValue(evt.newValue);
            textField.value = currentValue.ToString();
        });

        textField.RegisterValueChangedCallback((evt) =>
        {
            if (int.TryParse(evt.newValue, out int result))
            {
                currentValue = verifyValue(result);
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
        currentValue = verifyValue(value);
        slider.value = currentValue;
        textField.value = currentValue.ToString();
    }
    int verifyValue(float value)
    {
        int v = (int)Math.Round(value);
        if (!stepSize.HasValue) //Is stepsize set?
        {
            return v;
        }
        if (value > maxValue)   //Is the value outside the range?
        {
            return maxValue;
        }
        if (value < minValue)   //Is the value outside the range?
        {
            return minValue;
        }
        //Find the closest step
        int lowStep = v - (v % (int)stepSize);
        int highStep = lowStep + (int)stepSize;
        //Return the closest step
        if (Math.Abs(value - lowStep) < Math.Abs(value - highStep))
        {
            return lowStep;
        }
        else
        {
            return highStep;
        }
    }
}
