using System.Collections.Generic;
using System.Numerics;
using Model.Game.Graph;
using Model.Game.World.Mining;
using Model.Tools.Drawing;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Voronoi;

namespace Model.Game.World.Objects
{
    public class Mine : ILocalizable, INodeContainable<Coordinate>, IVoronoiObject<Coordinate>
    {
        public static List<IVoronoiObject<Coordinate>> Mines { get; } = new();
        private const float HeightDrawOffset = 1f;

        private readonly Graph<Node<Coordinate>, Coordinate> graph;

        public GoldContainer GoldContainer { get; private set; }

        string ILocalizable.Name { get; set; } = "Mine";
        int ILocalizable.Id { get; set; }
        public Coordinate NodeCoordinate { get; set; }

        public Mine(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph, int goldQty)
        {
            this.graph = graph;

            Mines.Add(this);

            GoldContainer = new GoldContainer(goldQty);

            node.AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);
            
            VoronoiRegistry<Node<Coordinate>, Coordinate>.GenerateVoronoi(typeof(Mine), graph, Mines);
        }

        public Coordinate GetCoordinates()
        {
            return NodeCoordinate;
        }

        ~Mine()
        {
            Localizables.RemoveLocalizable(this, ((ILocalizable)this).Id);
        }

        public Vector3 GetPosition()
        {
            float x = NodeCoordinate.X * graph.GetNodeDistance();
            float y = NodeCoordinate.Y * graph.GetNodeDistance();

            return new Vector3(x, HeightDrawOffset, y);
        }

        public void Update()
        {
        }
    }
}