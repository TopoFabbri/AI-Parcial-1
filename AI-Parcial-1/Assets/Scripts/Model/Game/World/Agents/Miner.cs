using System;
using System.Collections.Generic;
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
using Model.Tools.Time;

namespace Model.Game.World.Agents
{
    public class Miner : ILocalizable, INodeContainable<Coordinate>
    {
        #region Fields

        private readonly float mineSpeed;
        private readonly float moveSpeed;
        
        private Vector3 position;
        
        private const float GoldPerFoodUnit = 3f;
        private const float HeightDrawOffset = 1f;
        private readonly Graph<Node<Coordinate>, Coordinate> graph;
        private readonly Pathfinder<Node<Coordinate>, Coordinate> pathfinder;
        private readonly List<INode.NodeType> blockedNodes;

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
            FoodDepleted,
            TargetNotFound
        }

        private FSM<States, Flags> fsm;

        #endregion

        string ILocalizable.Name { get; set; } = "Miner";

        int ILocalizable.Id { get; set; }

        public Coordinate NodeCoordinate { get; set; }

        public Miner(Node<Coordinate> node, Graph<Node<Coordinate>, Coordinate> graph, List<INode.NodeType> blockedNodes, float mineSpeed, float moveSpeed, float maxGold, float goldQty = 0)
        {
            this.mineSpeed = mineSpeed;
            this.moveSpeed = moveSpeed;
            
            this.graph = graph;
            this.blockedNodes = blockedNodes;
            
            GoldContainer = new GoldContainer(goldQty, maxGold);
            pathfinder = new ThetaStarPathfinder<Node<Coordinate>, Coordinate>();
            targetCoordinate = new Coordinate();
            
            node.AddNodeContainable(this);
            ((ILocalizable)this).Id = Localizables.AddLocalizable(this);
            
            (float x, float y) = graph.GetPositionFromCoordinate(NodeCoordinate);
            
            position = new Vector3(x, 0f, y);
            
            InitializeFSM();

            EventSystem.Subscribe<RaiseAlarmEvent>(OnAlarmRaised);
        }

        private Func<Vector3, Vector3> MoveTowardsFunc => MoveTowards;
        
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
                onEnterParameters: () => new object[] { pathfinder, graph.Nodes[NodeCoordinate], graph.Nodes[targetCoordinate], graph, blockedNodes },
                onTickParameters: () => new object[] { graph, MoveTowardsFunc, NodeCoordinate });
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
            fsm.SetTransition(States.Move, Flags.TargetNotFound, States.FindMine);
            
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

        public string GetHoverText()
        {
            string infoText = ((ILocalizable)this).Name + " " + ((ILocalizable)this).Id + ":\n";
            infoText += "Gold " + Math.Round(GoldContainer.ContainingQty);

            return infoText;
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

        public Vector3 GetPosition()
        {
            return position + Vector3.UnitY * HeightDrawOffset;
        }
        
        private void OnAlarmRaised(RaiseAlarmEvent raiseAlarmEvent)
        {
            fsm.Transition(Model.AlarmRaised ? Flags.AlarmRaised : Flags.AlarmCleared);
        }

        public Vector3 MoveTowards(Vector3 target)
        {
            if (graph == null) return position;
            
            Vector3 direction = target - position;
            
            if (graph.IsCircumnavigable())
            {
                Coordinate size = graph.GetSize();
                float worldWidth = size.X * graph.GetNodeDistance();
                float worldHeight = size.Y * graph.GetNodeDistance();

                if (worldWidth > 0)
                {
                    if (MathF.Abs(direction.X) > worldWidth / 2f)
                        direction.X -= MathF.Sign(direction.X) * worldWidth;
                }

                if (worldHeight > 0)
                {
                    if (MathF.Abs(direction.Z) > worldHeight / 2f)
                        direction.Z -= MathF.Sign(direction.Z) * worldHeight;
                }
            }
            
            if (direction.LengthSquared() > (Vector3.Normalize(direction) * moveSpeed * Time.TickTime).LengthSquared())
                direction = Vector3.Normalize(direction) * moveSpeed * Time.TickTime;
            
            position += direction;
            
            if (graph.IsCircumnavigable())
            {
                Coordinate size = graph.GetSize();
                float worldWidth = size.X * graph.GetNodeDistance();
                float worldHeight = size.Y * graph.GetNodeDistance();

                if (worldWidth > 0)
                {
                    // Wrap X into [0, worldWidth)
                    position.X %= worldWidth;
                    if (position.X < 0) position.X += worldWidth;
                }
                if (worldHeight > 0)
                {
                    position.Z %= worldHeight;
                    if (position.Z < 0) position.Z += worldHeight;
                }
            }
            
            graph.MoveContainableTo(this, graph.GetNodeFromPosition(position.X, position.Z).GetCoordinate());
            
            return position;
        }
    }
}