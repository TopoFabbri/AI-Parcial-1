using System;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.Drawing;
using Model.Tools.EventSystem;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Time;
using Model.Tools.Voronoi;

namespace Model.Game
{
    public class Model
    {
        public Graph<Node<Coordinate>, Coordinate> Graph { get; private set; }
        
        private Center center;
        
        public static bool AlarmRaised { get; private set; }
        
        public Model()
        { 
            EventSystem.Subscribe<RaiseAlarmEvent>(OnRaiseAlarm);
        }
        
        ~Model()
        {
            EventSystem.Unsubscribe<RaiseAlarmEvent>(OnRaiseAlarm);
            
            center = null;
            
            Mine.Mines.Clear();
            Localizables.Clear();
            
            Graph = null;
        }
        
        public Graph<Node<Coordinate>, Coordinate> CreateGraph(int width, int height, int mineQty, float minMineGold, float maxMineGold, int maxFoodQty, float nodeDistance = 1f, bool circumnavigable = false)
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

                Mine mine = new(Graph.Nodes[coordinate], Graph, random.Next((int)minMineGold, (int)maxMineGold), maxFoodQty);
            }
            
            VoronoiRegistry<Node<Coordinate>, Coordinate>.GenerateVoronoi(typeof(Mine), Graph, Mine.Mines);
            EventSystem.Raise<GraphModifiedEvent>();
            
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
            return center.GoldContainer.ContainingQty;
        }
        
        private static void OnRaiseAlarm(RaiseAlarmEvent raiseAlarmEvent)
        {
            AlarmRaised = !AlarmRaised;
        }
    }
}