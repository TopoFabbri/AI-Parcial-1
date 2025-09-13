using Model.Tools.Pathfinder.Coordinate;

namespace Model.Tools.Voronoi
{
    public interface IVoronoiObject<TCoordinate> where TCoordinate : ICoordinate
    {
        TCoordinate GetCoordinates();
    }
}