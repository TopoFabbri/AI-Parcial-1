using System;
using System.Collections.Generic;
using System.Numerics;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Game.World.Resource;
using Model.Tools.Drawing;
using Model.Tools.EventSystem;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Voronoi;

namespace Model.Game.World.Objects
{
    public class Mine : ILocalizable, INodeContainable<Coordinate>, IVoronoiObject<Coordinate>
    {
        public static List<IVoronoiObject<Coordinate>> Mines { get; } = new();
        private const float HeightDrawOffset = 1f;

        private readonly Graph<Node<Coordinate>, Coordinate> graph;

        public GoldContainer GoldContainer { get; }
        public FoodContainer FoodContainer { get; }

        public string Name => "Mine";
        public int Id { get; }
        public Coordinate NodeCoordinate { get; set; }

        public Mine(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph, float goldQty, int maxFoodQty, int foodQty = 2)
        {
            this.graph = graph;

            Mines.Add(this);

            GoldContainer = new GoldContainer(goldQty, goldQty);
            GoldContainer.Depleted += Destroy;

            FoodContainer = new FoodContainer(foodQty, maxFoodQty);

            node.AddNodeContainable(this);
            Id = Localizables.AddLocalizable(this);
        }

        ~Mine()
        {
            GoldContainer.Depleted -= Destroy;
        }

        public string GetHoverText()
        {
            string infoText = ((ILocalizable)this).Name + " " + ((ILocalizable)this).Id + ":\n";
            infoText += "Gold " + Math.Round(GoldContainer.ContainingQty) + "\n";
            infoText += "Food " + FoodContainer.ContainingQty;

            return infoText;
        }

        public Coordinate GetCoordinates()
        {
            return NodeCoordinate;
        }

        public Vector3 GetPosition()
        {
            float x = NodeCoordinate.X * graph.GetNodeDistance();
            float y = NodeCoordinate.Y * graph.GetNodeDistance();

            return new Vector3(x, HeightDrawOffset, y);
        }

        public void Destroy()
        {
            graph.Nodes[NodeCoordinate].RemoveNodeContainable(this);
            Mines.Remove(this);
            Localizables.RemoveLocalizable(this, ((ILocalizable)this).Id);

            VoronoiRegistry<Node<Coordinate>, Coordinate>.GenerateVoronoi(typeof(Mine), Mines,
                new Voronoi<Node<Coordinate>, Coordinate>(new EuclideanDistancePolicy<Node<Coordinate>, Coordinate>(), graph));
            EventSystem.Raise<GraphModifiedEvent>();
        }

        public void Update()
        {
        }
    }
}