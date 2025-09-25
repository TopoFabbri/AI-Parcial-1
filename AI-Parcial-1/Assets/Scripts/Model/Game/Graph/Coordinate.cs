using System;
using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;

namespace Model.Game.Graph
{
    public class Coordinate : ICoordinate
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Coordinate()
        {
            X = 0;
            Y = 0;
        }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Set(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public void Set(params object[] parameters)
        {
            if (parameters.Length != 2) return;
            
            Set((int)parameters[0], (int)parameters[1]);
        }

        public int GetDistanceTo(ICoordinate coordinate)
        {
            if (!typeof(Coordinate).IsAssignableFrom(coordinate.GetType())) return -1;

            return Math.Abs(X - ((Coordinate)coordinate).X) + Math.Abs(Y - ((Coordinate)coordinate).Y);
        }

        public List<ICoordinate> GetAdjacents()
        {
            List<ICoordinate> neighbours = new();

            neighbours.Add(new Coordinate(X + 1, Y));
            neighbours.Add(new Coordinate(X, Y + 1));
            neighbours.Add(new Coordinate(X - 1, Y));
            neighbours.Add(new Coordinate(X, Y - 1));

            return neighbours;
        }

        public bool Equals(ICoordinate other)
        {
            return Equals(other as object);
        }

        public override bool Equals(object obj)
        {
            if (obj is not Coordinate coordinate)
                return false;

            return X == coordinate.X && Y == coordinate.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}