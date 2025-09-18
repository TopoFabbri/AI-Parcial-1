using System;
using System.Collections.Generic;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Game.World.Resource;
using Model.Tools.FSM;
using Model.Tools.Pathfinder.Algorithms;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Time;
using Model.Tools.Voronoi;

namespace Model.Game.World.Agents.CaravanStates
{
    public class HideState : State
    {
    }

    public class MoveState : State
    {
        public override Type[] OnEnterParamTypes =>
            new[] { typeof(Pathfinder<Node<Coordinate>, Coordinate>), typeof(Node<Coordinate>), typeof(Node<Coordinate>), typeof(Graph<Node<Coordinate>, Coordinate>) };

        public override Type[] OnTickParamTypes => new[] { typeof(Graph<Node<Coordinate>, Coordinate>), typeof(INodeContainable<Coordinate>), typeof(float) };

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
                    if (nodeContainable is Mine)
                    {
                        flag?.Invoke(Caravan.Flags.ReachedMine);
                        break;
                    }

                    if (nodeContainable is Center)
                    {
                        flag?.Invoke(Model.AlarmRaised ? Caravan.Flags.StayHidden : Caravan.Flags.ReachedCenter);
                        break;
                    }
                }
            }

            graph.MoveContainableTo(miner, path[currentNodeIndex].GetCoordinate());
        }
    }

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

    public class FindMineState : State
    {
        public override Type[] OnTickParamTypes => new[] { typeof(Coordinate), typeof(Coordinate) };

        private List<Mine> mines;

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            Coordinate moverCoordinate = parameters[0] as Coordinate;
            Coordinate targetCoordinate = parameters[1] as Coordinate;

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () =>
            {
                GetClosestMine(moverCoordinate, targetCoordinate);
                flag?.Invoke(Caravan.Flags.MineFound);
            });

            return behaviourActions;
        }

        private static void GetClosestMine(Coordinate moverCoordinate, Coordinate targetCoordinate)
        {
            Coordinate voronoiMapCoordinate = VoronoiRegistry<Node<Coordinate>, Coordinate>.GetClosestTo(typeof(Mine), moverCoordinate);
            targetCoordinate.Set(voronoiMapCoordinate.X, voronoiMapCoordinate.Y);
        }
    }
    
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
                flag?.Invoke(Caravan.Flags.CenterFound);
            });

            return behaviourActions;
        }
    }

    public class DepositState : State
    {
        private FoodContainer mineFoodContainer;

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

            behaviourActions.AddMultiThreadableBehaviour(0, () => { DepositFood(foodContainer); });

            return behaviourActions;
        }

        private void SetFoodContainer(Graph<Node<Coordinate>, Coordinate> graph, Coordinate coordinate)
        {
            if (graph == null || coordinate == null) return;

            foreach (INodeContainable<Coordinate> nodeContainable in graph.Nodes[coordinate].GetNodeContainables())
            {
                if (nodeContainable is Mine mine)
                    mineFoodContainer = mine.FoodContainer;
            } 
        }

        private void CheckFoodContainer()
        {
            if (mineFoodContainer == null)
                flag.Invoke(Caravan.Flags.FoodDepleted);
        }

        private void DepositFood(FoodContainer foodContainer)
        {
            if (foodContainer == null) return;

            mineFoodContainer.Add(foodContainer.Get(mineFoodContainer.SpaceAvailable));
                
            if (foodContainer.IsEmpty) flag.Invoke(Caravan.Flags.FoodDeposited);
        }
    }
}