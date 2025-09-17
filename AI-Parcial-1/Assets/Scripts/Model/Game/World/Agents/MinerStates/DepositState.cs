using System;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Game.World.Resource;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World.Agents.MinerStates
{
    public class DepositState : State
    {
        private Center center;
        
        public override Type[] OnEnterParamTypes => new[] { typeof(Graph<Node<Coordinate>, Coordinate>), typeof(Coordinate) };
        public override Type[] OnTickParamTypes => new[] { typeof(GoldContainer) };

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            Graph<Node<Coordinate>, Coordinate> graph = parameters[0] as Graph<Node<Coordinate>, Coordinate>;
            Coordinate coordinate = parameters[1] as Coordinate;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { SetCenter(graph, coordinate); });

            return behaviourActions;
        }
        
        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            GoldContainer minerContainer = parameters[0] as GoldContainer;
            
            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();
            
            behaviourActions.AddMultiThreadableBehaviour(0, () => { DepositGold(minerContainer); });
            
            return behaviourActions;
        }
        
        private void SetCenter(Graph<Node<Coordinate>, Coordinate> graph, Coordinate coordinate)
        {
            if (graph == null) return;

            foreach (INodeContainable<Coordinate> nodeContainable in graph.GetNodeAt(coordinate).GetNodeContainables())
            {
                if (nodeContainable is Center centerContainable )
                    center = centerContainable;
            }
        }

        private void DepositGold(GoldContainer minerContainer)
        {
            center.GoldContainer.Add(minerContainer.Get(minerContainer.ContainingQty));
            flag.Invoke(Miner.Flags.GoldDeposited);
        }
    }
}