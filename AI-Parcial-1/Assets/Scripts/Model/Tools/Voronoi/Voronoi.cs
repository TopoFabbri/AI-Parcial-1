using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    internal class Voronoi<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        private readonly Dictionary<TCoordinate, TCoordinate> voronoiMap = new();

        internal void Generate(IGraph<TNode, TCoordinate> graph, List<IVoronoiObject<TCoordinate>> voronoiObjects)
        {
            voronoiMap.Clear();
            
            List<TCoordinate> exploredCoords = new();
            Dictionary<TCoordinate, TCoordinate> toExploreCoords = new();

            foreach (IVoronoiObject<TCoordinate> voronoiObject in voronoiObjects)
            {
                TCoordinate coordinates = voronoiObject.GetCoordinates();
                
                if (exploredCoords.Contains(coordinates)) continue;
                
                voronoiMap[coordinates] = coordinates;
                exploredCoords.Add(coordinates);
            }

            foreach (TCoordinate exploredCoord in exploredCoords)
            {
                foreach (TCoordinate adjacent in graph.GetAdjacents(exploredCoord))
                {
                    if (exploredCoords.Contains(adjacent)) continue;
                    if (toExploreCoords.ContainsKey(adjacent)) continue;
                    
                    toExploreCoords.Add(adjacent, exploredCoord);
                }
            }

            Dictionary<TCoordinate, TCoordinate> toExploreTmp = new();

            do
            {
                foreach (KeyValuePair<TCoordinate, TCoordinate> toExploreCoord in toExploreCoords)
                {
                    voronoiMap.TryAdd(toExploreCoord.Key, voronoiMap[toExploreCoord.Value]);
                    exploredCoords.Add(toExploreCoord.Key);
                
                    toExploreTmp.Add(toExploreCoord.Key, toExploreCoord.Value);
                }
            
                toExploreCoords.Clear();

                foreach (TCoordinate toExploreCoord in toExploreTmp.Keys)
                {
                    foreach (TCoordinate adjacent in graph.GetAdjacents(toExploreCoord))
                    {
                        if (exploredCoords.Contains(adjacent)) continue;
                        if (toExploreCoords.ContainsKey(adjacent)) continue;
                    
                        toExploreCoords.Add(adjacent, toExploreCoord);
                    }
                }
            } while (toExploreCoords.Count > 0);
        }

        internal TCoordinate GetClosestTo(TCoordinate coordinate)
        {
            return voronoiMap[coordinate];
        }
    }
}