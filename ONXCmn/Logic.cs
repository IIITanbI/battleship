using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ONXCmn.Logic
{
    public enum ShipOrientation
    {
        Vertical = 1,
        Horizontal = 2
    }

    public enum ShipStatus
    {
        NotInitialized,
        Full,
        Ranen,
        Dead
    }


    public class Ship : IBattleble
    {
        public const int MAX_LENGTH = 4;
        public const int MIN_LENGTH = 1;

        public int ConfigID { get; private set; }
        public int Length { get; } = MIN_LENGTH;
        public Point Position { get; set; }
        public ShipOrientation Orientation { get; set; } = ShipOrientation.Horizontal;
        public ShipStatus Status { get; set; } = ShipStatus.NotInitialized;

        private ISet<Point> DamagedPoint { get; set; } = new HashSet<Point>();
        public Battleground Parent { get; set; }

        public Ship(ShipConfig config)
        {
            this.ConfigID = config.ID;
            this.Length = config.Length;
        }
        public Ship(int length)
        {
            if (length < MIN_LENGTH || length > MAX_LENGTH)
            {
                throw new ArgumentOutOfRangeException($"Ship length can be in range {MIN_LENGTH} - {MAX_LENGTH}");
            }
            this.Length = length;
        }
        public Ship(int length, ShipOrientation orientation)
            : this(length)
        {
            this.Orientation = orientation;
        }
        public Ship(int length, ShipOrientation orientation, Point Point)
            : this(length, orientation)
        {
            this.Position = Point;
        }
        public Ship(int length, Point Point)
            : this(length)
        {
            this.Position = Point;
        }

        public bool DamagePoint(Point point)
        {
            if (!GetOwnNeededSpace().Contains(point))
                return false;

            var res = DamagedPoint.Add(point);
            if (res)
            {
                if (DamagedPoint.Count == GetOwnNeededSpace().AllPoints().Count)
                    this.Status = ShipStatus.Dead;
                else
                    this.Status = ShipStatus.Ranen;
            }
            return res;
        }
        public bool IsDamaged(Point point)
        {
            return DamagedPoint.Contains(point);
        }

        public Rectangle GetOwnNeededSpace()
        {
            Point from = Position;
            Point to = Position;

            if (Orientation == ShipOrientation.Horizontal)
            {
                to.Column += Length - 1;
            }
            else if (Orientation == ShipOrientation.Vertical)
            {
                to.Row += Length - 1;
            }

            return new Rectangle(from, to);
        }
        public Rectangle GetTotalNeededSpace()
        {
            Rectangle rectangle = GetOwnNeededSpace();

            rectangle.From.Column--;
            rectangle.From.Row--;

            rectangle.To.Column++;
            rectangle.To.Row++;

            return rectangle;
        }
    }

    public class Barrier : IBattleble
    {
        public const int MAX_LENGTH = 4;
        public const int MIN_LENGTH = 1;

        public int Length { get; } = MIN_LENGTH;
        public Point Position { get; set; }
        public ShipOrientation Orientation { get; set; } = ShipOrientation.Horizontal;

        public Battleground Parent { get; set; }

        public Barrier(int length)
        {
            if (length < MIN_LENGTH || length > MAX_LENGTH)
            {
                throw new ArgumentOutOfRangeException($"Ship length can be in range {MIN_LENGTH} - {MAX_LENGTH}");
            }
            this.Length = length;
        }
        public Barrier(int length, ShipOrientation orientation)
            : this(length)
        {
            this.Orientation = orientation;
        }
        public Barrier(int length, ShipOrientation orientation, Point Point)
            : this(length, orientation)
        {
            this.Position = Point;
        }
        public Barrier(int length, Point Point)
            : this(length)
        {
            this.Position = Point;
        }

        public Rectangle GetOwnNeededSpace()
        {
            return new Rectangle(Position, Position);
        }
        public Rectangle GetTotalNeededSpace()
        {
            return GetOwnNeededSpace();
        }
    }

    public struct Point
    {
        public int Column;
        public int Row;

        public Point(int row, int column)
        {
            this.Column = column;
            this.Row = row;
        }


        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.Row - p2.Row, p1.Column - p2.Column);
        }
        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.Row + p2.Row, p1.Column + p2.Column);
        }
        public static bool operator ==(Point p1, Point p2)
        {
            return p1.Row == p2.Row && p1.Column == p2.Column;
        }
        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }

    }
    public struct Rectangle
    {
        public Point From;
        public Point To;

        public Rectangle(Point from, Point to)
        {
            this.From = from;
            this.To = to;
        }
        public bool IsNormalize()
        {
            return (From.Column <= To.Column) && (From.Row <= To.Row);
        }
        public void Normalize()
        {
            if (From.Column > To.Column)
                Util.Swap(ref From, ref To);

            if (From.Row > To.Row)
                Util.Swap(ref From.Row, ref To.Row);
        }

        public static bool operator ==(Rectangle p1, Rectangle p2)
        {
            return p1.From == p2.From && p1.To == p2.To;
        }
        public static bool operator !=(Rectangle p1, Rectangle p2)
        {
            return !(p1 == p2);
        }

        public bool Contains(Point point)
        {
            Normalize();

            return (point.Row >= From.Row && point.Row <= To.Row) &&
                   (point.Column >= From.Column && point.Column <= To.Column);
        }
        public bool Contains(Rectangle rectangle)
        {
            Normalize();
            rectangle.Normalize();

            //if (rectangle.From.Row    >= From.Row    && rectangle.From.Row    <= To.Row &&
            //    rectangle.From.Column >= From.Column && rectangle.From.Column <= To.Column)
            //{
            //    if (rectangle.To.Row    >= From.Row    && rectangle.To.Row    <= To.Row &&
            //        rectangle.To.Column >= From.Column && rectangle.To.Column <= To.Column)
            //    {

            //    }
            //}

            return (rectangle.From.Row >= From.Row && rectangle.To.Row <= To.Row &&
                    rectangle.From.Column >= From.Column && rectangle.To.Column <= To.Column);
        }

        public HashSet<Point> IntersectBy(Rectangle rectangle)
        {
            HashSet<Point> intersectPoints = new HashSet<Point>();
            var allPoints = AllPoints();
            foreach (Point point in allPoints)
            {
                if (rectangle.Contains(point))
                    intersectPoints.Add(point);
            }
            return intersectPoints;
        }

        public HashSet<Point> AllPoints()
        {
            HashSet<Point> points = new HashSet<Point>();

            Normalize();
            for (int row = From.Row; row <= To.Row; row++)
            {
                for (int column = From.Column; column <= To.Column; column++)
                {
                    points.Add(new Point(row, column));
                }
            }

            return points;
        }
    }


    public enum GameProcessStatus
    {
        InitializingGround,
        Battle
    }
    /*
    PointState
            Free = 0,
            AliveShip = 1,
            KillShip = 2,
            Bound = -1
    */


    public class Battleground
    {
        public const int MAX_N = 20;
        public const int MIN_N = 10;

        public Rectangle ground;
        //private int[][] matrix;
        public int N { get; }
        public GameProcessStatus GameStatus { get; private set; } = GameProcessStatus.InitializingGround;
        public HashSet<IBattleble> Objects { get; } = new HashSet<IBattleble>();

        public GameConfig GameConfig { get; set; } = null;
        public void Reset()
        {
            Objects.Clear();
            GameStatus = GameProcessStatus.InitializingGround;
        }

        public bool IsGameOver { get { return Objects.OfType<Ship>().All(s => s.Status == ShipStatus.Dead); } }



        //n - size of battleground
        public Battleground(int n, GameConfig config)
        {
            if (n < MIN_N || n > MAX_N)
            {
                throw new ArgumentOutOfRangeException($"N can be in range {MIN_N} - {MAX_N}");
            }
            this.N = n;
            ground = new Rectangle(new Point(0, 0), new Point(n - 1, n - 1));

            this.GameConfig = config;
            //matrix = new int[n][];
            //for (int i = 0; i < n; i++)
            //    matrix[i] = new int[n];
        }

        public void Battle()
        {
            this.GameStatus = GameProcessStatus.Battle;
        }

        public bool AddShip(Ship ship)
        {
            if (GameStatus == GameProcessStatus.Battle)
                return false;

            if (Objects.Contains(ship))
                throw new ArgumentException("This ship already in battlleground");
            //check
            if (!CanPlace(ship))
                return false;

            ship.Parent = this;
            ship.Status = ShipStatus.Full;
            return Objects.Add(ship);
        }
        public bool AddBarrier(Barrier barrier)
        {
            barrier.Parent = this;
            return Objects.Add(barrier);
        }

        public bool DeleteShip(Ship ship)
        {
            if (GameStatus == GameProcessStatus.Battle)
                return false;
            ship.Parent = null;
            return Objects.Remove(ship);
        }
        public bool DeleteBarrier(Barrier barrier)
        {
            barrier.Parent = null;
            return Objects.Remove(barrier);
        }

        public bool CanPlace(Ship ship)
        {
            Rectangle ownSpace = ship.GetOwnNeededSpace();
            Rectangle totalSpace = ship.GetTotalNeededSpace();

            if (AreaIsFree(ownSpace, true))
            {
                if (AreaIsFree(totalSpace, false))
                {
                    return true;
                }
            }
            return false;
        }

        public HashSet<Point> GetDeniedPoints(Ship ship)
        {
            var points = new HashSet<Point>();

            Rectangle totalArea = ship.GetTotalNeededSpace();

            totalArea.AllPoints().ToList().ForEach(p =>
            {
                if (!PointIsFree(p))
                    points.Add(p);
            });
            return points;
        }

        public bool AreaIsFree(Rectangle rectangle, bool strictMode)
        {
            rectangle.Normalize();

            //if (strictMode)
            //{
            //    if (!ground.Contains(rectangle))
            //        return false;
            //}

            bool isFree = true;

            //int toRow = Math.Min(rectangle.To.Row, N - 1);
            //int toColumn = Math.Min(rectangle.To.Column, N - 1);

            foreach (var point in rectangle.AllPoints())
            {
                if (!PointIsFree(point, strictMode))
                {
                    isFree = false;
                    break;
                }
            }
            //for (int row = rectangle.From.Row; row <= toRow; row++)
            //{
            //    for (int column = rectangle.From.Column; column <= toColumn; column++)
            //    {
            //        if (!PointIsFree(row, column))
            //        {
            //            isFree = false;
            //            break;
            //        }
            //    }
            //}

            return isFree;
        }

        public bool PointIsFree(Point point, bool strictMode = false)
        {
            return !Objects.Any(o => o.GetOwnNeededSpace().Contains(point)) && !(strictMode && !ground.Contains(point));
        }
        public bool PointIsShip(Point point)
        {
            return Objects.OfType<Ship>().Any(s => s.GetOwnNeededSpace().Contains(point));
        }
        public bool PointIsAttackShip(Point point)
        {
            return Objects.OfType<Ship>().Any(s => s.IsDamaged(point));
        }


        public Ship GetShipAtPoint(Point point)
        {
            return Objects.OfType<Ship>().FirstOrDefault(s => s.GetOwnNeededSpace().Contains(point));
        }
        public Barrier GetBarrierAtPoint(Point point)
        {
            return Objects.OfType<Barrier>().FirstOrDefault(s => s.GetOwnNeededSpace().Contains(point));
        }
        public T GetObjectAtPoint<T>(Point point) where T : IBattleble
        {
            return Objects.OfType<T>().FirstOrDefault(s => s.GetOwnNeededSpace().Contains(point));
        }

        public bool DamagePoint(Point point)
        {
            var res = Objects.OfType<Ship>().Any(s => s.DamagePoint(point));
            if (!res)
            {
                AddBarrier(new Barrier(1, point));
            }
            return res;
        }

        private int[] _x = { 0, -1, 0, 1 };
        private int[] _y = { -1, 0, 1, 0 };

        public bool ForceDamagePoint(Point point)
        {
            var ships = new List<Ship>();

            if (!PointIsFree(point))
                return false;

            for (int i = 0; i < 4; i++)
            {
                Point temp = new Point(point.Row + _y[i], point.Column + _x[i]);
                Ship ship = GetShipAtPoint(temp);
                if (ship != null)
                {
                    ships.Add(ship);
                }
            }

            if (ships.Count == 0)
            {
                Ship nShip = new Ship(1, point);
                foreach (var pp in nShip.GetOwnNeededSpace().AllPoints())
                {
                    nShip.DamagePoint(pp);
                }
                nShip.Status = ShipStatus.Ranen;
                return ForceAddShip(nShip);
            }
            else
            {
                int length = 0;

                foreach (var s in ships)
                {
                    if (s.IsDamaged(point))
                        return false;
                }

                foreach (var s in ships)
                {
                    length += s.Length;
                    ForceDeleteShip(s);
                }

                var first = ships.First();

                var st = first.Position;
                if (point.Row <= st.Row && point.Column <= st.Column)
                {
                    st = point;
                }

                Ship nShip = new Ship(length + 1, st);

                if (point.Row == first.Position.Row)
                    nShip.Orientation =  ShipOrientation.Horizontal;
                else
                    nShip.Orientation = ShipOrientation.Vertical;


                foreach (var pp in nShip.GetOwnNeededSpace().AllPoints())
                {
                    nShip.DamagePoint(pp);
                }
                nShip.Status = ShipStatus.Ranen;

                return ForceAddShip(nShip);
            }
        }
        public bool ForceKillShip(Point point)
        {
            var ship = GetShipAtPoint(point);
            if (ship == null) return false;

            ShipConfig currentConf = null;
            foreach(var conf in GameConfig.shipConfigs)
            {
                if (conf.Length == ship.Length)
                {
                    currentConf = conf;
                    break;
                }
            }

            if (currentConf == null)
                return false;

            Ship nShip = new Ship(currentConf);
            nShip.Orientation = ship.Orientation;
            nShip.Position = ship.Position;
            nShip.Status = ShipStatus.Dead;

            foreach (var pp in nShip.GetOwnNeededSpace().AllPoints())
            {
                nShip.DamagePoint(pp);
            }

            ForceDeleteShip(ship);
            ForceAddShip(nShip);

            return true;
        }
        public bool ForceAddShip(Ship ship)
        {
            ship.Parent = this;
            return Objects.Add(ship);
        }
        public bool ForceDeleteShip(Ship ship)
        {
            ship.Parent = null;
            return Objects.Remove(ship);
        }

        public void Draw()
        {
            //Console.Clear();
            Console.SetCursorPosition(0, 0);
            Print();
        }
        public void Print()
        {
            //for (int i = 0; i < matrix.Length; i++)
            //{
            //    for (int j = 0; j < matrix.Length; j++)
            //    {
            //        char ch = '!';

            //        if (PointIsFree(i, j))
            //            ch = '.';
            //        else if (PointIsShip(i, j))
            //            ch = '*';
            //        else if (PointIsAttackShip(i, j))
            //        {
            //            //Console.ForegroundColor = ConsoleColor.Red;
            //            Console.BackgroundColor = ConsoleColor.Red;
            //            ch = 'x';
            //        }

            //        Console.Write(ch);
            //        Console.ForegroundColor = ConsoleColor.DarkGreen;
            //        Console.BackgroundColor = ConsoleColor.Black;
            //    }
            //    Console.WriteLine();
            //}
        }
    }

    public static class Util
    {
        public static void Normalize(ref Point from, ref Point to)
        {
            if (from.Column > to.Column)
                Swap(ref from, ref to);

            if (from.Row > to.Row)
                Swap(ref from.Row, ref to.Row);
        }
        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
    }
}
