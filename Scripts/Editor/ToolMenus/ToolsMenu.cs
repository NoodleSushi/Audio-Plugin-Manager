using System.Linq;
using System.Collections.Generic;
using PluginManager.Editor.Containers;
using PluginManager.PluginTree;
using PluginManager.PluginTree.Components;
using System;

namespace PluginManager.Editor.ToolMenus
{
    public class ToolsMenu : MenuButtonExtended
    {
        public override void _Ready()
        {
            base._Ready();
            AddItem(nameof(OnEnterPluginListPressed));
            AddItem(nameof(OnCopyFromReferencePressed));
            AddItem(nameof(OnSortFolderPressed));
            AddItem(nameof(OnRevSortFolderPressed));
            AddItem(nameof(OnRemoveDuplicates));
            AddItem(nameof(OnNestedRemoveDuplicates));
        }

        [PopupItemAttribute("Generate Plugins")]
        public void OnEnterPluginListPressed()
        {
            PluginMakerDialog pluginMakerDialog = new();
            WindowContainer.Instance.AddChild(pluginMakerDialog);
            pluginMakerDialog.MakeFreeable();
            pluginMakerDialog.Popup();
        }

        [PopupItemAttribute("Selected Copy From Reference")]
        public void OnCopyFromReferencePressed()
        {
            EditorServer.Instance.SelectedTreeEntities
                .ForEach(x => x.GetComponent<ReferenceData>()?.CopyFromRef());
            EditorServer.Instance.RefreshFolderEditor();
        }

        [PopupItemAttribute("Sort Folder")]
        public void OnSortFolderPressed()
        {
            if (EditorServer.Instance.SelectedTreeEntity is not TreeFolder treeFolder)
            {
                WindowContainer.Instance.DisplayError("Tree folder is not selected.");
                return;
            }
            treeFolder.SetChildren(
                treeFolder.Children.OrderBy(x => x.GetComponent<Name>()?.NameString ?? "")
            );
            EditorServer.Instance.RefreshFolderEditor();
        }

        [PopupItemAttribute("Reverse Sort Folder")]
        public void OnRevSortFolderPressed()
        {
            if (EditorServer.Instance.SelectedTreeEntity is not TreeFolder treeFolder)
            {
                WindowContainer.Instance.DisplayError("Tree folder is not selected.");
                return;
            }
            treeFolder.SetChildren(
                treeFolder.Children.OrderByDescending(x => x.GetComponent<Name>()?.NameString ?? "")
            );
            EditorServer.Instance.RefreshFolderEditor();
        }
        
        public void RemoveDuplicates(TreeFolder treeFolder, bool nested = false)
        {
            HashSet<string> uniqueNames = new();
            List<TreeEntity> garbage = new();
            foreach (TreeEntity child in treeFolder.Children)
            {
                if (child.GetComponent<Name>() is not Name nameComp)
                    return;
                string name = nameComp.NameString;
                if (uniqueNames.Contains(name))
                    garbage.Add(child);
                else
                    uniqueNames.Add(name);
            }
            garbage.ForEach(x => x.Unparent());
            garbage.Clear();
            if (nested)
            {
                foreach (TreeFolder subTreeFolder in treeFolder.Children.OfType<TreeFolder>())
                {
                    RemoveDuplicates(subTreeFolder, true);
                }
            }
        }
        
        [PopupItemAttribute("Remove Duplicate Names")]
        public void OnRemoveDuplicates()
        {
            if (EditorServer.Instance.SelectedTreeEntity is not TreeFolder treeFolder)
            {
                WindowContainer.Instance.DisplayError("Tree folder is not selected.");
                return;
            }
            RemoveDuplicates(treeFolder);
            EditorServer.Instance.RefreshFolderEditor();
        }

        [PopupItemAttribute("Nested Remove Duplicate Names")]
        public void OnNestedRemoveDuplicates()
        {
            if (EditorServer.Instance.SelectedTreeEntity is not TreeFolder treeFolder)
            {
                WindowContainer.Instance.DisplayError("Tree folder is not selected.");
                return;
            }
            RemoveDuplicates(treeFolder, true);
            EditorServer.Instance.RefreshFolderEditor();
        }
    }
}