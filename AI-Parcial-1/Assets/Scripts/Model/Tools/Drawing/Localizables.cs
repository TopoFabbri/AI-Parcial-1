using System.Collections.Generic;

namespace Model.Tools.Drawing
{
    public static class Localizables
    {
        private static readonly Dictionary<string, Dictionary<int, ILocalizable>> DrawablesList = new();
        
        public static int AddLocalizable(ILocalizable localizable)
        {
            if (!DrawablesList.ContainsKey(localizable.Name))
                DrawablesList.Add(localizable.Name, new Dictionary<int, ILocalizable>());
            
            int id = GetAvailableId(localizable.Name);
            
            DrawablesList[localizable.Name].Add(id, localizable);
            
            return id;
        }

        public static void RemoveLocalizable(ILocalizable localizable, int id)
        {
            if (DrawablesList.TryGetValue(localizable.Name, out Dictionary<int, ILocalizable> drawablesByObjName))
                drawablesByObjName.Remove(id);
        }
        
        public static List<ILocalizable> GetLocalizablesOfName(string name)
        {
            return DrawablesList.TryGetValue(name, out Dictionary<int, ILocalizable> drawablesByObjName) ? new List<ILocalizable>(drawablesByObjName.Values) : new List<ILocalizable>();
        }

        public static ICollection<string> GetLocalizableNames()
        {
            return DrawablesList.Keys;
        }

        public static void Clear()
        {
            List<ILocalizable> localizables = new();
            foreach (string name in DrawablesList.Keys)
                localizables.AddRange(DrawablesList[name].Values);
            
            foreach (ILocalizable localizable in localizables)
                localizable.Destroy();
            
            DrawablesList.Clear();
        }
        
        private static int GetAvailableId(string name)
        {
            int id = 0;
            
            while (DrawablesList[name].ContainsKey(id))
                id++;
            
            return id;
        }
    }
}