using UnityEngine;
using UnityEngine.UI;
using System;

public class GenericIntSetting : MonoBehaviour
{
    public int maxValue;
    public int minValue;
    int currentValue;
    public int? stepSize;
    public string settingName;
    Text label;
    public GameObject labelGameObject;
    Slider slider;
    public GameObject sliderGameObject;
    InputField textField;
    public GameObject textFieldGameObject;
    


    public void Awake()
    {
        label = labelGameObject.GetComponent<UnityEngine.UI.Text>();
        slider = sliderGameObject.GetComponent<Slider>();
        textField = textFieldGameObject.GetComponent<InputField>();

        label.text = settingName;
        slider.value = currentValue;
        slider.maxValue = maxValue;
        slider.minValue = minValue;
        textField.text = currentValue.ToString();

    }

    public void sliderChanged()
    {
        currentValue = verifyValue(slider.value);
        textField.text = currentValue.ToString();
    }

    public void textFieldChanged()
    {
        if (int.TryParse(textField.text, out int result))
        {
            currentValue = verifyValue(result);
            slider.value = currentValue;
        } else
        {
            textField.text = currentValue.ToString();
        }

    }

    public float getValue()
    {
        return currentValue;
    }
    public void setValue(float value)
    {
        currentValue = verifyValue(value);
        slider.value = currentValue;
        textField.text = currentValue.ToString();
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
