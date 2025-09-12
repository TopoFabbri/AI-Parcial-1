using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Pathfinder.Graph
{
    public interface IBresenhamable<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        public ICollection<TNode> GetBresenhamNodes(TCoordinate start, TCoordinate end);
    }
}