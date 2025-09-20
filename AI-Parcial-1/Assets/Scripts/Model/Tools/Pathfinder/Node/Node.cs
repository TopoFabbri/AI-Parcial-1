using System;
using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;

namespace Model.Tools.Pathfinder.Node
{
    public class Node<TCoordinate> : INode, INode<TCoordinate>, INodeContainer<TCoordinate> where TCoordinate : ICoordinate
    {
        private TCoordinate coordinate;
        protected int cost;
        private INode.NodeType type;
        
        private readonly List<INodeContainable<TCoordinate>> nodeContainables = new();
        
        public void SetCost(int cost)
        {
            this.cost = cost;
        }

        public void SetType(INode.NodeType type)
        {
            this.type = type;
        }

        public bool IsBlocked(List<INode.NodeType> blockedTypes)
        {
            return blockedTypes.Contains(type);
        }

        public int GetCost()
        {
            return cost;
        }

        public INode.NodeType GetNodeType()
        {
            return type;
        }

        public void SetCoordinate(TCoordinate coordinate)
        {
            this.coordinate = coordinate;
        }

        public TCoordinate GetCoordinate()
        {
            return coordinate;
        }

        public void AddNodeContainable(INodeContainable<TCoordinate> nodeContainable)
        {
            nodeContainables.Add(nodeContainable);
            nodeContainable.NodeCoordinate = coordinate;
        }
        
        public List<INodeContainable<TCoordinate>> GetNodeContainables()
        {
            return nodeContainables;
        }

        public void RemoveNodeContainable(INodeContainable<TCoordinate> nodeContainable)
        {
            nodeContainables.Remove(nodeContainable);
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