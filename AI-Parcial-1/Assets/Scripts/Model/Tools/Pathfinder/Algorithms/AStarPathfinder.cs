using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Pathfinder.Algorithms
{
    public class AStarPathfinder<TNodeType, TCoordinate> : Pathfinder<TNodeType, TCoordinate> where TCoordinate : ICoordinate where TNodeType : INode<TCoordinate>, INode
    {
        protected override int MoveToNeighborCost(TNodeType a, TNodeType b, Dictionary<INode.NodeType, int> nodeCosts)
        {
            return nodeCosts[b.GetNodeType()];
        }

        protected override ICollection<TNodeType> GetAdjacents(TNodeType node, IGraph<TNodeType, TCoordinate> graph)
        {
            return graph.GetAdjacents(node);
        }
    }
}