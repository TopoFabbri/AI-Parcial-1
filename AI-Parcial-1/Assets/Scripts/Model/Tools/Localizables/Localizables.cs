using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Model.Tools.Drawing
{
    public static class Localizables
    {
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<int, ILocalizable>> LocalizablesList = new();

        public static int AddLocalizable(ILocalizable localizable)
        {
            if (!LocalizablesList.ContainsKey(localizable.Name))
                LocalizablesList.TryAdd(localizable.Name, new ConcurrentDictionary<int, ILocalizable>());

            int id = GetAvailableId(localizable.Name);

            LocalizablesList[localizable.Name].TryAdd(id, localizable);

            return id;
        }

        public static void RemoveLocalizable(ILocalizable localizable, int id)
        {
            if (LocalizablesList.TryGetValue(localizable.Name, out ConcurrentDictionary<int, ILocalizable> drawablesByObjName))
                drawablesByObjName.TryRemove(id, out _);
        }

        public static ConcurrentBag<ILocalizable> GetLocalizablesOfName(string name)
        {
            return LocalizablesList.TryGetValue(name, out ConcurrentDictionary<int, ILocalizable> drawablesByObjName)
                ? new ConcurrentBag<ILocalizable>(drawablesByObjName.Values)
                : new ConcurrentBag<ILocalizable>();
        }

        public static ICollection<string> GetLocalizableNames()
        {
            return LocalizablesList.Keys;
        }

        public static void Clear()
        {
            List<ILocalizable> localizables = new();
            foreach (string name in LocalizablesList.Keys)
                localizables.AddRange(LocalizablesList[name].Values);

            foreach (ILocalizable localizable in localizables)
                localizable.Destroy();

            LocalizablesList.Clear();
        }

        private static int GetAvailableId(string name)
        {
            int id = 0;

            while (LocalizablesList[name].ContainsKey(id))
                id++;

            return id;
        }
    }
}