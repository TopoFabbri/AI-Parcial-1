using System.Collections.Generic;
using System.Numerics;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Game.World.Agents.CaravanStates;
using Model.Game.World.Resource;
using Model.Tools.Drawing;
using Model.Tools.EventSystem;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Algorithms;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Time;

namespace Model.Game.World.Agents
{
    public class Caravan : ILocalizable, INodeContainable<Coordinate>
    {
        #region Fields

        private readonly Graph<Node<Coordinate>, Coordinate> graph;
        private readonly Pathfinder<Node<Coordinate>, Coordinate> pathfinder;
        private readonly List<INode.NodeType> blockedNodes;

        private FoodContainer FoodContainer { get; }

        private const float HeightDrawOffset = 1f;

        private readonly float moveSpeed;
        private readonly Coordinate targetCoordinate;
        
        private Vector3 position;

        #endregion

        #region FSM

        public enum States
        {
            Hide,
            FindCenter,
            FindMine,
            Move,
            Collect,
            Deposit
        }

        public enum Flags
        {
            AlarmRaised,
            AlarmCleared,
            CenterFound,
            MineFound,
            ReachedCenter,
            ReachedMine,
            FoodFilled,
            FoodDeposited,
            StayHidden,
            FoodDepleted,
            TargetNotFound
        }

        private FSM<States, Flags> fsm;

        #endregion

        public Caravan(Graph<Node<Coordinate>, Coordinate> graph, Pathfinder<Node<Coordinate>, Coordinate> pathfinder, Coordinate coordinate, List<INode.NodeType> blockedNodes,
            int maxFood, float moveSpeed, int startingFood)
        {
            this.graph = graph;
            this.pathfinder = pathfinder;
            this.moveSpeed = moveSpeed;
            this.blockedNodes = blockedNodes;

            targetCoordinate = new Coordinate();

            FoodContainer = new FoodContainer(startingFood, maxFood);

            graph.Nodes[coordinate].AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);

            (float x, float y) = graph.GetPositionFromCoordinate(NodeCoordinate);
            
            position = new Vector3(x , 0f, y);
            
            InitializeFSM();

            EventSystem.Subscribe<RaiseAlarmEvent>(OnAlarmRaised);
        }

        private void InitializeFSM()
        {
            fsm = new FSM<States, Flags>(States.FindCenter);

            fsm.AddState<HideState>(States.Hide);
            fsm.AddState<MoveState>(States.Move,
                onEnterParameters: () => new object[] { pathfinder, graph.Nodes[NodeCoordinate], graph.Nodes[targetCoordinate], graph, blockedNodes },
                onTickParameters: () => new object[] { graph, this });
            fsm.AddState<CollectState>(States.Collect, onEnterParameters: () => new object[] { graph, NodeCoordinate }, onTickParameters: () => new object[] { FoodContainer });
            fsm.AddState<FindMineState>(States.FindMine, () => new object[] { targetCoordinate });
            fsm.AddState<FindCenterState>(States.FindCenter, () => new object[] { targetCoordinate });
            fsm.AddState<DepositState>(States.Deposit, onEnterParameters: () => new object[] { graph, NodeCoordinate }, onTickParameters: () => new object[] { FoodContainer });

            fsm.SetTransition(States.Collect, Flags.FoodFilled, States.FindMine);
            fsm.SetTransition(States.Collect, Flags.FoodDepleted, States.FindCenter);
            fsm.SetTransition(States.Collect, Flags.AlarmRaised, States.FindCenter);

            fsm.SetTransition(States.FindMine, Flags.MineFound, States.Move);
            fsm.SetTransition(States.FindMine, Flags.AlarmRaised, States.FindCenter);

            fsm.SetTransition(States.Move, Flags.ReachedCenter, States.Collect);
            fsm.SetTransition(States.Move, Flags.ReachedMine, States.Deposit);
            fsm.SetTransition(States.Move, Flags.StayHidden, States.Hide);
            fsm.SetTransition(States.Move, Flags.AlarmCleared, States.FindCenter);
            fsm.SetTransition(States.Move, Flags.AlarmRaised, States.FindCenter);
            fsm.SetTransition(States.Move, Flags.TargetNotFound, States.FindMine);

            fsm.SetTransition(States.FindCenter, Flags.CenterFound, States.Move);
            fsm.SetTransition(States.FindCenter, Flags.AlarmRaised, States.FindCenter);
            fsm.SetTransition(States.FindCenter, Flags.AlarmCleared, States.FindMine);

            fsm.SetTransition(States.Hide, Flags.AlarmCleared, States.FindCenter);

            fsm.SetTransition(States.Deposit, Flags.FoodDeposited, States.FindCenter);
            fsm.SetTransition(States.Deposit, Flags.AlarmRaised, States.FindCenter);
        }

        string ILocalizable.Name { get; set; } = "Caravan";

        int ILocalizable.Id { get; set; }

        public string GetHoverText()
        {
            string infoText = ((ILocalizable)this).Name + " " + ((ILocalizable)this).Id + ":\n";
            infoText += "Food " + FoodContainer.ContainingQty;

            return infoText;
        }

        public Coordinate NodeCoordinate { get; set; }

        public Vector3 GetPosition()
        {
            return position + Vector3.UnitY * HeightDrawOffset;
        }

        public Vector3 MoveTowards(Vector3 target)
        {
            Vector3 direction = target - position;
            
            if (direction.LengthSquared() > (Vector3.Normalize(direction) * moveSpeed * Time.TickTime).LengthSquared())
                direction = Vector3.Normalize(direction) * moveSpeed * Time.TickTime;
            
            position += direction;
            
            graph.MoveContainableTo(this, graph.GetNodeFromPosition(position.X, position.Z).GetCoordinate());

            return position;
        }

        public void Update()
        {
            fsm.Tick();
        }

        public void Destroy()
        {
            graph.Nodes[NodeCoordinate].RemoveNodeContainable(this);
            Localizables.RemoveLocalizable(this, ((ILocalizable)this).Id);

            EventSystem.Unsubscribe<RaiseAlarmEvent>(OnAlarmRaised);
        }

        private void OnAlarmRaised(RaiseAlarmEvent raiseAlarmEvent)
        {
            fsm.Transition(Model.AlarmRaised ? Flags.AlarmRaised : Flags.AlarmCleared);
        }
    }
}