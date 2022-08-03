using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    public class ReferenceData : Component
    {
        private TreeEntity _TreeEntityRef;
        public TreeEntity TreeEntityRef
        {
            get => _TreeEntityRef;
            set
            {
                _TreeEntityRef = value;
                if (_TreeEntityRef is not null)
                {
                    _TreeEntityRef.GetComponent<Name>().Connect(nameof(Name.NameChanged), this, nameof(OnRefNameChanged));
                }
            }
        }

        public override string GetName() => "Reference Data";

        public override void ModifyTreeItem(TreeItem treeItem)
        {
            if (_TreeEntityRef is TreeFolder)
                TreeEntity.Icon =
                    (TreeEntity.GetComponent<FolderComp>().Collapsed) ? Resources.ICON_BOX_OPEN : Resources.ICON_BOX_CLOSE;
            else
                TreeEntity.Icon = Resources.ICON_NODE;
            treeItem.SetIconModulate(0, new Color("cb5b5b"));
            TreeEntity.Label = "[" + _TreeEntityRef.GetComponent<Name>().NameString + "]";
            // TreeEntity.GetComponent<Name>().ShallModifyTreeItem = (!TreeEntity.GetComponent<FolderComp>().Collapsed && IsModified);
            TreeEntity.GetComponent<Name>().ShallModifyTreeItem = (
                !(TreeEntity.GetComponent<FolderComp>().Collapsed || !TreeEntity.GetComponent<Name>().Active)
            );
            TreeEntity.GetComponent<FolderComp>().ShallModifyTreeItem = false;
        }

        public string GetEntityRefPath()
        {
            string path = _TreeEntityRef.GetComponent<Name>().NameString;
            for (TreeEntity trav = _TreeEntityRef; trav.Parent != null; trav = trav.Parent)
            {
                path = trav.Parent.GetComponent<Name>().NameString + "/" + path;
            }
            return path;
        }

        public override void GenerateProperties()
        {
            base.GenerateProperties();
            using (Label newLabel = new())
            {
                newLabel.Text = "Directory:";
                newLabel.Align = Label.AlignEnum.Center;
                EditorServer.Instance.AddProperty(newLabel);
            }

            using (RichTextLabel newLabel = new())
            {
                newLabel.Text = GetEntityRefPath();
                newLabel.RectMinSize = new Vector2(0, 64);
                EditorServer.Instance.AddProperty(newLabel);
            }

            using (Button newButton = new())
            {
                newButton.Text = "Copy From Reference";
                newButton.Connect("pressed", this, nameof(OnCopyFromRefButtonPressed));
                EditorServer.Instance.AddProperty(newButton);
            }
            TreeEntity.GetComponent<FolderComp>().Visible = _TreeEntityRef is TreeFolder;
            TreeEntity.GetComponent<Name>().isOptional = true;
            TreeEntity.GetComponent<DAWProperties>().isOptional = true;
            TreeEntity.GetComponent<TagCollection>().isOptional = true;
        }

        public void CopyFromRef()
        {
            DAWProperties thsProperties = TreeEntity.GetComponent<DAWProperties>();
            DAWProperties refProperties = _TreeEntityRef.GetComponent<DAWProperties>();
            thsProperties.Flags = refProperties.Flags;
            thsProperties.DAWQueries = (string[])refProperties.DAWQueries.Clone();
            TreeEntity.GetComponent<Name>().NameString = _TreeEntityRef
                .GetComponent<Name>()
                .NameString;
        }

        public void OnCopyFromRefButtonPressed()
        {
            CopyFromRef();
            TreeEntity.DeferredGenerateProperties();
            TreeEntity.DeferredUpdateTreeItem();
        }

        public void OnRefNameChanged(string newName)
        {
            TreeEntity.DeferredUpdateTreeItem();
        }

        public override Component Clone()
        {
            ReferenceData newComp = new()
            {
                _TreeEntityRef = this._TreeEntityRef
            };
            return newComp;
        }

        public override void Serialize(JObject jobj, TreeEntityLookup TEL)
        {
            jobj.Add("ref", TEL.GetID(_TreeEntityRef));
        }

        public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            _TreeEntityRef = TEL.GetTreeEntity((int)jobj["ref"]);
        }
    }
}
