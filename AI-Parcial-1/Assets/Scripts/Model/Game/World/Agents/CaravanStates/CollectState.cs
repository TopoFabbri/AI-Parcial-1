using System;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Game.World.Resource;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World.Agents.CaravanStates
{
    public class CollectState : State
    {
        private FoodContainer centerFoodContainer;

        public override Type[] OnEnterParamTypes => new[] { typeof(Graph<Node<Coordinate>, Coordinate>), typeof(Coordinate) };
        public override Type[] OnTickParamTypes => new[] { typeof(FoodContainer) };

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            Graph<Node<Coordinate>, Coordinate> graph = parameters[0] as Graph<Node<Coordinate>, Coordinate>;
            Coordinate coordinate = parameters[1] as Coordinate;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { SetFoodContainer(graph, coordinate); });
            behaviourActions.AddMultiThreadableBehaviour(1, CheckFoodContainer);

            return behaviourActions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            FoodContainer foodContainer = parameters[0] as FoodContainer;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { CollectFood(foodContainer); });

            return behaviourActions;
        }

        private void SetFoodContainer(Graph<Node<Coordinate>, Coordinate> graph, Coordinate coordinate)
        {
            if (graph == null || coordinate == null) return;

            foreach (INodeContainable<Coordinate> nodeContainable in graph.Nodes[coordinate].GetNodeContainables())
            {
                if (nodeContainable is Center center)
                    centerFoodContainer = center.FoodContainer;
            }
        }

        private void CheckFoodContainer()
        {
            if (centerFoodContainer == null)
                flag.Invoke(Caravan.Flags.FoodDepleted);
        }

        private void CollectFood(FoodContainer foodContainer)
        {
            if (foodContainer == null) return;

            foodContainer.Add(centerFoodContainer.Get(foodContainer.SpaceAvailable));

            if (foodContainer.IsFull) flag.Invoke(Caravan.Flags.FoodFilled);
        }
    }
}