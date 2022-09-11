using Godot;
using PluginManager.Editor;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree.Components
{
    // TODO: Reimplement DAW Properties
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
            Name name = TreeEntity.GetComponent<Name>();
            FolderComp folderComp = TreeEntity.GetComponent<FolderComp>();
            if (_TreeEntityRef is TreeFolder)
                TreeEntity.Icon = folderComp.Collapsed ? Resources.ICON_BOX_OPEN : Resources.ICON_BOX_CLOSE;
            else
                TreeEntity.Icon = Resources.ICON_NODE;
            treeItem.SetIconModulate(0, new Color("cb5b5b"));
            TreeEntity.Label = "[" + _TreeEntityRef.GetComponent<Name>().NameString + "]";
            name.ShallModifyTreeItem = !(folderComp.Collapsed || !name.Active);
            folderComp.ShallModifyTreeItem = false;
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
            foreach (Component comp in _TreeEntityRef.Components)
            {
                if (comp is not BaseOptional baseOpt)
                    continue;
                BaseOptional compMain = TreeEntity.GetComponent(baseOpt.GetType()) as BaseOptional;
                if (compMain is not null)
                    compMain.Copy(baseOpt);
            }
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

        public override Component Clone(Component newComponent = null)
        {
            ReferenceData newComp = newComponent as ReferenceData ?? new ReferenceData();
            newComp._TreeEntityRef = this._TreeEntityRef;
            return newComp;
        }

        public override void Serialize(JObject jobj, TreeEntityLookup TEL)
        {
            jobj.Add("ref", TEL.GetID(_TreeEntityRef));
        }

        public override void Deserialize(JObject jobj, TreeEntityLookup TEL)
        {
            if (jobj.GetValue("ref", -1) is int refIdx && refIdx != -1)
                _TreeEntityRef = TEL.GetTreeEntity(refIdx);
        }
    }
}
