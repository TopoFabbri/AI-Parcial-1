using System.Numerics;

namespace Model.Tools.Drawing
{
    public interface ILocalizable : ITextable
    {
        public string Name { get; }
        public int Id { get; }
        
        public Vector3 GetPosition();
        public void Update();
        public void Destroy();
    }
}