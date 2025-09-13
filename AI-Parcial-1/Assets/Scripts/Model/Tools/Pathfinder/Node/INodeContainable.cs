using Model.Tools.Pathfinder.Coordinate;

namespace Model.Tools.Pathfinder.Node
{
    public interface INodeContainable<TCoordinate> where TCoordinate : ICoordinate
    {
        public TCoordinate NodeCoordinate { get; set; }
        public void Update();
    }
}