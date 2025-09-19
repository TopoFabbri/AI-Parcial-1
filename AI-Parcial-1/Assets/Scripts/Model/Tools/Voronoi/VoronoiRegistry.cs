using System;
using System.Collections.Generic;
using Model.Game.Graph;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    public static class VoronoiRegistry<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode, new() where TCoordinate : ICoordinate
    {
        private static readonly Dictionary<Type, BisectorVoronoi> Registries = new();

        public static void GenerateVoronoi(Type type, IGraph<Node<Coordinate>, Coordinate> graph, List<IVoronoiObject<Coordinate>> voronoiObjects, IVoronoiPolicy<TNode, TCoordinate> policy = null)
        {
            Registries[type] = new BisectorVoronoi(new DistanceBasedVoronoiPolicy<Node<Coordinate>, Coordinate>());
            Registries[type].Generate(graph, voronoiObjects);
        }

        public static Coordinate GetClosestTo(Type type, Coordinate coordinate)
        {
            if (!Registries.TryGetValue(type, out BisectorVoronoi registry))
                throw new ArgumentException($"There is no voronoi for type " + type.Namespace);
            
            return registry.GetClosestTo(coordinate);
        }
    }
}