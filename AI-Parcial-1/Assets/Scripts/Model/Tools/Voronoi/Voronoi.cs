using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    internal class Voronoi<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        private readonly ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 32 };

        private readonly ConcurrentDictionary<TCoordinate, TCoordinate> voronoiMap = new();
        private readonly IVoronoiPolicy<TNode, TCoordinate> policy;

        internal Voronoi(IVoronoiPolicy<TNode, TCoordinate> policy)
        {
            this.policy = policy;
        }

        internal void Generate(IGraph<TNode, TCoordinate> graph, List<IVoronoiObject<TCoordinate>> voronoiObjects)
        {
            voronoiMap.Clear();

            ConcurrentBag<TCoordinate> sites = new();
            Parallel.ForEach(voronoiObjects, parallelOptions, voronoiObject =>
            {
                TCoordinate site = voronoiObject.GetCoordinates();

                if (!sites.Contains(site))
                    sites.Add(site);

                voronoiMap[site] = site;
            });

            if (sites.Count == 0)
                return;

            Parallel.ForEach(graph.GetNodes(), parallelOptions, node =>
            {
                TCoordinate point = node.GetCoordinate();
                TCoordinate winner = sites.ElementAt(0);

                for (int i = 1; i < sites.Count; i++)
                {
                    TCoordinate challenger = sites.ElementAt(i);
                    int compareResult = policy.CompareOwnership(point, winner, challenger, graph);

                    if (compareResult > 0)
                        winner = challenger;
                }

                voronoiMap[point] = winner;
            });
        }

        internal TCoordinate GetClosestTo(TCoordinate coordinate)
        {
            return voronoiMap.GetValueOrDefault(coordinate, coordinate);
        }
    }
}