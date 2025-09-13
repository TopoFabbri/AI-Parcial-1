using System;
using System.Collections.Generic;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Voronoi;

namespace Model.Game.World.Agents.MinerStates
{
    public class FindState : State
    {
        public override Type[] OnTickParamTypes => new[] { typeof(Coordinate), typeof(Coordinate) };

        private List<Mine> mines;

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Coordinate minerCoordinate = parameters[0] as Coordinate;
            Coordinate closestMineCoordinate = parameters[1] as Coordinate;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () =>
            {
                GetClosestMine(minerCoordinate, closestMineCoordinate);
                OnFlag?.Invoke(Miner.Flags.MineFound);
            });

            return behaviourActions;
        }

        private static void GetClosestMine(Coordinate minerCoordinate, Coordinate closestMineCoordinate)
        {
            Coordinate voronoiMapCoordinate = VoronoiRegistry<Node<Coordinate>, Coordinate>.GetClosestTo(typeof(Mine), minerCoordinate);
            closestMineCoordinate.Set(voronoiMapCoordinate.X, voronoiMapCoordinate.Y);
        }
    }
}