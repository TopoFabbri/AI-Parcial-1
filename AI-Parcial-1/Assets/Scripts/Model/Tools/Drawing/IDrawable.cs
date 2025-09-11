using System.Numerics;

namespace Model.Tools.Drawing
{
    public interface IDrawable
    {
        public string Name { get; protected set; }
        public int Id { get; internal set; }
        
        public Vector3 GetPosition();
    }
}