using Model.Tools.Pathfinder.Coordinate;

namespace Model.Tools.Pathfinder.Node
{
    public interface INodeContainable<TCoordinate> where TCoordinate : ICoordinate
    {
        public TCoordinate NodeCoordinate { get; internal set; }
        public void Update();
    }
}