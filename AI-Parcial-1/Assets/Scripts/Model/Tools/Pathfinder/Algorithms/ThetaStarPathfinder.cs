using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Pathfinder.Algorithms
{
    public class ThetaStarPathfinder<TNodeType, TCoordinate> : AStarPathfinder<TNodeType, TCoordinate> where TCoordinate : ICoordinate where TNodeType : INode<TCoordinate>, INode
    {
        public override List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode, IGraph<TNodeType, TCoordinate> graph, List<INode.NodeType> blockedTypes, Dictionary<INode.NodeType, int> nodeCosts)
        {
            List<TNodeType> astarPath = base.FindPath(startNode, destinationNode, graph, blockedTypes, nodeCosts);

            if (astarPath == null || astarPath.Count < 3) return astarPath;

            List<TNodeType> thetaPath = new();

            int currentIndex = 0;
            int targetIndex = astarPath.Count - 1;

            do
            {
                if (!IsPathSameType(astarPath, currentIndex, targetIndex))
                {
                    targetIndex--;

                    if (targetIndex <= currentIndex)
                    {
                        thetaPath.Add(astarPath[currentIndex]);
                        currentIndex++;
                        targetIndex = astarPath.Count - 1;
                    }
                    
                    continue;
                }
                
                if (IsBresenhamPathBlocked(graph, blockedTypes, astarPath, currentIndex, targetIndex))
                {
                    targetIndex--;
                    continue;
                }

                thetaPath.Add(astarPath[currentIndex]);
                
                currentIndex = targetIndex;
                targetIndex = astarPath.Count - 1;
                
            } while (currentIndex < astarPath.Count - 1);

            thetaPath.Add(destinationNode);

            return thetaPath;
        }

        private static bool IsPathSameType(List<TNodeType> astarPath, int currentIndex, int targetIndex)
        {
            INode.NodeType nodeType = astarPath[currentIndex].GetNodeType();

            bool sameType = true;

            for (int i = currentIndex + 1; i <= targetIndex; i++)
            {
                if (astarPath[i].GetNodeType() == nodeType) continue;
                    
                sameType = false;
                break;
            }

            return sameType;
        }

        private static bool IsBresenhamPathBlocked(IGraph<TNodeType, TCoordinate> graph, List<INode.NodeType> blockedTypes, List<TNodeType> astarPath, int currentIndex, int targetIndex)
        {
            ICollection<TNodeType> bresenhamPath = graph.GetBresenhamNodes(astarPath[currentIndex].GetCoordinate(), astarPath[targetIndex].GetCoordinate());

            bool blocked = false;

            foreach (TNodeType node in bresenhamPath)
            {
                if (!node.IsBlocked(blockedTypes)) continue;

                blocked = true;
                break;
            }

            return blocked;
        }
    }
}