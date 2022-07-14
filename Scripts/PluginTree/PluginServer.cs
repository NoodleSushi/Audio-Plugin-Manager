using Godot;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace PluginManager.PluginTree
{
    public class PluginServer : Godot.Object
    {
        [Signal]
        public delegate void Cleared();
        [Signal]
        public delegate void FolderDeleting(TreeFolder treeFolder);
        [Signal]
        public delegate void Deserialized();

        private static PluginServer _instance;
        private List<TreeFolder> _folderList = new();
        private List<Tag> _tagList = new();
        public IList<TreeFolder> FolderList => _folderList.AsReadOnly();
        public IList<Tag> TagList => _tagList.AsReadOnly();

        // Singleton Instance
        public static PluginServer Instance
        {   
            get 
            {
                if (_instance == null)
                {
                    _instance = new();
                }
                return _instance;
            }
        }

        // Some Methods
        public void Clear()
        {
            _folderList = new();
            _tagList = new();
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
                JArray jTagList = new();
                foreach (Tag tag in _tagList)
                {
                    jTagList.Add(tag.Serialize());
                }
                o.Add("tags", jTagList);
            }

            // serialize folders
            JArray jFolderList = new();
            foreach (TreeFolder treeFolder in _folderList)
            {
                jFolderList.Add(TEL.GetID(treeFolder));
            }
            o.Add("folders", jFolderList);
            
            // dump all other stuff
            o.Add("entities", TEL.DumpSerializedJArray());

            return o.ToString();
        }

        public void Deserialize(string json)
        {
            JObject o;
            try
            {
                o = JObject.Parse(json);
            }
            catch (JsonReaderException)
            {
                return;
            }
            Clear();
            try
            {
                if (o.ContainsKey("tags") && o["tags"] is JArray)
                {
                    foreach (JObject tagObj in o["tags"])
                    {
                        Tag newTag = new Tag();
                        if (tagObj.Property("name") is JProperty nameProperty)
                        {
                            newTag.Name = (string) nameProperty.Value;
                        }
                        if (tagObj.Property("visible") is JProperty visibleProperty)
                        {
                            newTag.Visible = (bool) visibleProperty.Value;
                        }
                        _tagList.Add(newTag);
                    }
                }
                TreeEntityLookup TEL = new();
                JToken entitiesToken;
                JToken foldersToken;
                if (o.TryGetValue("entities", out entitiesToken))
                {
                    JArray entities = entitiesToken.Value<JArray>();
                    foreach (JObject entity in entities)
                    {
                        TEL.AddJObject(entity);
                    }
                }
                else
                {
                    Clear();
                    return;
                }

                if (o.TryGetValue("folders", out foldersToken))
                {
                    JArray folders = foldersToken.Value<JArray>();
                    foreach (int id in folders)
                    {
                        _folderList.Add(TEL.GetTreeEntity(id) as TreeFolder);
                    }
                }                
                else
                {
                    Clear();
                    return;
                }
                EmitSignal(nameof(Deserialized));
            }
            catch (System.Exception)
            {
                Clear();
            }
        }

        // Folder Manipulation

        public TreeFolder CreateFolder()
        {
            TreeFolder newFolder = TreeEntityFactory.CreateFolder();
            _folderList.Add(newFolder);
            return newFolder;
        }

        public void RemoveFolder(TreeFolder treeFolder)
        {
            EmitSignal(nameof(FolderDeleting), treeFolder);
            _folderList.Remove(treeFolder);
        }

        public void ReorderFolderList(TreeFolder heldFolder, TreeFolder neighborFolder, int dropSection)
        {
            if (dropSection == 0)
                return;
            TreeFolder neighbor = _folderList[_folderList.IndexOf(neighborFolder)];
            if (neighbor == heldFolder)
                return;
            _folderList.Remove(heldFolder);
            _folderList.Insert(_folderList.IndexOf(neighbor) + ((dropSection == 1) ? 1 : 0), heldFolder);
        }

        // Tag Manipulation

        public Tag CreateTag()
        {
            Tag newTag = new()
            {
                Name = "Tag"
            };
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