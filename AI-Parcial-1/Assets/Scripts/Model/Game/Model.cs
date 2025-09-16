using System;
using System.Threading.Tasks;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.Drawing;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Time;
using Model.Tools.Voronoi;

namespace Model.Game
{
    public class Model
    {
        private readonly ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 32 };
        
        public Graph<Node<Coordinate>, Coordinate> Graph { get; private set; }
        
        private Center center;
        
        ~Model()
        {
            center = null;
            
            Mine.Mines.Clear();
            Localizables.Clear();
            
            Graph = null;
        }
        
        public Graph<Node<Coordinate>, Coordinate> CreateGraph(int width, int height, int mineQty, int minMineGold, int maxMineGold, float nodeDistance = 1f, bool circumnavigable = false)
        {
            Time.Start();
            
            Graph = new Graph<Node<Coordinate>, Coordinate>(width, height, nodeDistance, circumnavigable);
            
            center = new Center(Graph.GetNodeAtIndexes(width / 2, height / 2), Graph);

            Random random = new();
            
            for (int i = 0; i < mineQty; i++)
            {
                Coordinate coordinate = new();
                int maxIterations = 100;
                
                do
                {
                    coordinate.Set(random.Next(0, Graph.GetSize().X), random.Next(0, Graph.GetSize().Y));
                } while (Graph.Nodes[coordinate].GetNodeContainables().Count != 0 && --maxIterations > 0);

                Mine mine = new(Graph.Nodes[coordinate], Graph, random.Next(minMineGold, maxMineGold));
            }
            
            VoronoiRegistry<Node<Coordinate>, Coordinate>.GenerateVoronoi(typeof(Mine), Graph, Mine.Mines);
            
            return Graph;
        }
        
        public void Update()
        {
            Time.Update();

            foreach (string localizableName in Localizables.GetLocalizableNames())
            {
                foreach (ILocalizable localizable in Localizables.GetLocalizablesOfName(localizableName))
                    localizable.Update();
            }
        }

        public float GetCenterGold()
        {
            return center.GoldContainer.ContainingGold;
        }
    }
}