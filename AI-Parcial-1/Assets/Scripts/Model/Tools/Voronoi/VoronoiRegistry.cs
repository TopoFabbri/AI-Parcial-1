using System;
using System.Collections.Generic;
using System.Globalization;
using Model.Game.Events;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Time;

namespace Model.Tools.Voronoi
{
    public static class VoronoiRegistry<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode, new() where TCoordinate : ICoordinate
    {
        private static readonly Dictionary<Type, IVoronoi<TNode, TCoordinate>> Registries = new();

        public static void GenerateVoronoi(Type type, List<IVoronoiObject<TCoordinate>> voronoiObjects, IVoronoi<TNode, TCoordinate> voronoi)
        {
            Registries[type] = voronoi;
            
            Timer timer = new();

            Registries[type].Generate(voronoiObjects);
            
            EventSystem.EventSystem.Raise<DebugEvent>(timer.TimeElapsed.ToString(CultureInfo.InvariantCulture));
        }

        public static TCoordinate GetClosestTo(Type type, TCoordinate coordinate)
        {
            if (!Registries.TryGetValue(type, out IVoronoi<TNode, TCoordinate> registry))
                throw new ArgumentException("There is no voronoi for type " + type.Namespace);
            
            return registry.GetClosestTo(coordinate);
        }
    }
}