using System;
using Model.Tools.Pool;

namespace Model.Tools.FSM
{
    public abstract class State
    {
        public Action<Enum> OnFlag;

        public static ConcurrentPool Pool { get; } = new();
        public virtual Type[] OnEnterParamTypes => Array.Empty<Type>();
        public virtual Type[] OnTickParamTypes => Array.Empty<Type>();
        public virtual Type[] OnExitParamTypes => Array.Empty<Type>();

        public virtual BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            return null;
        }

        public virtual BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            return null;
        }

        public virtual BehaviourActions GetOnExitBehaviour(params object[] parameters)
        {
            return null;
        }
    }
}