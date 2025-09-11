using Model.Game.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Game
{
    public class Model
    {
        public Graph<Node<Coordinate>, Coordinate> Graph { get; private set; }

        public Model()
        {
        }
        
        public Graph<Node<Coordinate>, Coordinate> CreateGraph(int width, int height, float nodeDistance = 1f, bool circumnavigable = false)
        {
            Graph = new Graph<Node<Coordinate>, Coordinate>(width, height, nodeDistance, circumnavigable);
            
            return Graph;
        }
        
        public void Update()
        {
        }
    }
}