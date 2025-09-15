using Model.Game.Graph;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    // Perpendicular-bisector based decision for 2D integer coordinates using Euclidean geometry.
    // For sites A, B and point P, compute the signed value of (P - M) · (B - A), where M is the midpoint of A and B.
    // s < 0 => P is on A's side of the perpendicular bisector (A owns)
    // s > 0 => P is on B's side (B owns)
    // s = 0 => equidistant
    public sealed class Euclidean2DVoronoiPolicy : IVoronoiPolicy<Node<Coordinate>, Coordinate>
    {
        public int CompareOwnership(Coordinate point, Coordinate siteA, Coordinate siteB, IGraph<Node<Coordinate>, Coordinate> graph)
        {
            double dx = siteB.X - siteA.X;
            double dy = siteB.Y - siteA.Y;
            double mx = (siteA.X + siteB.X) * 0.5;
            double my = (siteA.Y + siteB.Y) * 0.5;

            double s = (point.X - mx) * dx + (point.Y - my) * dy;

            if (s < 0) return -1;
            if (s > 0) return 1;
            return 0;
        }
    }
}
