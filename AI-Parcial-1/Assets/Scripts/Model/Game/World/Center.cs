using System.Collections.Generic;
using System.Numerics;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Game.World.Agents;
using Model.Tools.Drawing;
using Model.Tools.EventSystem;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World
{
    public class Center : IDrawable, INodeContainable<Coordinate>
    {
        private const float HeightDrawOffset = 1f;

        string IDrawable.Name { get; set; } = "Center";

        int IDrawable.Id { get; set; }

        private readonly Graph<Node<Coordinate>, Coordinate> graph;
        private readonly List<Miner> miners = new();
        Coordinate INodeContainable<Coordinate>.NodeCoordinate { get; set; }

        public Center(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph)
        {
            this.graph = graph;
            
            node.AddNodeContainable(this);
            ((IDrawable)this).Id = Drawables.AddDrawable(this);
            
            EventSystem.Subscribe<RequestedMinerCreationEvent>(CreateMiner);
        }

        ~Center()
        {
            miners.Clear();
            EventSystem.Unsubscribe<RequestedMinerCreationEvent>(CreateMiner);
        }

        public Vector3 GetPosition()
        {
            return new Vector3(((INodeContainable<Coordinate>)this).NodeCoordinate.X, HeightDrawOffset, ((INodeContainable<Coordinate>)this).NodeCoordinate.Y);
        }

        public void Update()
        {
        }

        private void CreateMiner(RequestedMinerCreationEvent obj)
        {
            miners.Add(new Miner(graph.GetNodeAt(((INodeContainable<Coordinate>)this).NodeCoordinate), graph));
        }
    }
}