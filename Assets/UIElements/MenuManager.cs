using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    public Menu[] menus;
    public ResultDispatcher resultDispatcher;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Menu m in menus)
        {
            m.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Menu m in menus)
        {
            if (Input.GetKeyUp(m.boundKey)) 
            {
                Debug.Log("Got menu keybind for menu: " + m.name);
                if (m.IsOpen)
                {
                    m.Close();
                }
                else
                {
                    m.Open();
                }
            }
        }
    }
}

public abstract class Menu : MonoBehaviour
{
    public virtual void Open()
    {
        Menu.getElementByRelativeNamePath(document.rootVisualElement, "root").style.display = DisplayStyle.Flex;
        IsOpen = true;
    }
    public virtual void Close()
    {
        Menu.getElementByRelativeNamePath(document.rootVisualElement, "root").style.display = DisplayStyle.None;
        IsOpen = false;
    }
    public bool IsOpen { get; protected set; }
    public KeyCode boundKey;
    public UIDocument document;
    public MenuManager menuManager;

    public static VisualElement? getElementByRelativeNamePath(VisualElement rootElement, string path)
    {
        for (int i = 0; i < rootElement.childCount; i++)
        {
            VisualElement child = rootElement[i];
            if (child.name == path) //Check if child is destination
            {
                return child;
            }
            else
            {
                if (child.childCount == 0)  //check if at dead end
                {
                    continue;
                }
                var newPathArr = path.Split('/').Skip(1);
                if (!path.Split('/').Contains(child.name)) //check if this child is not in the path at all
                {
                    continue;
                }
                    
                
                var newPath = newPathArr.Count() > 1 ? newPathArr.Aggregate((a, b) => a + "/" + b) : newPathArr.First();
                VisualElement? result = getElementByRelativeNamePath(child, newPath);
                if (result != null)
                {
                    return result;
                }
            }
        }
        return null;
    }
    public static VisualElement? getElementByRelativeNamePathLogged(VisualElement? rootElement, string path)
    {
        if (rootElement == null) { return null; }
        var elem = getElementByRelativeNamePath(rootElement, path);
        if (elem != null) Debug.Log("Found Element: " + elem.name + " at: " + path);
        else Debug.Log("Element not found at: " + path);
        return elem;
    }

}