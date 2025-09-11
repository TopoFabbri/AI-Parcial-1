using System.Numerics;
using Model.Game.Graph;
using Model.Game.World.Agents.MinerStates;
using Model.Tools.Drawing;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World.Agents
{
    public class Miner : IDrawable, INodeContainable<Coordinate>
    {
        #region Fields

        private const float mineSpeed = 1f;
        private const float moveSpeed = 1f;
        private const float HeightDrawOffset = 1f;
        private Graph<Node<Coordinate>, Coordinate> graph;

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

        string IDrawable.Name { get; set; } = "Miner";

        int IDrawable.Id { get; set; }

        Coordinate INodeContainable<Coordinate>.NodeCoordinate { get; set; }

        public Miner(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph)
        {
            this.graph = graph;
            fsm = new FSM<States, Flags>(States.Idle);

            fsm.AddState<IdleState>(States.Idle, () => new object[] { });

            node.AddNodeContainable(this);
            ((IDrawable)this).Id = Drawables.AddDrawable(this);
        }
        
        public void Update()
        {
            fsm.Tick();
        }

        public Vector3 GetPosition()
        {
            return new Vector3(((INodeContainable<Coordinate>)this).NodeCoordinate.X, HeightDrawOffset, ((INodeContainable<Coordinate>)this).NodeCoordinate.Y);
        }
    }
}