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

    public class Ship
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

        public bool MoveTo(Point position)
        {
            Point from = position;
            Point to = position;
            int n = Parent.N;

            if (Orientation == ShipOrientation.Horizontal)
            {
                to.column += Length - 1;
            }
            else if (Orientation == ShipOrientation.Vertical)
            {
                to.row += Length - 1;
            }

            if (from.column > 0)
                from.column--;
            if (from.row > 0)
                from.row--;

            if (from.column < n - 1)
                from.column++;
            if (from.row < n - 1)
                from.row++;


            bool isFree = Parent.AreaIsFree(from, to);

            if (isFree)
            {
                this.Position = position;
            }

            return isFree;
        }

    }

    public struct Point
    {
        public int column { get; set; }
        public int row { get; set; }

        public Point(int row, int column)
        {
            this.column = column;
            this.row = row;
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.row == p2.row && p1.column == p2.column;
        }
        public static bool operator !=(Point p1, Point p2)
        {
            return !(p1 == p2);
        }
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

        private int[][] matrix;
        public int N { get; }

        public List<Ship> Ships { get; } = new List<Ship>();

        //n - size of battleground
        public Battleground(int n)
        {
            if (n < MIN_N || n > MAX_N)
            {
                throw new ArgumentOutOfRangeException($"N can be in range {MIN_N} - {MAX_N}");
            }
            this.N = n;

            matrix = new int[n][];
            for (int i = 0; i < n; i++)
                matrix[i] = new int[n];
        }

        public void AddShip(Ship ship)
        {
            Ships.Add(ship);
            ship.Parent = this;
            ship.Status = ShipStatius.NotInitialized;
        }

        public bool SetShip(Ship ship, Point point)
        {
            Point from = ship.Position;
            Point to = ship.Position;

            if (ship.Orientation == ShipOrientation.Horizontal)
            {
                to.column += ship.Length - 1;
            }
            else if (ship.Orientation == ShipOrientation.Vertical)
            {
                to.row += ship.Length - 1;
            }

            if (from.column > 0)
                from.column--;
            if (from.row > 0)
                from.row--;

            if (from.column < matrix.Length - 1)
                from.column++;
            if (from.row < matrix.Length - 1)
                from.row++;


            bool isFree = AreaIsFree(from, to);
            if (isFree)
            {
                ship.Status = ShipStatius.Full;
            }


            return isFree;
        }

        public bool MoveTo(Ship ship, Point position)
        {
            Point from = position;
            Point to = position;
            int n = N;

            if (ship.Orientation == ShipOrientation.Horizontal)
            {
                to.column += ship.Length - 1;
            }
            else if (ship.Orientation == ShipOrientation.Vertical)
            {
                to.row += ship.Length - 1;
            }

            if (to.column >= N || to.row >= N)
                return false;

            if (ship.Position == new Point(-1, -1))
            {
                Fill(from, to);
                ship.Position = position;
                return true;
            }

            if (from.column > 0)
                from.column--;
            if (from.row > 0)
                from.row--;

            if (to.column < n - 1)
                to.column++;
            if (to.row < n - 1)
                to.row++;


           
            //isFree = true;

                #region clear cur position

                Point startClear = ship.Position;
                Point endClear = ship.Position;
                if (ship.Orientation == ShipOrientation.Horizontal)
                {
                    endClear.column += ship.Length - 1;
                }
                else if (ship.Orientation == ShipOrientation.Vertical)
                {
                    endClear.row += ship.Length - 1;
                }

                Clear(startClear, endClear);
            #endregion
            bool isFree = AreaIsFree(from, to);
            ship.Position = position;

                #region draw new position
                Point startPosition = ship.Position;
                Point endPosition = ship.Position;
                if (ship.Orientation == ShipOrientation.Horizontal)
                {
                    endPosition.column += ship.Length - 1;
                }
                else if (ship.Orientation == ShipOrientation.Vertical)
                {
                    endPosition.row += ship.Length - 1;
                }

                Fill(startPosition, endPosition);
            #endregion
            if (isFree)
            {
                ship.Status = ShipStatius.Full;
            }
            else
                ship.Status = ShipStatius.NotInitialized;
            Draw(); 
            return isFree;
        }


        public void Fill(Point from, Point to)
        {
            for (int row = from.row; row <= to.row; row++)
            {
                for (int column = from.column; column <= to.column; column++)
                {
                    matrix[row][column]++;
                }
            }
        }
        public void Clear(Point from, Point to)
        {
            for (int row = from.row; row <= to.row; row++)
            {
                for (int column = from.column; column <= to.column; column++)
                {
                    matrix[row][column]--;
                    if (matrix[row][column] < 0)
                        matrix[row][column] = 0;
                }
            }
        }
        public bool AreaIsFree(Point from, Point to)
        {
            bool isFree = true;
            for (int row = from.row; row <= to.row; row++)
            {
                for (int column = from.column; column <= to.column; column++)
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
            return PointIsFree(point.row, point.column);
        }
        public bool PointIsFree(int row, int column)
        {
            return matrix[row][column] == 0;
        }

        public bool PointIsShip(Point point)
        {
            return PointIsShip(point.row, point.column);
        }
        public bool PointIsShip(int row, int column)
        {
            return matrix[row][column] == 1;
        }

        public bool PointIsAttackShip(Point point)
        {
            return PointIsAttackShip(point.row, point.column);
        }
        public bool PointIsAttackShip(int row, int column)
        {
            return matrix[row][column] == 2;
        }


        public void Draw()
        {
            //Console.Clear();
            Console.SetCursorPosition(0, 0);
            Print();
        }
        public void Print()
        {
            for(int i = 0; i < matrix.Length; i++)
            {
                for(int j = 0; j < matrix.Length; j++)
                {
                    char ch = '!';

                    if (PointIsFree(i, j))
                        ch = '.';
                    else if (PointIsShip(i, j))
                        ch = '*';
                    else if (PointIsAttackShip(i, j))
                        ch = 'x';

                    Console.Write(ch);
                }
                Console.WriteLine();
            }
        }
    }

    [Serializable]
    public class Turn
    {
        public int x { get; set; }
    }

}