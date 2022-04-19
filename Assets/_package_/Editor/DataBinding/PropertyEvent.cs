using System;
using System.Collections.Generic;

namespace DataBinding
{
    public class PropertyEvent
    {
        // public Action<object> PreGetEvent { get; set; }
        // public Action<object> PostGetEvent { get; set; }
        // public Action<object> PreSetEvent { get; set; }
        // public Action<object> PostSetEvent { get; set; }
        public virtual void Dispose()
        {
        }
    }

    public class PropertyEvent<T> : PropertyEvent
    {
        public Action<T> PreGetEvent { get; set; }
        public Action<T> PostGetEvent { get; set; }
        public Action<T> PreSetEvent { get; set; }
        public Action<T> PostSetEvent { get; set; }

        public override void Dispose()
        {
            PreGetEvent = null;
            PostGetEvent = null;
            PreSetEvent = null;
            PostSetEvent = null;
        }
    }
}