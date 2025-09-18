using System;

namespace Model.Game.World.Resource
{
    public class GoldContainer : IResourceContainer<float>
    {
        public event Action Depleted;
        public event Action Filled;

        public float ContainingQty { get; private set; }
        public float Max { get; }
        public float SpaceAvailable => Max - ContainingQty;

        public bool IsEmpty => ContainingQty <= 0;
        public bool IsFull => ContainingQty >= Max;
        
        public GoldContainer(float startingQty, float maxQty)
        {
            ContainingQty = startingQty;
            Max = maxQty;
        }
        
        public float Add(float qty)
        {
            ContainingQty += qty;

            if (ContainingQty < Max) return qty;
            
            qty = Max - ContainingQty;
            ContainingQty = Max;
            Filled?.Invoke();
            
            return qty;
        }

        public float Get(float qty)
        {
            if (qty > ContainingQty)
            {
                ContainingQty = 0;
                Depleted?.Invoke();
                
                return ContainingQty;
            }
            
            ContainingQty -= qty;
            return qty;
        }
    }
}