using System;
using ONX.Cmn;
using ONXCmn.Logic;

namespace ONX.Server
{
	public class MyService : MarshalByRefObject, IMyService
	{
        private static int Id_ = 0;

        private int id_;

		public MyService()
		{
            id_ = System.Threading.Interlocked.Increment(ref Id_);
            Log.Print("Instance of MyService is created, MyService.id={0}", id_);
		}

        public Turn YouTurn(Turn del)
        {
            Log.Print($"Clietn turn: {del.x}");
            del.x++;
            return del;
            //return string.Format("result", del.x);
        }

    }
}
