using Godot;
using System;
using System.Collections.Generic;

namespace PluginManager.PluginTree
{
    public class Server : Godot.Object
    {
        [Signal]
        public delegate void FocusedFolderChanged(TreeFolder newFocusedFolder);

        private static Server _instance;
        private List<TreeFolder> _folderList = new();
        private List<Tag> _tagList = new();
        private TreeFolder _focusedFolder;
        public IList<TreeFolder> FolderList
        {
            get => _folderList.AsReadOnly();
        }
        public IList<Tag> TagList
        {
            get => _tagList.AsReadOnly();
        }
        public TreeFolder FocusedFolder
        {
            get => _focusedFolder;
        }
        public TreeEntity SelectedTreeEntity;

        // Singleton Instance

        public static Server Instance
        {   
            get 
            {
                if (_instance == null)
                {
                    _instance = new Server();
                }
                return _instance;
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
            if (treeFolder == _focusedFolder)
                ChangeFocusedFolder(null);
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

        public void ChangeFocusedFolder(TreeFolder newFocusedFolder)
        {
            _focusedFolder = newFocusedFolder;
            EmitSignal(nameof(FocusedFolderChanged), newFocusedFolder);
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