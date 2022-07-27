using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PluginManager.PluginTree;
using PluginManager.PluginTree.Components;
using System;

namespace PluginManager.Editor
{
    public class PluginMakerDialog : Godot.AcceptDialog
    {
        private readonly LineEdit NameListEditor = new();
        private readonly LineEdit NameEditor = new();
        private readonly List<LineEdit> DAWNameEditors = new();
        private readonly List<CheckBox> DAWAvailableEditors = new();
        private readonly Tree TagSelectionTree = new();
        private readonly HashSet<int> TagIndexes = new();
        private readonly FileDialog NameGrabberDialog = new();

        public override void _Ready()
        {
            WindowTitle = "Generate Plugins";
            PopupExclusive = true;
            Resizable = true;

            MarginContainer marginContainer = new();
            marginContainer.SizeFlagsHorizontal |= (int)Control.SizeFlags.ExpandFill;
            marginContainer.SizeFlagsVertical |= (int)Control.SizeFlags.ExpandFill;
            marginContainer.AddConstantOverride("margin_right", 8);
            marginContainer.AddConstantOverride("margin_top", 8);
            marginContainer.AddConstantOverride("margin_left", 8);
            marginContainer.AddConstantOverride("margin_bottom", 36);
            AddChild(marginContainer);

            VBoxContainer vBoxContainer = new();
            vBoxContainer.SizeFlagsVertical |= (int)Control.SizeFlags.ExpandFill;
            marginContainer.AddChild(vBoxContainer);

            vBoxContainer.AddChild(new Label { Text = "Name List:" });

            NameListEditor.PlaceholderText = "Plugin 1, Plugin 2, Plugin 3";
            vBoxContainer.AddChild(NameListEditor);

            Button NameGrabberButton = new() { Text = "Grab Names From Files" };
            NameGrabberButton.Connect("pressed", this, nameof(OnNameGrabberButtonPressed));
            vBoxContainer.AddChild(NameGrabberButton);

            vBoxContainer.AddChild(new HSeparator());

            NameEditor.PlaceholderText = "{name}";
            vBoxContainer.AddChild(NameEditor);

            vBoxContainer.AddChild(new HSeparator());

            for (int idx = 0; idx < DAWProperties.DAWCount; idx++)
            {
                TextureRect icon = new()
                {
                    Texture = DAWProperties.ICONS[idx],
                    StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
                };
                vBoxContainer.AddChild(icon);
                LineEdit dawEditor = new() { PlaceholderText = "{name}" };
                CheckBox availableEditor = new() { Text = "Exposed" };
                DAWNameEditors.Add(dawEditor);
                DAWAvailableEditors.Add(availableEditor);
                vBoxContainer.AddChild(availableEditor);
                vBoxContainer.AddChild(dawEditor);
            }

            vBoxContainer.AddChild(new HSeparator());

            TagSelectionTree.SizeFlagsVertical = (int)Control.SizeFlags.ExpandFill;
            TagSelectionTree.HideRoot = true;
            TagSelectionTree.Connect("button_pressed", this, nameof(OnTagSelectionTreeButtonPressed));
            vBoxContainer.AddChild(TagSelectionTree);

            Connect("confirmed", this, nameof(OnConfirmed));

            NameGrabberDialog.Mode = Godot.FileDialog.ModeEnum.OpenFiles;
            NameGrabberDialog.Access = Godot.FileDialog.AccessEnum.Filesystem;
            NameGrabberDialog.WindowTitle = "Grab Names From Files";
            NameGrabberDialog.Connect("files_selected", this, nameof(OnNameGrabberDialogFilesSelected));
            WindowContainer.Instance.AddChild(NameGrabberDialog);
        }

        private void OnNameGrabberDialogFilesSelected(string[] paths)
        {
            NameListEditor.Text = NameListEditor.Text.TrimEnd(',');
            foreach (string path in paths)
            {
                string newPath = path.GetFile();
                int index = newPath.LastIndexOf('.');
                newPath = index == -1 ? newPath : newPath.Substring(0, index);
                if (newPath.Length > 0)
                {
                    if (NameListEditor.Text.Length > 0)
                        NameListEditor.Text += ',';
                    NameListEditor.Text += newPath;
                }
            }
        }

