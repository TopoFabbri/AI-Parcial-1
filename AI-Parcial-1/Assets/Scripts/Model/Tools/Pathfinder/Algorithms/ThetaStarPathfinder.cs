using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Pathfinder.Algorithms
{
    public class ThetaStarPathfinder<TNodeType, TCoordinate> : AStarPathfinder<TNodeType, TCoordinate> 
        where TCoordinate : ICoordinate 
        where TNodeType : INode<TCoordinate>, INode
    {
        public override List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode, IGraph<TNodeType, TCoordinate> graph, List<INode.NodeType> blockedTypes)
        {
            List<TNodeType> astarPath = base.FindPath(startNode, destinationNode, graph, blockedTypes);
            
            if (astarPath == null || astarPath.Count < 3) return astarPath;

            List<TNodeType> newPath = new();

            TNodeType currentNode = startNode;

            int nodeIndex = 1;
            
            do
            {
                ICollection<TNodeType> bresenhamPath = graph.GetBresenhamNodes(currentNode.GetCoordinate(), astarPath[nodeIndex].GetCoordinate());

                bool blocked = false;

                foreach (TNodeType node in bresenhamPath)
                {
                    if (!node.IsBlocked(blockedTypes)) continue;

                    blocked = true;
                    break;
                }

                if (blocked)
                {
                    newPath.Add(astarPath[nodeIndex - 1]);
                    currentNode = astarPath[nodeIndex - 1];
                }
                
                nodeIndex++;
                    
            } while (nodeIndex < astarPath.Count);
            
            newPath.Add(destinationNode);
            
            return newPath;
        }
    }
}