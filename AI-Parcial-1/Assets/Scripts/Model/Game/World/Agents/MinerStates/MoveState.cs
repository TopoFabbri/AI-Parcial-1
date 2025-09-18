using System;
using System.Collections.Generic;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Algorithms;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Time;

namespace Model.Game.World.Agents.MinerStates
{
    public class MoveState : State
    {
        public override Type[] OnEnterParamTypes => new[]
        {
            typeof(Pathfinder<Node<Coordinate>, Coordinate>), 
            typeof(Node<Coordinate>), 
            typeof(Node<Coordinate>), 
            typeof(Graph<Node<Coordinate>, Coordinate>)
        };

        public override Type[] OnTickParamTypes => new[]
        {
            typeof(Graph<Node<Coordinate>, Coordinate>), 
            typeof(INodeContainable<Coordinate>), 
            typeof(float)
        };

        private DateTime enterTime;
        private List<Node<Coordinate>> path;
        private int currentNodeIndex;

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            Pathfinder<Node<Coordinate>, Coordinate> pathfinder = parameters[0] as Pathfinder<Node<Coordinate>, Coordinate>;
            Node<Coordinate> startNode = parameters[1] as Node<Coordinate>;
            Node<Coordinate> targetNode = parameters[2] as Node<Coordinate>;
            Graph<Node<Coordinate>, Coordinate> graph = parameters[3] as Graph<Node<Coordinate>, Coordinate>;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () =>
            {
                currentNodeIndex = 0;
                enterTime = Time.DateTime;
            });

            behaviourActions.AddMultiThreadableBehaviour(0, () =>
            {
                if (pathfinder != null)
                    path = pathfinder.FindPath(startNode, targetNode, graph);
                else
                    path = new List<Node<Coordinate>> { targetNode };

                path.Insert(0, startNode);
            });

            return behaviourActions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Graph<Node<Coordinate>, Coordinate> graph = parameters[0] as Graph<Node<Coordinate>, Coordinate>;
            INodeContainable<Coordinate> miner = parameters[1] as INodeContainable<Coordinate>;
            float speed = (float)parameters[2];

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { MoveToTargetCoordinate(graph, miner, speed); });

            return behaviourActions;
        }

        private void MoveToTargetCoordinate(Graph<Node<Coordinate>, Coordinate> graph, INodeContainable<Coordinate> miner, float speed)
        {
            currentNodeIndex = (int)((Time.DateTime - enterTime).TotalSeconds * speed);

            if (currentNodeIndex >= path.Count)
            {
                currentNodeIndex = path.Count - 1;

                foreach (INodeContainable<Coordinate> nodeContainable in graph.Nodes[path[currentNodeIndex].GetCoordinate()].GetNodeContainables())
                {
                    if (nodeContainable is Mine mine)
                    {
                        flag?.Invoke(Miner.Flags.ReachedMine);
                        break;
                    }

                    if (nodeContainable is Center center)
                    {
                        flag?.Invoke(Model.AlarmRaised ? Miner.Flags.StayHidden : Miner.Flags.ReachedCenter);
                        break;
                    }
                }
            }

            graph.MoveContainableTo(miner, path[currentNodeIndex].GetCoordinate());
        }
    }
}