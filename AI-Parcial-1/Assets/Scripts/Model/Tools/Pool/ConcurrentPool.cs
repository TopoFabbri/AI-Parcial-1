using System;
using System.Collections.Concurrent;

namespace Model.Tools.Pool
{
    public class ConcurrentPool
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentStack<IResetable>> Pool = new();

        public static TResetable Get<TResetable>() where TResetable : IResetable, new()
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
                value = new TResetable();
            }

            return value;
        }

        public static void Release<TResetable>(TResetable obj) where TResetable : IResetable, new()
        {
            obj.Reset();
            Pool[typeof(TResetable)].Push(obj);
        }
    }
}