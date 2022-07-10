using System;

namespace PluginManager.PluginTree
{
    public class DAWProperties: Component
    {
        [Flags]
        public enum DAWFlags
        {
            None = 0,
            FLStudio = 1,
            Ableton = 2,
            All = FLStudio | Ableton,
        }

        public enum DAW
        {
            FLStudio = 0,
            Ableton = 1,
        }

        const int DAWCount = 2;
        private string[] _DAWQueries = new string[DAWCount];
        public DAWFlags Flags = DAWFlags.None;

        public string GetDAWQuery(DAW idx)
        {
            return _DAWQueries[(int)idx];
        }

        public void SetDAWQuery(DAW idx, string newQuery)
        {
            _DAWQueries[(int)idx] = newQuery;
        }
    }
}
