using System;
using Model.Tools.Pathfinder.Coordinate;

namespace Model.Tools.Pathfinder.Node
{
    public class Node<TCoordinate> : INode, INode<TCoordinate> where TCoordinate : ICoordinate
    {
        private bool blocked;
        private TCoordinate coordinate;
        protected int cost;

        public void SetCost(int cost)
        {
            this.cost = cost;
        }

        public bool IsBlocked()
        {
            return blocked;
        }

        public int GetCost()
        {
            return cost;
        }

        public void SetBlocked(bool blocked)
        {
            this.blocked = blocked;
        }

        public void SetCoordinate(TCoordinate coordinate)
        {
            this.coordinate = coordinate;
        }

        public TCoordinate GetCoordinate()
        {
            return coordinate;
        }

        public bool Equals(INode<TCoordinate> other)
        {
            return other != null && coordinate.Equals(other.GetCoordinate());
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Node<TCoordinate>)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(coordinate.GetHashCode());
        }
    }
}