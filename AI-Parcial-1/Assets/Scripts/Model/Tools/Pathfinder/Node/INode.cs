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
        public NodeType GetNodeType();
        public void SetType(NodeType type);
    }

    public interface INode<TCoordinate> : IEquatable<INode<TCoordinate>> where TCoordinate : ICoordinate
    {
        public void SetCoordinate(TCoordinate coordinateType);
        public TCoordinate GetCoordinate();
    }
}