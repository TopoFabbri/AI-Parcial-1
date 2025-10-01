using System;
using System.Collections.Generic;
using System.Numerics;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Algorithms;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.World.Agents.CaravanStates
{
    public class MoveState : State
    {
        public override Type[] OnEnterParamTypes =>
            new[]
            {
                typeof(Pathfinder<Node<Coordinate>, Coordinate>), 
                typeof(Node<Coordinate>), 
                typeof(Node<Coordinate>), 
                typeof(Graph<Node<Coordinate>, Coordinate>),
                typeof(List<INode.NodeType>),
                typeof(List<Vector3>),
                typeof(Dictionary<INode.NodeType, int>)
            };

        public override Type[] OnTickParamTypes => new[] { typeof(Graph<Node<Coordinate>, Coordinate>), typeof(Func<Vector3, Vector3>), typeof(Coordinate) };

        private List<Vector3> path = new();
        private int currentNodeIndex;

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            Pathfinder<Node<Coordinate>, Coordinate> pathfinder = parameters[0] as Pathfinder<Node<Coordinate>, Coordinate>;
            Node<Coordinate> startNode = parameters[1] as Node<Coordinate>;
            Node<Coordinate> targetNode = parameters[2] as Node<Coordinate>;
            Graph<Node<Coordinate>, Coordinate> graph = parameters[3] as Graph<Node<Coordinate>, Coordinate>;
            List<INode.NodeType> blockedTypes = parameters[4] as List<INode.NodeType>;
            path = parameters[5] as List<Vector3>;
            Dictionary<INode.NodeType, int> nodeCosts = parameters[6] as Dictionary<INode.NodeType, int>;
            
            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { currentNodeIndex = 0; });

            behaviourActions.AddMultiThreadableBehaviour(0, () => { GeneratePath(pathfinder, startNode, targetNode, graph, blockedTypes, nodeCosts); });

            return behaviourActions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Graph<Node<Coordinate>, Coordinate> graph = parameters[0] as Graph<Node<Coordinate>, Coordinate>;
            Func<Vector3, Vector3> moveFunc = parameters[1] as Func<Vector3, Vector3>;
            Coordinate caravanCoordinate = parameters[2] as Coordinate;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { MoveTowardsCoordinate(graph, moveFunc, caravanCoordinate); });

            return behaviourActions;
        }

        public override BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { path.Clear(); });

            return behaviourActions;
        }

        private void GeneratePath(Pathfinder<Node<Coordinate>, Coordinate> pathfinder, Node<Coordinate> startNode, Node<Coordinate> targetNode,
            Graph<Node<Coordinate>, Coordinate> graph, List<INode.NodeType> blockedTypes, Dictionary<INode.NodeType, int> nodeCosts)
        {
            path.Clear();
            currentNodeIndex = 0;

            if (pathfinder == null) return;
            
            List<Node<Coordinate>> nodePath = pathfinder.FindPath(startNode, targetNode, graph, blockedTypes, nodeCosts);

            if (nodePath == null || nodePath.Count == 0)
            {
                flag?.Invoke(Caravan.Flags.PathNotFound);
                return;
            }

            foreach (Node<Coordinate> node in nodePath)
            {
                (float x, float y) = graph.GetPositionFromCoordinate(node.GetCoordinate());
                path.Add(new Vector3(x, 0, y));
            }
        }

        private void MoveTowardsCoordinate(Graph<Node<Coordinate>, Coordinate> graph, Func<Vector3, Vector3> moveTowards, Coordinate caravanCoordinate)
        {
            if (currentNodeIndex < path.Count)
            {
                Vector3 position = moveTowards.Invoke(path[currentNodeIndex]);

                if (Vector3.Distance(position, path[currentNodeIndex]) <= float.Epsilon)
                    currentNodeIndex++;
            }

            if (currentNodeIndex < path.Count)
                return;

            bool targetFound = false;

            foreach (INodeContainable<Coordinate> nodeContainable in graph.Nodes[caravanCoordinate].GetNodeContainables())
            {
                if (nodeContainable is Mine)
                {
                    flag?.Invoke(Caravan.Flags.ReachedMine);
                    targetFound = true;
                    break;
                }

                if (nodeContainable is Center)
                {
                    flag?.Invoke(Model.AlarmRaised ? Caravan.Flags.StayHidden : Caravan.Flags.ReachedCenter);
                    targetFound = true;
                    break;
                }
            }

            if (!targetFound)
                flag?.Invoke(Caravan.Flags.TargetNotFound);
        }
    }
}