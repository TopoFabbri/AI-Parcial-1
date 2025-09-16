using System.Numerics;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Game.World.Agents.MinerStates;
using Model.Game.World.Mining;
using Model.Tools.Drawing;
using Model.Tools.EventSystem;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Algorithms;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World.Agents
{
    public class Miner : ILocalizable, INodeContainable<Coordinate>
    {
        #region Fields

        private float mineSpeed = 1f;
        private float moveSpeed = 1f;
        
        private const float idleTime = 1f;
        private const float HeightDrawOffset = 1f;
        private readonly Graph<Node<Coordinate>, Coordinate> graph;
        private AStarPathfinder<Node<Coordinate>, Coordinate> pathfinder;

        private Coordinate targetCoordinate;
        
        public GoldContainer GoldContainer { get; set; }

        #endregion
        
        #region Fsm

        public enum States
        {
            Idle,
            FindMine,
            FindCenter,
            Move,
            Mine
        }

        public enum Flags
        {
            IdleEnded,
            MineFound,
            ReachedTarget,
            AlarmRaised,
            CenterFound,
            MineDepleted,
            GoldFilled
        }

        private readonly FSM<States, Flags> fsm;

        #endregion

        string ILocalizable.Name { get; set; } = "Miner";

        int ILocalizable.Id { get; set; }

        public Coordinate NodeCoordinate { get; set; }

        public Miner(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph, float mineSpeed, float moveSpeed, float maxGold, int goldQty = 0)
        {
            this.mineSpeed = mineSpeed;
            this.moveSpeed = moveSpeed;
            
            this.graph = graph;
            GoldContainer = new GoldContainer(goldQty, maxGold);
            pathfinder = new AStarPathfinder<Node<Coordinate>, Coordinate>();
            targetCoordinate = new Coordinate();
            
            node.AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);
            
            fsm = new FSM<States, Flags>(States.Idle);

            fsm.AddState<IdleState>(States.Idle, onTickParameters: () => new object[] { idleTime });
            fsm.AddState<FindMineState>(States.FindMine, onTickParameters: () => new object[] { NodeCoordinate, targetCoordinate });
            fsm.AddState<FindCenterState>(States.FindCenter, onTickParameters: () => new object[] { targetCoordinate });
            fsm.AddState<MoveState>(States.Move,
                onEnterParameters: () => new object[] { pathfinder, graph.Nodes[NodeCoordinate], graph.Nodes[targetCoordinate], graph },
                onTickParameters: () => new object[] { graph, this, moveSpeed });
            fsm.AddState<MineState>(States.Mine, 
                onEnterParameters: () => new object[] {graph, NodeCoordinate, GoldContainer},
                onTickParameters: () => new object[] { GoldContainer, mineSpeed },
                onExitParameters: () => new object[] { GoldContainer });

            fsm.SetTransition(States.Idle, Flags.IdleEnded, States.FindMine);
            fsm.SetTransition(States.Idle, Flags.AlarmRaised, States.FindCenter);
            
            fsm.SetTransition(States.FindMine, Flags.MineFound, States.Move);
            fsm.SetTransition(States.FindMine, Flags.AlarmRaised, States.FindCenter);
            
            fsm.SetTransition(States.Move, Flags.ReachedTarget, States.Mine);
            fsm.SetTransition(States.Move, Flags.AlarmRaised, States.FindCenter);
            
            fsm.SetTransition(States.FindCenter, Flags.CenterFound, States.Move);
            
            fsm.SetTransition(States.Mine, Flags.MineDepleted, States.FindMine);
            fsm.SetTransition(States.Mine, Flags.GoldFilled, States.FindCenter);
            
            EventSystem.Subscribe<RaiseAlarmEvent>(OnAlarmRaised);
        }
        
        ~Miner()
        {
            EventSystem.Unsubscribe<RaiseAlarmEvent>(OnAlarmRaised);
            Localizables.RemoveLocalizable(this, ((ILocalizable)this).Id);
        }
        
        public void Update()
        {
            fsm.Tick();
        }

        public void Destroy()
        {
            Localizables.RemoveLocalizable(this, ((ILocalizable)this).Id);

            EventSystem.Unsubscribe<RaiseAlarmEvent>(OnAlarmRaised);
        }

        public Vector3 GetPosition()
        {
            float x = NodeCoordinate.X * graph.GetNodeDistance();
            float y = NodeCoordinate.Y * graph.GetNodeDistance();
            
            return new Vector3(x, HeightDrawOffset, y);
        }
        
        private void OnAlarmRaised(RaiseAlarmEvent raiseAlarmEvent)
        {
            fsm.Transition(Flags.AlarmRaised);
        }
    }
}