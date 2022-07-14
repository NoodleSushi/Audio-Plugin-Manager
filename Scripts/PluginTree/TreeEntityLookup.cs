using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace PluginManager.PluginTree
{
    public class TreeEntityLookup : Godot.Object
    {
        private int _IDIter = 0;
        private Dictionary<TreeEntity, int> _TreeEntity2ID = new();
        private Dictionary<int, TreeEntity> _ID2TreeEntity = new();
        private Dictionary<int, JObject> _ID2JObject = new();

        public int GetID(TreeEntity treeEntity)
        {
            if (!_TreeEntity2ID.ContainsKey(treeEntity))
            {
                JObject jobj = treeEntity.Serialize(this);
                _ID2JObject[_IDIter] = jobj;
                _ID2TreeEntity[_IDIter] = treeEntity;
                _TreeEntity2ID[treeEntity] = _IDIter;
                _IDIter++;
            }
            return _TreeEntity2ID[treeEntity];
        }

        public void AddJObject(JObject jobject)
        {
            _ID2JObject[_IDIter] = jobject;
            _IDIter++;
        }

        public TreeEntity GetTreeEntity(int id)
        {
            if (!_ID2TreeEntity.ContainsKey(id))
            {
                if (_ID2JObject.ContainsKey(id))
                {
                    JObject jobj = _ID2JObject[id];
                    TreeEntity treeEntity = TreeEntityFactory.CreateTreeEntityByIdentifier((string) jobj["type"]);
                    _ID2TreeEntity[id] = treeEntity;
                    _TreeEntity2ID[treeEntity] = id;
                    treeEntity.Deserialize(jobj, this);
                }
                else
                {
                    return null;
                }
            }
            return _ID2TreeEntity[id];
        }

        public void Clear()
        {
            _IDIter = 0;
            _TreeEntity2ID = new();
            _ID2TreeEntity = new();
            _ID2JObject = new();
        }

        public JArray DumpSerializedJArray()
        {
            JArray jArray = new();
            for (int i = 0; i < _IDIter; i++)
            {
                jArray.Add(_ID2JObject[i]);
            }
            return jArray;
        }
    }
}
