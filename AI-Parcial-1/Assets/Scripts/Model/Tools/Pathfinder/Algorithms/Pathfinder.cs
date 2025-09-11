using System;
using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Pathfinder.Algorithms
{
    public abstract class Pathfinder<TNodeType, TCoordinate> where TNodeType : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        public virtual List<TNodeType> FindPath(TNodeType startNode, TNodeType destinationNode, IGraph<TNodeType, TCoordinate> graph)
        {
            Dictionary<TNodeType, (TNodeType Parent, int AcumulativeCost, int Heuristic)> nodes = new();

            foreach (TNodeType node in graph.GetNodes()) nodes.Add(node, (default, 0, 0));

            List<TNodeType> openList = new();
            List<TNodeType> closedList = new();

            openList.Add(startNode);

            while (openList.Count > 0)
            {
                TNodeType currentNode = openList[0];
                int currentIndex = 0;

                for (int i = 1; i < openList.Count; i++)
                    if (nodes[openList[i]].AcumulativeCost + nodes[openList[i]].Heuristic < nodes[currentNode].AcumulativeCost + nodes[currentNode].Heuristic)
                    {
                        currentNode = openList[i];
                        currentIndex = i;
                    }

                openList.RemoveAt(currentIndex);
                closedList.Add(currentNode);

                if (NodesEquals(currentNode, destinationNode)) return GeneratePath(startNode, destinationNode);

                foreach (TNodeType neighbor in GetAdjacents(currentNode, graph))
                {
                    if (!nodes.ContainsKey(neighbor) || IsBlocked(neighbor) || closedList.Contains(neighbor))
                        continue;

                    int tentativeNewAccumulatedCost = 0;
                    tentativeNewAccumulatedCost += nodes[currentNode].AcumulativeCost;
                    tentativeNewAccumulatedCost += MoveToNeighborCost(currentNode, neighbor);

                    if (!openList.Contains(neighbor) || tentativeNewAccumulatedCost < nodes[currentNode].AcumulativeCost)
                    {
                        nodes[neighbor] = (currentNode, tentativeNewAccumulatedCost, Distance(neighbor, destinationNode));

                        if (!openList.Contains(neighbor)) openList.Add(neighbor);
                    }
                }
            }

            return null;

            List<TNodeType> GeneratePath(TNodeType startNode, TNodeType goalNode)
            {
                List<TNodeType> path = new();
                TNodeType currentNode = goalNode;

                while (!NodesEquals(currentNode, startNode))
                {
                    path.Add(currentNode);
                    currentNode = nodes[currentNode].Parent;
                }

                path.Reverse();
                return path;
            }
        }

        protected virtual int Distance(TNodeType a, TNodeType b)
        {
            return (int)MathF.Round(a.GetCoordinate().GetDistanceTo(b.GetCoordinate()));
        }

        protected virtual bool NodesEquals(TNodeType a, TNodeType b)
        {
            return a.Equals(b);
        }

        protected abstract int MoveToNeighborCost(TNodeType a, TNodeType b);

        protected virtual bool IsBlocked(TNodeType node)
        {
            return node.IsBlocked();
        }

        protected abstract ICollection<TNodeType> GetAdjacents(TNodeType node, IGraph<TNodeType, TCoordinate> graph);
    }
}