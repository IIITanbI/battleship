using System;
using System.Runtime.Remoting;
using ONX.Cmn;
using ONX.Server;
using ONXCmn.Logic;
using System.Collections.Generic;

namespace ONX.Client
{
    public static class Utility
    {
        public static int ReadInt(string text = null)
        {
            int res = 0;

            do
            {
                Console.WriteLine(text);
            }
            while (!int.TryParse(Console.ReadLine(), out res));

            return res;
        }
        public static int ReadIntInRange(int from, int to, string text = null)
        {
            int res = 0;

            do
            {
                Console.WriteLine(text);
            }
            while (!(int.TryParse(Console.ReadLine(), out res) && (res >= from && res <= to)));

            return res;
        }
        public static int ReadIntInSequence(List<int> sequence, string text = null)
        {
            int res = 0;

            do
            {
                Console.WriteLine(text);
            }
            while (!(int.TryParse(Console.ReadLine(), out res) && sequence.Contains(res)));

            return res;
        }
    }


    public class MyClient
    {
        struct point
        {
            public int columnStart;
            public int rowStart;

            public int columnEnd;
            public int rowEnd;

            public point(int rowStart, int columnStart, int rowEnd, int columnEnd)
            {
                this.rowStart = rowStart;
                this.rowEnd = rowEnd;

                this.columnStart = columnStart;
                this.columnEnd = columnEnd;
            }
        }

        private static IMyService GetMyService()
        {
            return
                Activator.GetObject(typeof(IMyService), "tcp://localhost:33000/MyServiceUri")
                as IMyService;
        }

        IMyService myService1;

        Dictionary<int, int> availableShips = new Dictionary<int, int>();

        point shipsPoint = new point(30, 0, 35, 40);

        void ClearConsole(point p)
        {
            for (int row = p.rowStart; row <= p.rowEnd; row++)
            {
                Console.SetCursorPosition(p.columnStart, row);
                for (int column = p.columnStart; column <= p.columnEnd; column++)
                {
                    Console.Write(' ');
                }
            }
        }

        public void PrintShips()
        {
            ClearConsole(shipsPoint);
            Console.SetCursorPosition(shipsPoint.columnStart, shipsPoint.rowStart);
            foreach (var pair in availableShips)
            {
                int length = pair.Key;
                int count = pair.Value;

                for(int i = 0; i < length; i++)
                {
                    Console.Write('*');
                }
                Console.CursorLeft = 10;
                Console.WriteLine(count);
                Console.WriteLine();
            }
        }

        public void GO()
        {
            //myService1 = GetMyService();
            //myService1 = new MyService();
            //Log.Print("myService1 created. Proxy? {0}", (RemotingServices.IsTransparentProxy(myService1) ? "YES" : "NO"));


            int n = Utility.ReadInt("Enter battleground size:");
            Battleground battleGround = new Battleground(n);

            battleGround.Print();

            #region Init available ships
            int totalCount = 0;
            for (int length = 1; length <= 4; length++)
            {
                int count = 5 - length;
                availableShips[length] = count;
                totalCount += count;
            }
            #endregion


            while (totalCount > 0)
            {
                int length = Utility.ReadIntInRange(1, 4, "Enter ship length:");
                if (availableShips.ContainsKey(length) && availableShips[length] > 0)
                {
                    ShipOrientation orientation = (ShipOrientation)Utility.ReadIntInRange(1, 2, "Enter ship orientation:");
                    Ship ship = new Ship(length, orientation);

                    Console.Clear();
                    battleGround.Draw();

                    Console.CursorVisible = false;
                    Move(battleGround, ship);
                    Console.CursorVisible = !false;
                    Console.SetCursorPosition(0, n);
                    totalCount--;

                    availableShips[length]--;
                    PrintShips();
                }
            }

            //var turn = new Turn();
            //turn.x = 0;
            //YouTurn(turn);

            //myService1.YouTurn(turn);
        }

        public void Move(Battleground battleground, Ship ship)
        {
            Point init = ship.Position;
            battleground.MoveTo(ship, new Point(0,0));
            ConsoleKeyInfo keyinfo;
            do
            {
                keyinfo = Console.ReadKey();
                Point next = ship.Position;
                switch (keyinfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (next.column > 0)
                            next.column--;
                        break;
                    case ConsoleKey.RightArrow:
                        if (next.column < battleground.N - 1)
                            next.column++;
                        break;
                    case ConsoleKey.UpArrow:
                        if (next.row > 0)
                            next.row--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (next.row < battleground.N - 1)
                            next.row++;
                        break;
                    case ConsoleKey.Enter:
                        if (ship.Status == ShipStatus.Full)
                            return;
                        else break;
                    default:
                        break;
                }
                battleground.MoveTo(ship, next);


                Console.SetCursorPosition(0, 40);
                Console.WriteLine($"Position: {ship.Position.row + 1}, {ship.Position.column + 1}");
            }
            while (true);
        }

        public void YouTurn(Turn del)
        {
            while (del.x < 100)
            {
                Log.Print($"Server turn: {del.x}");
                Log.WaitForEnter(" you turn");
                del.x++;
                del = this.myService1?.YouTurn(del);
            }
            //return string.Format("result", del.x);
        }


        [STAThread]
        static void Main(string[] args)
        {
            //RemotingConfiguration.Configure("ONXClient.exe.config");
            //Utils.DumpAllInfoAboutRegisteredRemotingTypes();

            new MyClient().GO();

            Log.WaitForEnter("Press ENTER to exit...");
        }
    }
}
