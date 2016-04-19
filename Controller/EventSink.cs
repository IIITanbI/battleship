using ONX.Cmn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    //public class EventSink : MarshalByRefObject
    //{
    //    private OnEventHandler handler_;

    //    public EventSink(OnEventHandler handler)
    //    {
    //        handler_ = handler;
    //    }

    //    [System.Runtime.Remoting.Messaging.OneWay]
    //    public void EventHandlerCallback(Turn turn)
    //    {
    //        if (handler_ != null)
    //        {
    //            handler_(turn);
    //        }
    //    }

    //    public void Register(IMyService service)
    //    {
    //        service.Ev += new OnEventHandler(EventHandlerCallback);
    //    }

    //    public void Unregister(IMyService service)
    //    {
    //        service.Ev -= new OnEventHandler(EventHandlerCallback);
    //    }
    //}
}
