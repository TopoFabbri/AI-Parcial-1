using System;
using System.Collections.Generic;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World.Agents.MinerStates
{
    public class FindState : State
    {
        public override Type[] OnTickParamTypes => new[] { typeof(Coordinate), typeof(Coordinate) };

        private List<Mine> mines;

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { mines = GetMines(); });

            return behaviourActions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Coordinate minerCoordinate = parameters[0] as Coordinate;
            Coordinate closestMineCoordinate = parameters[1] as Coordinate;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () =>
            {
                GetClosestMine(mines, minerCoordinate, closestMineCoordinate);
                OnFlag?.Invoke(Miner.Flags.MineFound);
            });

            return behaviourActions;
        }

        private static void GetClosestMine(List<Mine> mines, Coordinate minerCoordinate, Coordinate closestMineCoordinate)
        {
            closestMineCoordinate.Set(((INodeContainable<Coordinate>)mines[0]).NodeCoordinate.X, ((INodeContainable<Coordinate>)mines[0]).NodeCoordinate.Y);
            float closestDistance = closestMineCoordinate.GetDistanceTo(minerCoordinate);

            foreach (Mine mine in mines)
            {
                Coordinate mineCoordinate = ((INodeContainable<Coordinate>)mine).NodeCoordinate;
                float distance = mineCoordinate.GetDistanceTo(minerCoordinate);

                if (!(distance < closestDistance)) continue;

                closestMineCoordinate.Set(mineCoordinate.X, mineCoordinate.Y);
                closestDistance = distance;
            }
        }

        private static List<Mine> GetMines()
        {
            return Mine.Mines;
        }
    }
}