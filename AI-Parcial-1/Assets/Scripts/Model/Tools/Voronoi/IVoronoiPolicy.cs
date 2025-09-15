using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    // Defines how to decide which site owns a point based on the perpendicular-bisector rule (or an equivalent metric).
    // Return value convention:
    //   < 0  => siteA owns the point
    //   > 0  => siteB owns the point
    //   = 0  => tie (equidistant)
    public interface IVoronoiPolicy<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode where TCoordinate : ICoordinate
    {
        int CompareOwnership(TCoordinate point, TCoordinate siteA, TCoordinate siteB, IGraph<TNode, TCoordinate> graph);
    }
}
