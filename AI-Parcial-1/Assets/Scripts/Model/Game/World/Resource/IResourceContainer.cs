namespace Model.Game.World.Resource
{
    public interface IResourceContainer<TValue> where TValue : struct
    {
        public TValue ContainingQty { get; }
        public TValue Max { get; }
        public TValue SpaceAvailable { get; }
        public bool IsEmpty { get; }
        public bool IsFull { get; }

        public TValue Add(TValue qty);

        public TValue Get(TValue qty);
    }
}