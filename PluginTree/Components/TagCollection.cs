using Godot;
using System.Collections.Generic;

namespace PluginManager.PluginTree
{
    public class TagCollection : Component
    {
        private List<Tag> _tagList = new();
        public IList<Tag> TagList
        {
            get => _tagList.AsReadOnly();
        }
        
        public void AddTag(Tag tag)
        {
            _tagList.Add(tag);
        }

        public void RemoveTag(Tag tag)
        {
            _tagList.Remove(tag);
        }
    }
}
