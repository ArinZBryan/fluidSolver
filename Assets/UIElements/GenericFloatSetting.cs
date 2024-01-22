using UnityEngine;
using UnityEngine.UI;
using System;
using AdvancedEditorTools.Attributes;

public class GenericFloatSetting : MonoBehaviour
{
    public float maxValue;
    public float minValue;
    float currentValue;
    public float stepSize;
    public string settingName;
    Text label;
    public GameObject labelGameObject;
    Slider slider;
    public GameObject sliderGameObject;
    InputField textField;
    public GameObject textFieldGameObject;

    public void Awake()
    {
        label = labelGameObject.GetComponent<Text>();
        slider = sliderGameObject.GetComponent<Slider>();
        textField = textFieldGameObject.GetComponent<InputField>();

        label.text = settingName;
        slider.value = currentValue;
        slider.maxValue = maxValue;
        slider.minValue = minValue;

        textField.text = currentValue.ToString();

        /*
        slider.RegisterValueChangedCallback((evt) =>
        {
            currentValue = verifyValue(evt.newValue);
            textField.value = currentValue.ToString();
        });

        textField.RegisterValueChangedCallback((evt) =>
        {
            if (float.TryParse(evt.newValue, out float result))
            {
                currentValue = verifyValue(result);
                slider.value = currentValue;
            }
        });
        */

    }
    public void sliderChanged()
    {
        currentValue = verifyValue(slider.value);
        textField.text = currentValue.ToString();
    }

    public void textFieldChanged()
    {
        if (float.TryParse(textField.text, out float result))
        {
            currentValue = verifyValue(result);
            slider.value = currentValue;
        }
        else
        {
            textField.text = currentValue.ToString();
        }
        
    }
    [Button("Get Value")]
    public float getValue()
    {
        Debug.Log(currentValue);
        return currentValue;
    }
    public void setValue(float value) 
    {
        currentValue = verifyValue(value);
        slider.value = currentValue;
        textField.text = currentValue.ToString();
    }
    float verifyValue(float value)
    {
        if (stepSize == 0f) //Is stepsize set?
        {
            return value;
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
        float lowStep = value - (value % (float)stepSize);
        float highStep = lowStep + (float)stepSize;
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
