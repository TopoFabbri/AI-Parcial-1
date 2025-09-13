using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Model.Tools.Pool;

namespace Model.Tools.FSM
{
    public class FSM<TStateType, TFlagType> where TStateType : Enum where TFlagType : Enum
    {
        private const int UnassignedTransition = -1;
        private readonly Dictionary<int, Func<object[]>> behaviourOnEnterParameters;
        private readonly Dictionary<int, Func<object[]>> behaviourOnExitParameters;
        private readonly Dictionary<int, Func<object[]>> behaviourOnTickParameters;
        
        private readonly ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 32 };
        private readonly Dictionary<int, State> states;

        private readonly (int destinationState, Action onTrnasition)[,] transitions;
        private int currentState;

        public FSM(TStateType defaultState)
        {
            states = new Dictionary<int, State>();
            transitions = new (int, Action)[Enum.GetValues(typeof(TStateType)).Length, Enum.GetValues(typeof(TFlagType)).Length];
            for (int i = 0; i < transitions.GetLength(0); i++)
            for (int j = 0; j < transitions.GetLength(1); j++)
                transitions[i, j] = (UnassignedTransition, null);

            behaviourOnTickParameters = new Dictionary<int, Func<object[]>>();
            behaviourOnEnterParameters = new Dictionary<int, Func<object[]>>();
            behaviourOnExitParameters = new Dictionary<int, Func<object[]>>();
            ForceState(defaultState);
        }

        private BehaviourActions GetCurrentTickBehaviour => states[currentState].GetOnTickBehaviours(behaviourOnTickParameters[currentState]?.Invoke());

        private BehaviourActions GetCurrentOnEnterBehaviour => states[currentState].GetOnEnterBehaviours(behaviourOnEnterParameters[currentState]?.Invoke());

        private BehaviourActions GetCurrentOnExitBehaviour => states[currentState].GetOnExitBehaviour(behaviourOnExitParameters[currentState]?.Invoke());

        public void AddState<TState>(TStateType stateIndex, Func<object[]> onTickParameters = null, Func<object[]> onEnterParameters = null, Func<object[]> onExitParameters = null)
            where TState : State, new()
        {
            if (states.ContainsKey(Convert.ToInt32(stateIndex))) return;
            TState state = new();

            ValidateParams(state.OnEnterParamTypes, onEnterParameters);
            ValidateParams(state.OnTickParamTypes, onTickParameters);
            ValidateParams(state.OnExitParamTypes, onExitParameters);

            state.OnFlag += Transition;
            states.Add(Convert.ToInt32(stateIndex), state);
            behaviourOnTickParameters.Add(Convert.ToInt32(stateIndex), onTickParameters);
            behaviourOnEnterParameters.Add(Convert.ToInt32(stateIndex), onEnterParameters);
            behaviourOnExitParameters.Add(Convert.ToInt32(stateIndex), onExitParameters);
        }

        private void ForceState(TStateType state)
        {
            currentState = Convert.ToInt32(state);
        }

        public void SetTransition(TStateType originalState, TFlagType flag, TStateType destinationState, Action onTransition = null)
        {
            transitions[Convert.ToInt32(originalState), Convert.ToInt32(flag)] = (Convert.ToInt32(destinationState), onTransition);
        }

        public void Transition(Enum flag)
        {
            if (states.ContainsKey(currentState)) ExecuteBehaviour(GetCurrentOnExitBehaviour);

            if (transitions[Convert.ToInt32(currentState), Convert.ToInt32(flag)].destinationState == UnassignedTransition) return;

            transitions[currentState, Convert.ToInt32(flag)].onTrnasition?.Invoke();
            currentState = transitions[Convert.ToInt32(currentState), Convert.ToInt32(flag)].destinationState;
            ExecuteBehaviour(GetCurrentOnEnterBehaviour);
        }

        public void Tick()
        {
            if (states.ContainsKey(currentState)) ExecuteBehaviour(GetCurrentTickBehaviour);
        }

        private void ExecuteBehaviour(BehaviourActions behaviourActions)
        {
            if (behaviourActions == null)
                return;

            int executionOrder = 0;

            while ((behaviourActions.MainThreadBehaviours != null && behaviourActions.MainThreadBehaviours.Count > 0) ||
                   (behaviourActions.MultiThreadableBehaviours != null && behaviourActions.MultiThreadableBehaviours.Count > 0))
            {
                Task multithradableBehaviour = new(() =>
                {
                    if (behaviourActions.MultiThreadableBehaviours == null) return;
                    if (!behaviourActions.MultiThreadableBehaviours.TryGetValue(executionOrder, out ConcurrentBag<Action> currentBehaviour)) return;

                    Parallel.ForEach(currentBehaviour, parallelOptions, behaviour => { behaviour?.Invoke(); });
                    behaviourActions.MultiThreadableBehaviours.TryRemove(executionOrder, out _);
                });

                multithradableBehaviour.Start();

                if (behaviourActions.MainThreadBehaviours != null)
                    if (behaviourActions.MainThreadBehaviours.ContainsKey(executionOrder))
                    {
                        foreach (Action behaviour in behaviourActions.MainThreadBehaviours[executionOrder]) behaviour?.Invoke();
                        behaviourActions.MainThreadBehaviours.Remove(executionOrder);
                    }

                multithradableBehaviour?.Wait();
                executionOrder++;
            }

            behaviourActions.TransitionBehaviour?.Invoke();
            State.Pool.Release(behaviourActions);
        }

        public void ValidateParams(Type[] expectedParams, Func<object[]> receivedParams)
        {
            if (expectedParams.Length == 0 && receivedParams == null) return;

            List<Type> receivedParamTypes = new();

            foreach (object param in receivedParams.Invoke())
                receivedParamTypes.Add(param.GetType());

            if (expectedParams.Length != receivedParamTypes.Count)
                throw new ArgumentException("Wrong number of parameters");

            for (int i = 0; i < expectedParams.Length; i++)
                if (!expectedParams[i].IsAssignableFrom(receivedParamTypes[i]))
                    throw new InvalidCastException("Param " + i + ". Type: " + receivedParamTypes[i].Name + ". Expected type: " + expectedParams[i].Name);
        }
    }
}