using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    internal class Voronoi<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        private readonly Dictionary<TCoordinate, TCoordinate> voronoiMap = new();
        private readonly IVoronoiPolicy<TNode, TCoordinate> policy;

        internal Voronoi(IVoronoiPolicy<TNode, TCoordinate> policy)
        {
            this.policy = policy;
        }

        internal void Generate(IGraph<TNode, TCoordinate> graph, List<IVoronoiObject<TCoordinate>> voronoiObjects)
        {
            voronoiMap.Clear();

            List<TCoordinate> sites = new();
            
            foreach (IVoronoiObject<TCoordinate> voronoiObject in voronoiObjects)
            {
                TCoordinate site = voronoiObject.GetCoordinates();
                
                if (!sites.Contains(site))
                    sites.Add(site);
                
                voronoiMap[site] = site;
            }

            if (sites.Count == 0)
                return;

            foreach (TNode node in graph.GetNodes())
            {
                TCoordinate point = node.GetCoordinate();

                TCoordinate winner = sites[0];
                
                for (int i = 1; i < sites.Count; i++)
                {
                    TCoordinate challenger = sites[i];
                    int compareResult = policy.CompareOwnership(point, winner, challenger, graph);
                    
                    if (compareResult > 0)
                        winner = challenger;
                }

                voronoiMap[point] = winner;
            }
        }

        internal TCoordinate GetClosestTo(TCoordinate coordinate)
        {
            return voronoiMap[coordinate];
        }
    }
}