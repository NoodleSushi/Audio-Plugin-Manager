using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace PluginManager.Editor.ToolMenus
{
    public class MenuButtonExtended : MenuButton
    {
        [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
        protected sealed class PopupItemAttribute : Attribute
        {
            private readonly string label;

            public PopupItemAttribute(string label)
            {
                this.label = label;
            }

            public string Label => label;
        }

        private int _id = 0;
        private readonly Dictionary<int, string> _commands = new();

        public override void _Ready()
        {
            GetPopup().Connect("id_pressed", this, nameof(OnPopupIDPressed));
        }

        public void OnPopupIDPressed(int ID)
        {
            GetType().GetMethod(_commands[ID]).Invoke(this, null);
        }

        protected void AddSeparator() => GetPopup().AddSeparator();

        protected void AddItem(string methodName)
        {
            MethodInfo methodInfo = GetType().GetMethod(methodName);
            PopupItemAttribute popupItemAttribute = methodInfo.GetCustomAttribute<PopupItemAttribute>();

            if (popupItemAttribute == null)
                throw new Exception(
                    $"method {methodName}() does not have PopupItemAttribute Implemented"
                );
            GetPopup().AddItem(popupItemAttribute.Label, _id);
            _commands.Add(_id, methodName);
            _id++;
        }
    }
}
