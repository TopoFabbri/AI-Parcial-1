using System;
using System.Threading.Tasks;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.Pathfinder.Node;

namespace Model.Game
{
    public class Model
    {
        private readonly ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 32 };
        
        public Graph<Node<Coordinate>, Coordinate> Graph { get; private set; }

        private Center center;

        public const int MineQty = 5;
        public const int MinMineGold = 100;
        public const int MaxMineGold = 1000;
        
        public Graph<Node<Coordinate>, Coordinate> CreateGraph(int width, int height, float nodeDistance = 1f, bool circumnavigable = false)
        {
            Graph = new Graph<Node<Coordinate>, Coordinate>(width, height, nodeDistance, circumnavigable);
            
            center = new Center(Graph.GetNodeAtIndexes(width / 2, height / 2), Graph);

            Random random = new();
            
            for (int i = 0; i < MineQty; i++)
            {
                Coordinate coordinate = new();
                int maxIterations = 100;
                
                do
                {
                    coordinate.Set(random.Next(0, Graph.GetSize().X), random.Next(0, Graph.GetSize().Y));
                } while (Graph.Nodes[coordinate].GetNodeContainables().Count != 0 && --maxIterations > 0);
                
                Mine mine = new(Graph.Nodes[coordinate], Graph, random.Next(MinMineGold, MaxMineGold));
                Graph.Nodes[coordinate].AddNodeContainable(mine);
            }
            
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