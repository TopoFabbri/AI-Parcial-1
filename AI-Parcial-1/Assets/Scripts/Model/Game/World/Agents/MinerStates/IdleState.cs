using System;
using Model.Game.World.Resource;
using Model.Tools.FSM;
using Model.Tools.Time;

namespace Model.Game.World.Agents.MinerStates
{
    public class IdleState : State
    {
        public override Type[] OnTickParamTypes => new[]
        {
            typeof(FoodContainer)
        };

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            FoodContainer foodContainer = parameters[0] as FoodContainer;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { CheckIfEnded(foodContainer); });

            return behaviourActions;
        }

        private void CheckIfEnded(FoodContainer foodContainer)
        {
            if (foodContainer == null) return;

            if (!foodContainer.IsEmpty)
                flag.Invoke(Miner.Flags.IdleEnded);
        }
    }
}