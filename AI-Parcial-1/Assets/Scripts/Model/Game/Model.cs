using System.Threading.Tasks;
using Model.Game.Graph;
using Model.Game.World.Agents;
using Model.Tools.Pathfinder.Node;

namespace Model.Game
{
    public class Model
    {
        private readonly ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 32 };
        
        public Graph<Node<Coordinate>, Coordinate> Graph { get; private set; }

        private Miner testMiner;
        
        public Graph<Node<Coordinate>, Coordinate> CreateGraph(int width, int height, float nodeDistance = 1f, bool circumnavigable = false)
        {
            Graph = new Graph<Node<Coordinate>, Coordinate>(width, height, nodeDistance, circumnavigable);
            
            testMiner = new Miner(Graph.GetNodeAtIndexes(width / 2, height / 2));
            
            return Graph;
        }
        
        public void Update()
        {
            Parallel.ForEach(Graph.Nodes.Values, parallelOptions, node => 
            {
                node.Update(parallelOptions);
            });
        }
    }
}