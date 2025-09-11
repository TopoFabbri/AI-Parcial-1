using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Tools.Pathfinder.Coordinate;

namespace Model.Tools.Pathfinder.Node
{
    public interface INodeContainer<TCoordinate> where TCoordinate : ICoordinate
    {
        public void AddNodeContainable(INodeContainable<TCoordinate> nodeContainable);
        public void RemoveNodeContainable(INodeContainable<TCoordinate> nodeContainable);
        public ConcurrentBag<INodeContainable<TCoordinate>> GetNodeContainables();
        public void Update(ParallelOptions parallelOptions);
    }
}