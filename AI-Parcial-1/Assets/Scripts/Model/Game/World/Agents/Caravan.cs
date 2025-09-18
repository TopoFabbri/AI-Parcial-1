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
    public class Caravan : ILocalizable, INodeContainable<Coordinate>
    {
        #region Fields

        private readonly Graph<Node<Coordinate>, Coordinate> graph;
        private Pathfinder<Node<Coordinate>, Coordinate> pathfinder;

        public FoodContainer FoodContainer { get; private set; }

        private const float HeightDrawOffset = 1f;

        private float moveSpeed;
        private Coordinate targetCoordinate;

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
            FoodDepleted
        }

        private readonly FSM<States, Flags> fsm;

        #endregion

        public Caravan(Graph<Node<Coordinate>, Coordinate> graph, Pathfinder<Node<Coordinate>, Coordinate> pathfinder, Coordinate coordinate, int maxFood, float moveSpeed,
            int startingFood)
        {
            this.graph = graph;
            this.pathfinder = pathfinder;
            this.moveSpeed = moveSpeed;
            
            targetCoordinate = new Coordinate();

            FoodContainer = new FoodContainer(startingFood, maxFood);

            graph.Nodes[coordinate].AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);

            // FSM start
            fsm = new FSM<States, Flags>(States.Hide);

            fsm.AddState<MoveState>(States.Move,
                onEnterParameters: () => new object[] { pathfinder, graph.Nodes[NodeCoordinate], graph.Nodes[targetCoordinate], graph },
                onTickParameters: () => new object[] { graph, this, moveSpeed });

            fsm.AddState<MoveState>(States.Move);

            // FSM end

            EventSystem.Subscribe<RaiseAlarmEvent>(OnAlarmRaised);
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
            float x = NodeCoordinate.X * graph.GetNodeDistance();
            float y = NodeCoordinate.Y * graph.GetNodeDistance();

            return new Vector3(x, HeightDrawOffset, y);
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

        private void OnAlarmRaised(RaiseAlarmEvent raiseAlarmEvent)
        {
        }
    }
}