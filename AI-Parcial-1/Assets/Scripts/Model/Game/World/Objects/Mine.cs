using System.Collections.Generic;
using System.Numerics;
using Model.Game.Graph;
using Model.Game.World.Resource;
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

        public GoldContainer GoldContainer { get; }

        string ILocalizable.Name { get; set; } = "Mine";
        int ILocalizable.Id { get; set; }
        public Coordinate NodeCoordinate { get; set; }

        public Mine(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph, int goldQty)
        {
            this.graph = graph;

            Mines.Add(this);

            GoldContainer = new GoldContainer(goldQty, goldQty);
            GoldContainer.Depleted += Destroy;

            node.AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);
        }
        
        ~Mine()
        {
            Localizables.RemoveLocalizable(this, ((ILocalizable)this).Id);
            
            GoldContainer.Depleted -= Destroy;
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
            Mines.Remove(this);
            Localizables.RemoveLocalizable(this, ((ILocalizable)this).Id);
            
            VoronoiRegistry<Node<Coordinate>, Coordinate>.GenerateVoronoi(typeof(Mine), graph, Mines);
        }

        public void Update()
        {
        }
    }
}