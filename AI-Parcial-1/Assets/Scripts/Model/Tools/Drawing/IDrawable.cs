using System.Numerics;

namespace Model.Tools.Drawing
{
    public interface IDrawable
    {
        public string Name { get; protected set; }
        public int Id { get; protected set; }
        
        public Vector3 GetPosition();
    }
}