using System;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Game.World.Resource;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Time;

namespace Model.Game.World.Agents.MinerStates
{
    public class IdleState : State
    {
        private Timer timer;
        private FoodContainer foodContainer;
        private Coordinate currentCoordinate;

        private const float TimeToRequestFood = 5f;

        public override Type[] OnEnterParamTypes => new[]
        {
            typeof(Graph<Node<Coordinate>, Coordinate>),
            typeof(Coordinate)
        };

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            Graph<Node<Coordinate>, Coordinate> graph = parameters[0] as Graph<Node<Coordinate>, Coordinate>;
            Coordinate coordinate = parameters[1] as Coordinate;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { SetFoodContainer(graph, coordinate); });
            behaviourActions.AddMultiThreadableBehaviour(0, StartTimer);

            return behaviourActions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, CheckIfEnded);
            behaviourActions.AddMultiThreadableBehaviour(0, CheckTimer);

            return behaviourActions;
        }

        private void SetFoodContainer(Graph<Node<Coordinate>, Coordinate> graph, Coordinate coordinate)
        {
            currentCoordinate = coordinate;
            if (graph == null) return;

            foreach (INodeContainable<Coordinate> nodeContainable in graph.GetNodeAt(coordinate).GetNodeContainables())
            {
                if (nodeContainable is Mine mineContainable)
                    foodContainer = mineContainable.FoodContainer;
            }
        }

        private void CheckIfEnded()
        {
            if (foodContainer == null || !foodContainer.IsEmpty)
                flag.Invoke(Miner.Flags.IdleEnded);
        }

        private void CheckTimer()
        {
            if (timer == null || timer.TimeElapsed > TimeToRequestFood) return;
            
            FoodRequestSystem.RequestFood(currentCoordinate);
            timer = new Timer();
        }

        private void StartTimer()
        {
            timer = new Timer();
        }
    }
}