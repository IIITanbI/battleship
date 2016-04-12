using System;

namespace ONX.Cmn
{
    public class Log
    {
        private Log()
        {
        }

        public static void Print(string text, params object[] args)
        {
            DoLog("", text, args);
        }

        public static void Info(string text, params object[] args)
        {
            DoLog("INFO ", text, args);
        }

        public static void Warning(string text, params object[] args)
        {
            DoLog("WARNING ", text, args);
        }

        public static void Error(string text, params object[] args)
        {
            DoLog("!!!ERROR!!! ", text, args);
        }

        public static void WaitForEnter()
        {
            WaitForEnter("Press ENTER to continue...");
        }

        public static void WaitForEnter(string prompt)
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.Write(prompt);
            Console.ReadLine();
        }

        private static void DoLog(string prefix, string text, params object[] args)
        {
            // For newer .NET Frameworks you should use 
            // "System.Threading.Thread.CurrentThread.ManagedThreadId()"
            // as "AppDomain.GetCurrentThreadId()" is depreceted
            // there. But unfortunately it is not available in .NET v.1.1
            int threadId = AppDomain.GetCurrentThreadId();
            Console.Write("[{0:D4}] [{1}] ",
                threadId,
                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            Console.Write(prefix);
            Console.WriteLine(text, args);
        }
    } 	
}
