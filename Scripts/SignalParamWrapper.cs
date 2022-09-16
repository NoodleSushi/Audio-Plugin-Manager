using Godot;

namespace PluginManager
{
    public class SignalParamWrapper : Object
    {
        private readonly object value;

        public SignalParamWrapper(object value)
        {
            this.value = value;
        }

        public T GetValue<T>()
        {
            return (T)value;
        }
    }
}