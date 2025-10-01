using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    public sealed class EuclideanDistancePolicy<TNode, TCoordinate> : IVoronoiPolicy<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        public int CompareOwnership(TCoordinate point, TCoordinate siteA, TCoordinate siteB, IGraph<TNode, TCoordinate> graph)
        {
            float da = graph.GetEuclideanDistanceBetweenCoordinates(siteA, point);
            float db = graph.GetEuclideanDistanceBetweenCoordinates(siteB, point);

            if (da < db) return -1;
            if (da > db) return 1;
            return 0;
        }
    }
}