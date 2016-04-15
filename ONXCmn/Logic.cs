﻿using System;
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

    public enum ShipStatius
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

        public int Length { get; } = MIN_LENGTH;
        public Point Position { get; set; }
        public ShipOrientation Orientation { get; set; } = ShipOrientation.Horizontal;
        public ShipStatius Status { get; set; } = ShipStatius.NotInitialized;

        public Battleground Parent { get; set; }

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

        public bool IntersectBy(Point point)
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

            return (rectangle.From.Row <= To.Row && rectangle.To.Row >= From.Row &&
                    rectangle.From.Column <= To.Column && rectangle.To.Column >= From.Column);
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

        Rectangle ground;
        //private int[][] matrix;
        public int N { get; }

        public GameProcessStatus GameStatus { get; private set; } = GameProcessStatus.InitializingGround;
        public HashSet<IBattleble> Objects { get; } = new HashSet<IBattleble>();

        //n - size of battleground
        public Battleground(int n)
        {
            if (n < MIN_N || n > MAX_N)
            {
                throw new ArgumentOutOfRangeException($"N can be in range {MIN_N} - {MAX_N}");
            }
            this.N = n;
            ground = new Rectangle(new Point(0, 0), new Point(n - 1, n - 1));
            //matrix = new int[n][];
            //for (int i = 0; i < n; i++)
            //    matrix[i] = new int[n];
        }

        public void Battle()
        {
            this.GameStatus = GameProcessStatus.Battle;
        }

        private bool AddShip(Ship ship)
        {
            if (GameStatus == GameProcessStatus.Battle)
                return false;

            //check
            if (!CanPlace(ship))
                return false;

            ship.Parent = this;
            ship.Status = ShipStatius.Full;
            return Objects.Add(ship);
        }
        private bool AddBarrier(Barrier barrier)
        {
            if (GameStatus == GameProcessStatus.Battle)
                return false;

            barrier.Parent = this;
            return Objects.Add(barrier);
        }

        public bool DeleteShip(Ship ship)
        {
            if (GameStatus == GameProcessStatus.Battle)
                return false;

            return Objects.Remove(ship);
        }
        public bool DeleteBarrier(Barrier barrier)
        {
            if (GameStatus == GameProcessStatus.Battle)
                return false;

            return Objects.Remove(barrier);
        }

        public bool CanPlace(Ship ship)
        {
            return CanPlace(ship, ship.Position);
        }
        public bool CanPlace(Ship ship, Point position)
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

        public bool AreaIsFree(Point from, Point to)
        {
            Util.Normalize(ref from, ref to);

            if (to.Row >= N || to.Column >= N)
                return false;

            bool isFree = true;

            for (int row = from.Row; row <= to.Row; row++)
            {
                for (int column = from.Column; column <= to.Column; column++)
                {
                    if (!PointIsFree(row, column))
                    {
                        isFree = false;
                        break;
                    }
                }
            }

            return isFree;
        }
        public bool AreaIsFree(Rectangle rectangle, bool strictMode)
        {
            rectangle.Normalize();

            if (strictMode)
            {
                if (!ground.Contains(rectangle))
                    return false;
            }


            bool isFree = true;

            int toRow = Math.Min(rectangle.To.Row, N - 1);
            int toColumn = Math.Min(rectangle.To.Column, N - 1);

            for (int row = rectangle.From.Row; row <= toRow; row++)
            {
                for (int column = rectangle.From.Column; column <= toColumn; column++)
                {
                    if (!PointIsFree(row, column))
                    {
                        isFree = false;
                        break;
                    }
                }
            }

            return isFree;
        }


        public bool PointIsFree(Point point)
        {
            return !Objects.Any(o => o.GetOwnNeededSpace().IntersectBy(point));
        }
        public bool PointIsFree(int row, int column)
        {
            return PointIsFree(new Point(row, column));
        }

        public bool PointIsShip(Point point)
        {
            return Objects.Where(o => o is Ship).Any(s => s.GetOwnNeededSpace().IntersectBy(point));
        }
        public bool PointIsShip(int row, int column)
        {
            return PointIsShip(new Point(row, column));
        }

        public Ship GetShipAtPoint(Point point)
        {
            return (Ship)Objects.Where(o => o is Ship).FirstOrDefault(s => s.GetOwnNeededSpace().IntersectBy(point));
        }
        public Barrier GetBarrierAtPoint(Point point)
        {
            return (Barrier)Objects.Where(o => o is Barrier).FirstOrDefault(s => s.GetOwnNeededSpace().IntersectBy(point));
        }
        public T GetObjectAtPoint<T>(Point point)
        {
            return (T)Objects.Where(o => o is T).FirstOrDefault(s => s.GetOwnNeededSpace().IntersectBy(point));
        }

        public bool PointIsAttackShip(Point point)
        {
            throw new NotImplementedException();
        }
        public bool PointIsAttackShip(int row, int column)
        {
            return PointIsAttackShip(new Point(row, column));
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

    [Serializable]
    public class Turn
    {
        public int x { get; set; }
    }

}
