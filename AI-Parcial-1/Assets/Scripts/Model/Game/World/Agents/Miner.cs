using System.Numerics;
using Model.Game.Graph;
using Model.Game.World.Agents.MinerStates;
using Model.Game.World.Mining;
using Model.Tools.Drawing;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World.Agents
{
    public class Miner : ILocalizable, INodeContainable<Coordinate>
    {
        #region Fields

        private const float mineSpeed = 1f;
        private const float moveSpeed = 1f;
        private const float HeightDrawOffset = 1f;
        private readonly Graph<Node<Coordinate>, Coordinate> graph;
        public GoldContainer GoldContainer { get; set; }

        #endregion
        
        #region Fsm

        public enum States
        {
            Idle,
            FindMine,
            FindPath,
            Move,
            Mine,
            Hide
        }

        public enum Flags
        {
            MineFound,
            PathFound,
            ReachedTarget,
            AlarmTriggered,
        }

        private readonly FSM<States, Flags> fsm;

        #endregion

        string ILocalizable.Name { get; set; } = "Miner";

        int ILocalizable.Id { get; set; }

        Coordinate INodeContainable<Coordinate>.NodeCoordinate { get; set; }

        public Miner(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph, float goldQty = 0)
        {
            this.graph = graph;
            GoldContainer = new GoldContainer(goldQty);
            
            fsm = new FSM<States, Flags>(States.Idle);

            fsm.AddState<IdleState>(States.Idle, () => new object[] { });

            node.AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);
        }
        
        public void Update()
        {
            fsm.Tick();
        }

        public Vector3 GetPosition()
        {
            float x = ((INodeContainable<Coordinate>)this).NodeCoordinate.X * graph.GetNodeDistance();
            float y = ((INodeContainable<Coordinate>)this).NodeCoordinate.Y * graph.GetNodeDistance();
            
            return new Vector3(x, HeightDrawOffset, y);
        }
    }
}