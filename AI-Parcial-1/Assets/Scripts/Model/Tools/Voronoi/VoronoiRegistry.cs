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

        public static void GenerateVoronoi(Type type, IGraph<TNode, TCoordinate> graph, List<IVoronoiObject<TCoordinate>> voronoiObjects, IVoronoiPolicy<TNode, TCoordinate> policy = null)
        {
            policy ??=  new DistanceBasedVoronoiPolicy<TNode, TCoordinate>();
            
            Registries[type] = new Voronoi<TNode, TCoordinate>(policy);
            Registries[type].Generate(graph, voronoiObjects);
        }

        public static TCoordinate GetClosestTo(Type type, TCoordinate coordinate)
        {
            return Registries[type].GetClosestTo(coordinate);
        }
    }
}