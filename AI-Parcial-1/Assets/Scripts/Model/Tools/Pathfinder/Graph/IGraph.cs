using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Pathfinder.Graph
{
    public interface IGraph<TNode, TCoordinate> : IBresenhamable<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        public ICollection<TNode> GetNodes();
        public TCoordinate GetSize();
        public ICollection<TNode> GetAdjacents(TNode node);
        public ICollection<TCoordinate> GetAdjacents(TCoordinate coordinate);
        public float GetNodeDistance();
        public float GetDistanceBetweenNodes(TNode a, TNode b);
        public float GetDistanceBetweenCoordinates(TCoordinate a, TCoordinate b);
        public bool IsCircumnavigable();
        public TNode GetNodeAt(TCoordinate coordinate);
    }
}