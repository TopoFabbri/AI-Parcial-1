using System;
using System.Collections.Concurrent;

namespace Model.Tools.Pool
{
    public class ConcurrentPool
    {
        private readonly ConcurrentDictionary<Type, ConcurrentStack<IResetable>> Pool = new();

        public TResetable Get<TResetable>(params object[] parameters) where TResetable : IResetable
        {
            if (!Pool.ContainsKey(typeof(TResetable)))
                Pool.TryAdd(typeof(TResetable), new ConcurrentStack<IResetable>());

            TResetable value;

            if (Pool[typeof(TResetable)].Count > 0)
            {
                Pool[typeof(TResetable)].TryPop(out IResetable resetable);
                value = (TResetable)resetable;
            }
            else
            {
                value = (TResetable)Activator.CreateInstance(typeof(TResetable),parameters);
            }

            return value;
        }

        public void Release<TResetable>(TResetable obj) where TResetable : IResetable
        {
            obj.Reset();
            Pool[typeof(TResetable)].Push(obj);
        }
    }
}