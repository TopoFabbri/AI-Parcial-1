using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    internal class Voronoi<TNode, TCoordinate> : IVoronoi<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        private readonly ConcurrentDictionary<TCoordinate, TCoordinate> voronoiMap = new();
        private readonly IVoronoiPolicy<TNode, TCoordinate> policy;
        private readonly IGraph<TNode, TCoordinate> graph;
        
        public ParallelOptions ParallelOptions { get; } = new() { MaxDegreeOfParallelism = 32 };

        internal Voronoi(IVoronoiPolicy<TNode, TCoordinate> policy, IGraph<TNode, TCoordinate> graph)
        {
            this.policy = policy;
            this.graph = graph;
        }

        public void Generate(List<IVoronoiObject<TCoordinate>> voronoiObjects)
        {
            voronoiMap.Clear();

            ConcurrentBag<TCoordinate> sites = new();
            Parallel.ForEach(voronoiObjects, ParallelOptions, voronoiObject =>
            {
                TCoordinate site = voronoiObject.GetCoordinates();

                if (!sites.Contains(site))
                    sites.Add(site);

                voronoiMap[site] = site;
            });

            if (sites.Count == 0)
                return;

            Parallel.ForEach(graph.GetNodes(), ParallelOptions, node =>
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

        public TCoordinate GetClosestTo(TCoordinate coordinate)
        {
            return voronoiMap.GetValueOrDefault(coordinate, coordinate);
        }
    }
    
    public interface IVoronoi<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        ParallelOptions ParallelOptions { get; }
        
        void Generate(List<IVoronoiObject<TCoordinate>> voronoiObjects);
        TCoordinate GetClosestTo(TCoordinate coordinate);
    }
}