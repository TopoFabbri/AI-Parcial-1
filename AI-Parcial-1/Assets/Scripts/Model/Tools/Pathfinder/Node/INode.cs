using System;
using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;

namespace Model.Tools.Pathfinder.Node
{
    public interface INode
    {
        public enum NodeType
        {
            Grass,
            Road,
            Water
        }

        public bool IsBlocked(List<NodeType> blockedTypes);
        public int GetCost();
        public void SetCost(int cost);
        public NodeType GetNodeType();
        public void SetType(INode.NodeType type);
    }

    public interface INode<TCoordinate> : IEquatable<INode<TCoordinate>> where TCoordinate : ICoordinate
    {
        public void SetCoordinate(TCoordinate coordinateType);
        public TCoordinate GetCoordinate();
    }
}