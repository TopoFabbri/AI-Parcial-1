using System;
using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;
using Model.Game.Graph;

namespace Model.Tools.Voronoi
{
    public static class VoronoiRegistry<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode, new() where TCoordinate : ICoordinate
    {
        private static readonly Dictionary<Type, Voronoi<TNode, TCoordinate>> Registries = new();

        private static IVoronoiPolicy<TNode, TCoordinate> CreateDefaultPolicy(IGraph<TNode, TCoordinate> graph)
        {
            return new DistanceBasedVoronoiPolicy<TNode, TCoordinate>();
            if (typeof(TCoordinate) != typeof(Coordinate)) return new DistanceBasedVoronoiPolicy<TNode, TCoordinate>();
            if (!graph.IsCircumnavigable()) return (IVoronoiPolicy<TNode, TCoordinate>)(object)new Euclidean2DVoronoiPolicy();
            
            Coordinate size = (Coordinate)(object)graph.GetSize();
            return (IVoronoiPolicy<TNode, TCoordinate>)(object)new Euclidean2DToroidalVoronoiPolicy(size.X, size.Y);
        }

        public static void GenerateVoronoi(Type type, IGraph<TNode, TCoordinate> graph, List<IVoronoiObject<TCoordinate>> voronoiObjects, IVoronoiPolicy<TNode, TCoordinate> policy = null)
        {
            policy ??= CreateDefaultPolicy(graph);
            
            Registries[type] = new Voronoi<TNode, TCoordinate>(policy);
            Registries[type].Generate(graph, voronoiObjects);
        }

        public static TCoordinate GetClosestTo(Type type, TCoordinate coordinate)
        {
            return Registries[type].GetClosestTo(coordinate);
        }
    }
}