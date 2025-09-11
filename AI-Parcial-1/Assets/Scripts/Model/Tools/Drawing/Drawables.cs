using System.Collections.Generic;

namespace Model.Tools.Drawing
{
    public static class Drawables
    {
        private static readonly Dictionary<string, Dictionary<int, IDrawable>> DrawablesList = new();
        
        public static int AddDrawable(IDrawable drawable)
        {
            if (!DrawablesList.ContainsKey(drawable.Name))
                DrawablesList.Add(drawable.Name, new Dictionary<int, IDrawable>());
            
            int id = GetAvailableId(drawable.Name);
            
            DrawablesList[drawable.Name].Add(id, drawable);
            
            return id;
        }

        public static void RemoveDrawable(IDrawable drawable, int id)
        {
            if (DrawablesList.TryGetValue(drawable.Name, out Dictionary<int, IDrawable> drawablesByObjName))
                drawablesByObjName.Remove(id);
        }
        
        public static List<IDrawable> GetDrawablesOfName(string name)
        {
            return DrawablesList.TryGetValue(name, out Dictionary<int, IDrawable> drawablesByObjName) ? new List<IDrawable>(drawablesByObjName.Values) : new List<IDrawable>();
        }

        public static ICollection<string> GetDrawableNames()
        {
            return DrawablesList.Keys;
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