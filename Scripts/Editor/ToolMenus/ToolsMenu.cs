using System.Linq;
using PluginManager.Editor.Containers;
using PluginManager.PluginTree;
using PluginManager.PluginTree.Components;

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
            AddItem(nameof(OnDeactivateOptionalsPressed));
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

        [PopupItemAttribute("Selected Deactivate Optionals")]
        public void OnDeactivateOptionalsPressed()
        {
            var comps = EditorServer.Instance.SelectedTreeEntities
                .SelectMany(x => x.Components)
                .OfType<BaseOptional>()
                .Where(x => x.isOptional);
            foreach (var comp in comps)
                comp.Active = false;
        }

        public void OnModifyOptionalsActivePressed()
        {
            var optionalTypes = typeof(BaseOptional).Assembly.GetTypes()
                .Where(x => x.IsSubclassOf(typeof(BaseOptional)));
            foreach (var type in optionalTypes)
            {
                // type.Name;
            }
        }
    }
}