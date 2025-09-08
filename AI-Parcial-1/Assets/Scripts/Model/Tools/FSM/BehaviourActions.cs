using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Model.Tools.Pool;

namespace Model.Tools.FSM
{
    public class BehaviourActions : IResetable, IEquatable<BehaviourActions>
    {
        public Dictionary<int, List<Action>> MainThreadBehaviours { get; private set; }

        public ConcurrentDictionary<int, ConcurrentBag<Action>> MultiThreadableBehaviours { get; private set; }

        public Action TransitionBehaviour { get; private set; }

        public bool Equals(BehaviourActions other)
        {
            return Equals(MainThreadBehaviours, other.MainThreadBehaviours) && Equals(MultiThreadableBehaviours, other.MultiThreadableBehaviours) &&
                   Equals(TransitionBehaviour, other.TransitionBehaviour);
        }

        public void Reset()
        {
            if (MainThreadBehaviours != null)
            {
                foreach (KeyValuePair<int, List<Action>> behaviour in MainThreadBehaviours)
                    behaviour.Value.Clear();

                MainThreadBehaviours.Clear();
            }

            if (MultiThreadableBehaviours != null)
            {
                foreach (KeyValuePair<int, ConcurrentBag<Action>> behaviour in MultiThreadableBehaviours)
                    behaviour.Value.Clear();

                MultiThreadableBehaviours.Clear();
            }

            TransitionBehaviour = null;
        }

        public void AddMainThreadBehaviour(int executionOrder, Action behaviour)
        {
            MainThreadBehaviours ??= new Dictionary<int, List<Action>>();

            if (!MainThreadBehaviours.ContainsKey(executionOrder))
                MainThreadBehaviours.Add(executionOrder, new List<Action>());

            MainThreadBehaviours[executionOrder].Add(behaviour);
        }

        public void AddMultiThreadableBehaviour(int executionOrder, Action behaviour)
        {
            MultiThreadableBehaviours ??= new ConcurrentDictionary<int, ConcurrentBag<Action>>();

            if (!MultiThreadableBehaviours.ContainsKey(executionOrder))
                MultiThreadableBehaviours.TryAdd(executionOrder, new ConcurrentBag<Action>());

            MultiThreadableBehaviours[executionOrder].Add(behaviour);
        }

        public void SetTransitionBehaviour(Action behaviour)
        {
            TransitionBehaviour = behaviour;
        }

        public override bool Equals(object obj)
        {
            return obj is BehaviourActions other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MainThreadBehaviours, MultiThreadableBehaviours, TransitionBehaviour);
        }
    }
}