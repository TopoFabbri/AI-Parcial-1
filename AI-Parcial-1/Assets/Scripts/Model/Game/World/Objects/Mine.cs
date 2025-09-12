using System.Collections.Generic;
using System.Numerics;
using Model.Game.Graph;
using Model.Game.World.Mining;
using Model.Tools.Drawing;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World.Objects
{
    public class Mine : ILocalizable, INodeContainable<Coordinate>
    {
        public static List<Mine> Mines { get; } = new();
        private const float HeightDrawOffset = 1f;
        
        private readonly Graph<Node<Coordinate>, Coordinate> graph;
        
        public GoldContainer GoldContainer { get; private set; }

        string ILocalizable.Name { get; set; } = "Mine";
        int ILocalizable.Id { get; set; }
        Coordinate INodeContainable<Coordinate>.NodeCoordinate { get; set; }

        public Mine(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph, float goldQty)
        {
            this.graph = graph;
            
            Mines.Add(this);
            
            GoldContainer = new GoldContainer(goldQty);
            
            node.AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);
        }
        
        public Vector3 GetPosition()
        {
            float x = ((INodeContainable<Coordinate>)this).NodeCoordinate.X * graph.GetNodeDistance();
            float y = ((INodeContainable<Coordinate>)this).NodeCoordinate.Y * graph.GetNodeDistance();

            return new Vector3(x, HeightDrawOffset, y);
        }

        public void Update()
        {
        }
    }
}