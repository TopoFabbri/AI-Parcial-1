using Model.Game.Graph;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    // Euclidean Voronoi decision on a toroidal (circumnavigable) 2D grid using the minimum-image convention.
    // Distances are computed using the shortest wrapped displacement along each axis.
    public sealed class Euclidean2DToroidalVoronoiPolicy : IVoronoiPolicy<Node<Coordinate>, Coordinate>
    {
        private readonly int width;
        private readonly int height;

        public Euclidean2DToroidalVoronoiPolicy(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public int CompareOwnership(Coordinate point, Coordinate siteA, Coordinate siteB, IGraph<Node<Coordinate>, Coordinate> graph)
        {
            long da = SquaredToroidalDistance(point, siteA);
            long db = SquaredToroidalDistance(point, siteB);

            if (da < db) return -1;
            if (da > db) return 1;
            return 0;
        }

        private long SquaredToroidalDistance(Coordinate p, Coordinate s)
        {
            int dx = MinImage(p.X - s.X, width);
            int dy = MinImage(p.Y - s.Y, height);
            return (long)dx * dx + (long)dy * dy;
        }

        private static int MinImage(int d, int size)
        {
            if (size <= 0) return d;
            int mod = d % size;
            if (mod < 0) mod += size; // now in [0, size)
            if (mod > size / 2) mod -= size; // map to shortest signed displacement
            return mod;
        }
    }
}
