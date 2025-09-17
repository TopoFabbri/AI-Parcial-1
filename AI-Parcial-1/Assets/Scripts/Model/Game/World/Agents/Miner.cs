using System.Numerics;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Game.World.Agents.MinerStates;
using Model.Game.World.Resource;
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

        private readonly float mineSpeed;
        private readonly float moveSpeed;
        
        private const float GoldPerFoodUnit = 3f;
        private const float HeightDrawOffset = 1f;
        private readonly Graph<Node<Coordinate>, Coordinate> graph;
        private readonly Pathfinder<Node<Coordinate>, Coordinate> pathfinder;

        private Coordinate targetCoordinate;

        private GoldContainer GoldContainer { get; }

        #endregion
        
        #region Fsm

        public enum States
        {
            Idle,
            FindMine,
            FindCenter,
            Move,
            Mine,
            Deposit,
            Hide
        }

        public enum Flags
        {
            IdleEnded,
            MineFound,
            ReachedMine,
            AlarmRaised,
            AlarmCleared,
            CenterFound,
            MineDepleted,
            GoldFilled,
            GoldDeposited,
            ReachedCenter,
            StayHidden,
            FoodDepleted
        }

        private FSM<States, Flags> fsm;

        #endregion

        string ILocalizable.Name { get; set; } = "Miner";

        int ILocalizable.Id { get; set; }

        public Coordinate NodeCoordinate { get; set; }

        public Miner(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph, float mineSpeed, float moveSpeed, float maxGold, float goldQty = 0)
        {
            this.mineSpeed = mineSpeed;
            this.moveSpeed = moveSpeed;
            
            this.graph = graph;
            GoldContainer = new GoldContainer(goldQty, maxGold);
            pathfinder = new AStarPathfinder<Node<Coordinate>, Coordinate>();
            targetCoordinate = new Coordinate();
            this.mineSpeed = mineSpeed;
            this.moveSpeed = moveSpeed;
            
            this.graph = graph;
            GoldContainer = new GoldContainer(goldQty, maxGold);
            pathfinder = new AStarPathfinder<Node<Coordinate>, Coordinate>();
            targetCoordinate = new Coordinate();
            
            node.AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);
            
            InitializeFSM();

            EventSystem.Subscribe<RaiseAlarmEvent>(OnAlarmRaised);
        }

        private void InitializeFSM()
        {
            fsm = new FSM<States, Flags>(States.FindMine);

            fsm.AddState<IdleState>(States.Idle, onEnterParameters: () => new object[] { graph, NodeCoordinate });
            fsm.AddState<FindMineState>(States.FindMine, onTickParameters: () => new object[] { NodeCoordinate, targetCoordinate });
            fsm.AddState<FindCenterState>(States.FindCenter, onTickParameters: () => new object[] { targetCoordinate });
            fsm.AddState<HideState>(States.Hide);
            fsm.AddState<DepositState>(States.Deposit, 
                onEnterParameters: () => new object[] { graph, NodeCoordinate },
                onTickParameters: () => new object[] { GoldContainer });
            fsm.AddState<MoveState>(States.Move,
                onEnterParameters: () => new object[] { pathfinder, graph.Nodes[NodeCoordinate], graph.Nodes[targetCoordinate], graph },
                onTickParameters: () => new object[] { graph, this, moveSpeed });
            fsm.AddState<MineState>(States.Mine, 
                onEnterParameters: () => new object[] {graph, NodeCoordinate, GoldContainer},
                onTickParameters: () => new object[] { GoldContainer, mineSpeed, GoldPerFoodUnit },
                onExitParameters: () => new object[] { GoldContainer });

            fsm.SetTransition(States.Idle, Flags.IdleEnded, States.Mine);
            fsm.SetTransition(States.Idle, Flags.AlarmRaised, States.FindCenter);
            
            fsm.SetTransition(States.FindMine, Flags.MineFound, States.Move);
            fsm.SetTransition(States.FindMine, Flags.AlarmRaised, States.FindCenter);
            
            fsm.SetTransition(States.Move, Flags.ReachedMine, States.Mine);
            fsm.SetTransition(States.Move, Flags.AlarmRaised, States.FindCenter);
            fsm.SetTransition(States.Move, Flags.ReachedCenter, States.Deposit);
            fsm.SetTransition(States.Move, Flags.StayHidden, States.Hide);
            fsm.SetTransition(States.Move, Flags.AlarmCleared, States.FindMine);
            
            fsm.SetTransition(States.FindCenter, Flags.CenterFound, States.Move);
            
            fsm.SetTransition(States.Hide, Flags.AlarmCleared, States.FindMine);
            
            fsm.SetTransition(States.Mine, Flags.MineDepleted, States.FindMine);
            fsm.SetTransition(States.Mine, Flags.GoldFilled, States.FindCenter);
            fsm.SetTransition(States.Mine, Flags.AlarmRaised, States.FindCenter);
            fsm.SetTransition(States.Mine, Flags.FoodDepleted, States.Idle);
            
            fsm.SetTransition(States.Deposit, Flags.GoldDeposited, States.FindMine);
            fsm.SetTransition(States.Deposit, Flags.AlarmRaised, States.FindCenter);
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
            fsm.Transition(Model.AlarmRaised ? Flags.AlarmRaised : Flags.AlarmCleared);
        }
    }
}