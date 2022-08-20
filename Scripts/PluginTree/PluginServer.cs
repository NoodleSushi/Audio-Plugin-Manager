using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using PluginManager.PluginTree.Components;
using GDArray = Godot.Collections.Array;

namespace PluginManager.PluginTree
{
    public class PluginServer : Node
    {
        [Signal]
        public delegate void Cleared();

        [Signal]
        public delegate void FolderDeleting(TreeFolder treeFolder);

        [Signal]
        public delegate void Deserialized();

        [Signal]
        public delegate void DAWListSizeChanged();

        private static PluginServer _instance;
        private List<TreeFolder> _folderList = new();
        private List<Tag> _tagList = new();
        private List<string> _dawList = new();

        public ReadOnlyCollection<TreeFolder> FolderList => _folderList.AsReadOnly();
        public ReadOnlyCollection<Tag> TagList => _tagList.AsReadOnly();
        public ReadOnlyCollection<string> DAWList => _dawList.AsReadOnly();
        public int DAWCount => _dawList.Count;
        public GDArray GDFolderList => new(_folderList);
        public GDArray GDDAWList => new(_dawList);

        // Singleton Instance
        public static PluginServer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new();
                    _instance.Clear();
                }
                return _instance;
            }
        }

        // Instantiate
        public override void _EnterTree()
        {
            Clear();
        }

        public void DAWListDefault()
        {
            _dawList.Add("FLStudio");
            _dawList.Add("Ableton");
        }

        // Some Methods
        public void Clear(bool loadDAWList = true)
        {
            _folderList = new();
            _tagList = new();
            _dawList = new();
            if (loadDAWList)
                DAWListDefault();
            EmitSignal(nameof(Cleared));
        }

        // IO
        public string Serialize()
        {
            JObject o = new();
            TreeEntityLookup TEL = new();

            // serialize tags
            if (_tagList.Count > 0)
            {
                o.Add("tags", new JArray(_tagList.Select<Tag, JObject>(tag => tag.Serialize())));
            }

            // serialize daws
            if (_dawList.Count > 0)
            {
                o.Add("daws", new JArray(_dawList));
            }

            // serialize folders
            o.Add("folders", new JArray(_folderList.Select<TreeFolder, int>(treeFolder => TEL.GetID(treeFolder))));

            // dump all other stuff
            o.Add("entities", TEL.DumpSerializedJArray());

            return o.ToString();
        }

        public bool Deserialize(string json)
        {
            Clear(false);
            JObject o;
            o = JObject.Parse(json);
            if (o.ContainsKey("tags") && o["tags"] is JArray)
            {
                foreach (JObject tagObj in o["tags"].Cast<JObject>())
                {
                    Tag newTag = new();
                    if (tagObj.Property("name") is JProperty nameProperty)
                    {
                        newTag.Name = (string)nameProperty.Value;
                    }
                    if (tagObj.Property("visible") is JProperty visibleProperty)
                    {
                        newTag.Visible = (bool)visibleProperty.Value;
                    }
                    _tagList.Add(newTag);
                }
            }

            if (o.ContainsKey("daws") && o["daws"] is JArray)
            {
                foreach (string daw in o["daws"].Select(v => (string)v))
                {
                    _dawList.Add(daw);
                }
            }
            else
            {
                DAWListDefault();
            }

            TreeEntityLookup TEL = new();
            if (o.TryGetValue("entities", out JToken entitiesToken))
            {
                JArray entities = entitiesToken.Value<JArray>();
                foreach (JObject entity in entities.Cast<JObject>())
                {
                    TEL.AddJObject(entity);
                }
            }
            else
            {
                Clear();
                return false;
            }

            if (o.TryGetValue("folders", out JToken foldersToken))
            {
                JArray folders = foldersToken.Value<JArray>();
                _folderList.AddRange(folders.Select(id => TEL.GetTreeEntity((int)id) as TreeFolder));
            }
            else
            {
                Clear();
                return false;
            }
            EmitSignal(nameof(Deserialized));
            return true;
        }

        // Folder Manipulation

        public TreeFolder CreateFolder()
        {
            TreeFolder newFolder = TreeEntityFactory.CreateFolder();
            newFolder.GetComponent<FolderComp>().Visible = false;
            _folderList.Add(newFolder);
            return newFolder;
        }

        public void RemoveFolder(TreeFolder treeFolder)
        {
            EmitSignal(nameof(FolderDeleting), treeFolder);
            _folderList.Remove(treeFolder);
        }

        public void ReorderFolderList(
            TreeFolder heldFolder,
            TreeFolder neighborFolder,
            int dropSection
        )
        {
            if (dropSection == 0)
                return;
            TreeFolder neighbor = _folderList[_folderList.IndexOf(neighborFolder)];
            if (neighbor == heldFolder)
                return;
            _folderList.Remove(heldFolder);
            _folderList.Insert(
                _folderList.IndexOf(neighbor) + ((dropSection == 1) ? 1 : 0),
                heldFolder
            );
        }

        // Tag Manipulation

        public Tag CreateTag()
        {
            Tag newTag = new() { Name = "Tag" };
            _tagList.Add(newTag);
            return newTag;
        }

        public void RemoveTag(Tag tag)
        {
            tag.NotifyDeletion();
            _tagList.Remove(tag);
        }

        public void ReorderTagList(Tag heldTag, Tag neighborTag, int dropSection)
        {
            if (dropSection == 0)
                return;
            Tag neighbor = _tagList[_tagList.IndexOf(neighborTag)];
            if (neighbor == heldTag)
                return;
            _tagList.Remove(heldTag);
            _tagList.Insert(_tagList.IndexOf(neighbor) + ((dropSection == 1) ? 1 : 0), heldTag);
        }
    }
}
