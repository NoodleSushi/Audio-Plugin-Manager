using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginManager.PluginTree
{
    public class Name : Component
    {
        public string NameString;

        public override void ModifyTreeItem(TreeItem treeItem)
        {
            TreeEntity.Label = NameString;
        }

        public override void GenerateProperties(VBoxContainer propertiesContainer)
        {
            LineEdit lineEdit = new LineEdit();
            propertiesContainer.AddChild(lineEdit);
            lineEdit.Text = NameString;
            lineEdit.Connect("text_entered", this, nameof(OnLineEditTextEntered));
        }

        public void OnLineEditTextEntered(string new_text)
        {
            NameString = new_text;
            TreeEntity.UpdateTreeItem();
        }
    }
}
