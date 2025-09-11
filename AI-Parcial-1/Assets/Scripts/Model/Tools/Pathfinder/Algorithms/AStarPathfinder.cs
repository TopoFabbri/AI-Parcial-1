using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Pathfinder.Algorithms
{
    public class AStarPathfinder<TNodeType, TCoordinate> : Pathfinder<TNodeType, TCoordinate> where TCoordinate : ICoordinate where TNodeType : INode<TCoordinate>, INode
    {
        public override List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode, IGraph<TNodeType, TCoordinate> graph)
        {
            List<TNodeType> astarPath = base.FindPath(startNode, destinationNode, graph);

            return astarPath;
            
            if (astarPath == null || astarPath.Count == 0) return astarPath;

            List<TNodeType> tmpPath = new();
            List<TNodeType> newPath = new();

            foreach (TNodeType node in astarPath)
                tmpPath.Add(node);

            TNodeType currentNode = startNode;

            do
            {
                ICollection<TNodeType> bresenhamPath = graph.GetBresenhamNodes(currentNode.GetCoordinate(), tmpPath[^1].GetCoordinate());

                bool blocked = false;

                foreach (TNodeType node in bresenhamPath)
                {
                    if (!node.IsBlocked()) continue;

                    blocked = true;
                    break;
                }

                if (blocked)
                {
                    tmpPath.RemoveAt(tmpPath.Count - 1);
                }
                else
                {
                    newPath.Add(tmpPath[^1]);
                    currentNode = tmpPath[^1];

                    tmpPath.Clear();
                    foreach (TNodeType node in astarPath)
                        tmpPath.Add(node);
                }
            } while (!currentNode.Equals(destinationNode));

            return newPath;
        }

        protected override int MoveToNeighborCost(TNodeType a, TNodeType b)
        {
            return b.GetCost();
        }

        protected override ICollection<TNodeType> GetAdjacents(TNodeType node, IGraph<TNodeType, TCoordinate> graph)
        {
            return graph.GetAdjacents(node);
        }
    }
}