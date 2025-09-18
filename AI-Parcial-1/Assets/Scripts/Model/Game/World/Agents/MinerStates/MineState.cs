using System;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Game.World.Resource;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Time;

namespace Model.Game.World.Agents.MinerStates
{
    public class MineState : State
    {
        private Mine mine;
        private float goldUntilFoodRequired;

        public override Type[] OnEnterParamTypes => new[] { typeof(Graph<Node<Coordinate>, Coordinate>), typeof(Coordinate), typeof(GoldContainer) };
        public override Type[] OnTickParamTypes => new[] { typeof(GoldContainer), typeof(float), typeof(float) };
        public override Type[] OnExitParamTypes => new[] { typeof(GoldContainer) };

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            Graph<Node<Coordinate>, Coordinate> graph = parameters[0] as Graph<Node<Coordinate>, Coordinate>;
            Coordinate coordinate = parameters[1] as Coordinate;
            GoldContainer goldContainer = parameters[2] as GoldContainer;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { SetMine(graph, coordinate); });
            behaviourActions.AddMultiThreadableBehaviour(0, () =>
            {
                if (goldContainer == null) return;

                goldContainer.Filled += OnGoldFilled;
            });

            return behaviourActions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            GoldContainer goldContainer = parameters[0] as GoldContainer;
            float mineSpeed = (float)parameters[1];
            float goldPerFoodUnit = (float)parameters[2];

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { CheckFood(goldPerFoodUnit); });
            behaviourActions.AddMultiThreadableBehaviour(1, () => { MineGold(goldContainer, mineSpeed); });

            return behaviourActions;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            GoldContainer goldContainer = parameters[0] as GoldContainer;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () =>
            {
                if (goldContainer == null) return;

                goldContainer.Filled -= OnGoldFilled;
            });

            return behaviourActions;
        }

        private void SetMine(Graph<Node<Coordinate>, Coordinate> graph, Coordinate coordinate)
        {
            if (graph == null) return;

            foreach (INodeContainable<Coordinate> nodeContainable in graph.GetNodeAt(coordinate).GetNodeContainables())
            {
                if (nodeContainable is Mine mineContainable)
                    mine = mineContainable;
            }
        }

        private void MineGold(GoldContainer goldContainer, float mineSpeed)
        {
            if (mine == null || mine.GoldContainer.IsEmpty)
            {
                flag?.Invoke(Miner.Flags.MineDepleted);
                return;
            }
            
            if (goldUntilFoodRequired <= 0f) return;
            
            float goldMined = mine.GoldContainer.Get(mineSpeed * Time.TickTime);
            goldContainer.Add(goldMined);
            goldUntilFoodRequired -= goldMined;
        }

        private void CheckFood(float goldPerFoodUnit)
        {
            if (mine == null) return;
            
            FoodContainer foodContainer = mine.FoodContainer;
            
            if (foodContainer == null || foodContainer.IsEmpty)
            {
                flag.Invoke(Miner.Flags.FoodDepleted);
                return;
            }

            if (goldUntilFoodRequired > 0f) return;
            
            foodContainer.Get(1);
            goldUntilFoodRequired += goldPerFoodUnit;
        }
        
        private void OnGoldFilled()
        {
            flag?.Invoke(Miner.Flags.GoldFilled);
        }
    }
}