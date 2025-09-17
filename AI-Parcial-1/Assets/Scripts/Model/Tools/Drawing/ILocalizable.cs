using System.Numerics;

namespace Model.Tools.Drawing
{
    public interface ILocalizable : ITextable
    {
        public string Name { get; protected set; }
        public int Id { get; internal set; }
        
        public Vector3 GetPosition();
        public void Update();
        public void Destroy();
    }
}