using ONX.Cmn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    public class EventSink : MarshalByRefObject
    {
        private OnEventHandler handler_;

        public EventSink(OnEventHandler handler)
        {
            handler_ = handler;
        }

        [System.Runtime.Remoting.Messaging.OneWay]
        public void EventHandlerCallback(string text)
        {
            if (handler_ != null)
            {
                handler_(text);
            }
        }

        public void Register(Controller service)
        {
            service.Ev += new OnEventHandler(EventHandlerCallback);
        }

        public void Unregister(Controller service)
        {
            service.Ev -= new OnEventHandler(EventHandlerCallback);
        }
    }
}
