using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    // Generic fallback policy that uses the coordinate's provided distance function.
    // This still adheres to the Voronoi definition (region of closest site) under the given metric.
    public sealed class ManhattanDistancePolicy<TNode, TCoordinate> : IVoronoiPolicy<TNode, TCoordinate>
        where TNode : INode<TCoordinate>, INode
        where TCoordinate : ICoordinate
    {
        public int CompareOwnership(TCoordinate point, TCoordinate siteA, TCoordinate siteB, IGraph<TNode, TCoordinate> graph)
        {
            float da = graph.GetDistanceBetweenCoordinates(siteA, point);
            float db = graph.GetDistanceBetweenCoordinates(siteB, point);

            if (da < db) return -1;
            if (da > db) return 1;
            return 0;
        }
    }
}