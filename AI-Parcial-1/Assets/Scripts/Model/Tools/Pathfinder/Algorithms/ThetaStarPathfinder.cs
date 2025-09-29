using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Pathfinder.Algorithms
{
    public class ThetaStarPathfinder<TNodeType, TCoordinate> : AStarPathfinder<TNodeType, TCoordinate> where TCoordinate : ICoordinate where TNodeType : INode<TCoordinate>, INode
    {
        public override List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode, IGraph<TNodeType, TCoordinate> graph, List<INode.NodeType> blockedTypes)
        {
            List<TNodeType> astarPath = base.FindPath(startNode, destinationNode, graph, blockedTypes);

            if (astarPath == null || astarPath.Count < 3) return astarPath;

            List<TNodeType> thetaPath = new();

            int currentIndex = 0;
            int targetIndex = astarPath.Count - 1;

            do
            {
                if (targetIndex > currentIndex + 1)
                {
                    ICollection<TNodeType> bresenhamPath = graph.GetBresenhamNodes(astarPath[currentIndex].GetCoordinate(), astarPath[targetIndex].GetCoordinate());

                    INode.NodeType type = astarPath[currentIndex].GetNodeType();
                    bool sameType = true;

                    foreach (TNodeType node in bresenhamPath)
                    {
                        if (node.GetNodeType() == type) continue;

                        sameType = false;
                        break;
                    }

                    if (!sameType)
                    {
                        targetIndex--;
                        continue;
                    }
                }

                thetaPath.Add(astarPath[currentIndex]);
                
                currentIndex = targetIndex;
                targetIndex = astarPath.Count - 1;
                
            } while (currentIndex < astarPath.Count - 1);

            thetaPath.Add(destinationNode);

            return thetaPath;
        }
    }
}