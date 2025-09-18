using System;
using System.Collections.Generic;
using System.Numerics;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Game.World.Agents;
using Model.Game.World.Resource;
using Model.Tools.Drawing;
using Model.Tools.EventSystem;
using Model.Tools.Pathfinder.Algorithms;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World.Objects
{
    public class Center : ILocalizable, INodeContainable<Coordinate>
    {
        private const float MaxGold = 10000f;
        private const int MaxFood = 10000;
        private const float HeightDrawOffset = 1f;
        private static Coordinate _centerCoordinate;

        private string infoText;

        public GoldContainer GoldContainer { get; }
        public FoodContainer FoodContainer { get; private set; }

        string ILocalizable.Name { get; set; } = "Center";

        int ILocalizable.Id { get; set; }

        private readonly Graph<Node<Coordinate>, Coordinate> graph;
        private readonly List<Miner> miners = new();
        private readonly List<Caravan> caravans = new();
        public Coordinate NodeCoordinate { get; set; }

        public Center(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph)
        {
            this.graph = graph;
            GoldContainer = new GoldContainer(0, MaxGold);
            FoodContainer = new FoodContainer(MaxFood, MaxFood);

            node.AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);

            _centerCoordinate = NodeCoordinate;

            EventSystem.Subscribe<RequestedMinerCreationEvent>(CreateMiner);
            EventSystem.Subscribe<RequestedCaravanCreationEvent>(CreateCaravan);
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

        public void Destroy()
        {
            _centerCoordinate = new Coordinate();
            Localizables.RemoveLocalizable(this, ((ILocalizable)this).Id);

            EventSystem.Unsubscribe<RequestedMinerCreationEvent>(CreateMiner);
            EventSystem.Unsubscribe<RequestedCaravanCreationEvent>(CreateCaravan);
        }

        private void CreateMiner(RequestedMinerCreationEvent minerCreationRequest)
        {
            miners.Add(new Miner(graph.GetNodeAt(graph.GetNodeAtIndexes(0, 0).GetCoordinate()), graph, minerCreationRequest.blockedTypes, minerCreationRequest.mineSpeed,
                minerCreationRequest.moveSpeed, minerCreationRequest.maxGold));
        }

        private void CreateCaravan(RequestedCaravanCreationEvent caravanCreationRequest)
        {
            float caravanSpeed = caravanCreationRequest.moveSpeed;
            int caravanCapacity = caravanCreationRequest.carryCapacity;
            List<INode.NodeType> blockedNodes = caravanCreationRequest.blockedNodes;

            caravans.Add(new Caravan(graph, new AStarPathfinder<Node<Coordinate>, Coordinate>(), NodeCoordinate, blockedNodes, caravanCapacity, caravanSpeed, 0));
        }

        public static Coordinate GetCoordinate()
        {
            return _centerCoordinate;
        }

        public string GetHoverText()
        {
            infoText = ((ILocalizable)this).Name + " " + ((ILocalizable)this).Id + ":\n";
            infoText += "Gold " + Math.Round(GoldContainer.ContainingQty, 2) + "\n";
            infoText += "Miners " + miners.Count + "\n";
            infoText += "Caravans " + caravans.Count;
            return infoText;
        }
    }
}