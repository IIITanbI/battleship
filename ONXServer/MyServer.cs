using System;
using System.Runtime.Remoting;
using ONX.Cmn;

namespace ONX.Server
{
    class MyServer
    {
        [STAThread]
        static void Main(string[] args)
        {
            RemotingConfiguration.Configure("ONXServer.exe.config");
            Utils.DumpAllInfoAboutRegisteredRemotingTypes();

            Log.WaitForEnter("Press EXIT to stop MyService host...");
        }
    }
}
