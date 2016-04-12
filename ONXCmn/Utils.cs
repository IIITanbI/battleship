using System;
using System.Runtime.Remoting;

namespace ONX.Cmn
{
	public class Utils
	{
        private static void DumpTypeEntries(Array arr)
        {
            foreach(object obj in arr)
            {
                Log.Print("  {0}: {1}", obj.GetType().Name, obj);
            }
        }

        public static void DumpAllInfoAboutRegisteredRemotingTypes()
        {
            Log.Print("ALL REGISTERED TYPES IN REMOTING -(BEGIN)---------");

            DumpTypeEntries(RemotingConfiguration.GetRegisteredActivatedClientTypes());
            DumpTypeEntries(RemotingConfiguration.GetRegisteredActivatedServiceTypes());
            DumpTypeEntries(RemotingConfiguration.GetRegisteredWellKnownClientTypes());
            DumpTypeEntries(RemotingConfiguration.GetRegisteredWellKnownServiceTypes());

            Log.Print("ALL REGISTERED TYPES IN REMOTING -(END)  ---------");
        }
	}
}
