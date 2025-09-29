using System;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Game.World.Resource;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World.Agents.CaravanStates
{
    public class DepositState : State
    {
        private FoodContainer mineFoodContainer;
        private int caravanId;

        public override Type[] OnEnterParamTypes => new[] { typeof(Graph<Node<Coordinate>, Coordinate>), typeof(Coordinate), typeof(int) };
        public override Type[] OnTickParamTypes => new[] { typeof(FoodContainer) };

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            Graph<Node<Coordinate>, Coordinate> graph = parameters[0] as Graph<Node<Coordinate>, Coordinate>;
            Coordinate coordinate = parameters[1] as Coordinate;
            caravanId = (int)parameters[2];

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { SetFoodContainer(graph, coordinate); });
            behaviourActions.AddMultiThreadableBehaviour(1, CheckFoodContainer);

            return behaviourActions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            FoodContainer foodContainer = parameters[0] as FoodContainer;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { DepositFood(foodContainer); });

            return behaviourActions;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMainThreadBehaviour(0, () => { FoodRequestSystem.RequestReleased(caravanId); });

            return behaviourActions;
        }

        private void SetFoodContainer(Graph<Node<Coordinate>, Coordinate> graph, Coordinate coordinate)
        {
            if (graph == null || coordinate == null) return;

            foreach (INodeContainable<Coordinate> nodeContainable in graph.Nodes[coordinate].GetNodeContainables())
            {
                if (nodeContainable is Mine mine)
                    mineFoodContainer = mine.FoodContainer;
            }
        }

        private void CheckFoodContainer()
        {
            if (mineFoodContainer == null)
                flag.Invoke(Caravan.Flags.FoodDeposited);
        }

        private void DepositFood(FoodContainer foodContainer)
        {
            if (foodContainer == null)
            {
                flag.Invoke(Caravan.Flags.FoodDeposited);
                FoodRequestSystem.RequestReleased(caravanId);
                return;
            }

            mineFoodContainer.Add(foodContainer.Get(mineFoodContainer.SpaceAvailable));

            if (!foodContainer.IsEmpty) return;
            
            flag.Invoke(Caravan.Flags.FoodDeposited);
            FoodRequestSystem.RequestCompleted(caravanId);
        }
    }
}