using System;
using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    public static class VoronoiRegistry<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        private static readonly Dictionary<Type, Voronoi<TNode, TCoordinate>> Registries = new();

        public static void GenerateVoronoi(Type type, IGraph<TNode, TCoordinate> graph, List<IVoronoiObject<TCoordinate>> voronoiObjects)
        {
            if (!Registries.ContainsKey(type))
                Registries.Add(type, new Voronoi<TNode, TCoordinate>());

            Registries[type].Generate(graph, voronoiObjects);
        }

        public static TCoordinate GetClosestTo(Type type, TCoordinate coordinate)
        {
            return Registries[type].GetClosestTo(coordinate);
        }
    }
}