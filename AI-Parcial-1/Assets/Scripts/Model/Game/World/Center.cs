using System.Collections.Generic;
using System.Numerics;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Game.World.Agents;
using Model.Tools.Drawing;
using Model.Tools.EventSystem;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World
{
    public class Center : ILocalizable, INodeContainable<Coordinate>
    {
        private const float HeightDrawOffset = 1f;

        string ILocalizable.Name { get; set; } = "Center";

        int ILocalizable.Id { get; set; }

        private readonly Graph<Node<Coordinate>, Coordinate> graph;
        private readonly List<Miner> miners = new();
        Coordinate INodeContainable<Coordinate>.NodeCoordinate { get; set; }

        public Center(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph)
        {
            this.graph = graph;
            
            node.AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);
            
            EventSystem.Subscribe<RequestedMinerCreationEvent>(CreateMiner);
        }

        ~Center()
        {
            miners.Clear();
            EventSystem.Unsubscribe<RequestedMinerCreationEvent>(CreateMiner);
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

        private void CreateMiner(RequestedMinerCreationEvent minerCreationRequest)
        {
            miners.Add(new Miner(graph.GetNodeAt(((INodeContainable<Coordinate>)this).NodeCoordinate), graph));
        }
    }
}