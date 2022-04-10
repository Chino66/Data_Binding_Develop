using System;
using System.Collections.Generic;

namespace DataBinding
{
    public class PropertyEvent
    {
        public List<Action<object>> GetEvent;
        public List<Action<object>> SetEvent;

        public PropertyEvent()
        {
            GetEvent = new List<Action<object>>();
            SetEvent = new List<Action<object>>();
        }
    }
}