        private void OnNameGrabberButtonPressed()
        {
            NameGrabberDialog.PopupCenteredRatio();
        }

        private void OnTagSelectionTreeButtonPressed(TreeItem item, int column, int id)
        {
            if (TagIndexes.Contains(id))
                TagIndexes.Remove(id);
            else
                TagIndexes.Add(id);
            FillTagSelectionTree();
        }

        private void OnConfirmed()
        {
            string[] plugins = Array.ConvertAll(NameListEditor.Text.Split(','), str => str.Trim());
            List<TreeEntity> treeEntities = new();
            foreach (string plugin in plugins)
            {
                if (plugin.Length == 0)
                    continue;
                var x = new Dictionary<string, object> {
                    {"name", plugin}
                };
                TreeEntity treeEntity = TreeEntityFactory.CreatePlugin();
                treeEntity.GetComponent<Name>().NameString = Format(NameEditor.Text, x);
                DAWProperties dawProperties = treeEntity.GetComponent<DAWProperties>();
                for (int idx = 0; idx < DAWProperties.DAWCount; idx++)
                {
                    dawProperties.ChangeQuery(Format(DAWNameEditors[idx].Text, x), idx);
                    dawProperties.ToggleFlag(DAWAvailableEditors[idx].Pressed, idx);
                }
                TagCollection tagCollection = treeEntity.GetComponent<TagCollection>();
                foreach (int tagIdx in TagIndexes)
                {
                    tagCollection.AddTag(PluginServer.Instance.TagList[tagIdx]);
                }
                treeEntities.Add(treeEntity);
            }

            if (EditorServer.Instance.SelectedTreeEntity is TreeFolder treeFolder)
            {
                treeEntities.ForEach(te => treeFolder.AddChild(te));
            }
            else
            {
                TreeEntity latest = EditorServer.Instance.SelectedTreeEntity;
                foreach (TreeEntity treeEntity in treeEntities)
                {
                    latest.Parent.AddChildAfter(treeEntity, latest);
                    latest = treeEntity;
                }
            }
            EditorServer.Instance.RefreshFolderEditor();
        }

        public void Popup()
        {
            if (EditorServer.Instance.SelectedTreeEntity is null)
            {
                WindowContainer.Instance.DisplayOutput("Tree entity is not selected.");
                return;
            }
            NameListEditor.Clear();
            NameEditor.Text = "{name}";
            DAWNameEditors.ForEach(item => item.Text = "{name}");
            DAWAvailableEditors.ForEach(item => item.Pressed = false);
            TagIndexes.Clear();
            FillTagSelectionTree();
            PopupCenteredRatio();
        }

        private void FillTagSelectionTree()
        {
            TagSelectionTree.GetRoot()?.Free();
            TreeItem root = TagSelectionTree.CreateItem();
            int idx = 0;
            foreach (Tag tag in PluginServer.Instance.TagList)
            {
                TreeItem treeItem = TagSelectionTree.CreateItem(root);
                treeItem.SetText(0, tag.Name);
                treeItem.SetIcon(0, Resources.ICON_TAG);
                treeItem.AddButton(0, TagIndexes.Contains(idx) ? Resources.ICON_REMOVE : Resources.ICON_ADD, idx++);
            }
            TagSelectionTree.Update();
        }

        private static string Format(string formatWithNames, IDictionary<string,object> data) {
            int pos = 0;
            var args = new List<object>();
            var fmt = Regex.Replace(
                formatWithNames,
                @"(?<={)[^}]+(?=})",
                new MatchEvaluator(m => {
                    var res = (pos++).ToString();
                    var tok = m.Groups[0].Value.Split(':');
                    args.Add(data[tok[0]]);
                    return tok.Length == 2 ? res+":"+tok[1] : res;
                })
            );
            return string.Format(fmt, args.ToArray());
        }
    }
}