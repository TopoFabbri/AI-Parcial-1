using System;
using System.Collections.Generic;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Game.World.Resource;
using Model.Tools.FSM;

namespace Model.Game.World.Agents.CaravanStates
{
    public class FindMineState : State
    {
        public override Type[] OnTickParamTypes => new[] { typeof(Coordinate) };

        private List<Mine> mines;

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Coordinate targetCoordinate = parameters[0] as Coordinate;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { GetMineWithFoodRequest(targetCoordinate); });

            return behaviourActions;
        }

        private void GetMineWithFoodRequest(Coordinate targetCoordinate)
        {
            Coordinate requestCoordinate = FoodRequestSystem.GetNextRequest();
            
            if (requestCoordinate == null) return;
            
            targetCoordinate.Set(requestCoordinate.X, requestCoordinate.Y);
            flag?.Invoke(Caravan.Flags.MineFound);
        }
    }
}