using System;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.FSM;

namespace Model.Game.World.Agents.MinerStates
{
    public class FindCenterState : State
    {
        public override Type[] OnTickParamTypes => new[] { typeof(Coordinate) };
        
        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Coordinate centerCoordinate = parameters[0] as Coordinate;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () =>
            {
                if (centerCoordinate == null) return;
                
                centerCoordinate.Set(Center.GetCoordinate().X, Center.GetCoordinate().Y);
                flag?.Invoke(Miner.Flags.CenterFound);
            });

            return behaviourActions;
        }
    }
}