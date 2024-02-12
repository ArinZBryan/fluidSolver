using System;

namespace AdvancedEditorTools.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonAttribute : Attribute
    {
        public string buttonName;
        public int buttonSize;

        /// <param fileName="buttonName">Text displayed on top of the button</param>
        /// <param fileName="fontSize">Font size of the button's fileName</param>
        public ButtonAttribute(string buttonName, int fontSize = 12)
        {
            this.buttonName = buttonName;
            this.buttonSize = fontSize;
        }
    }
}