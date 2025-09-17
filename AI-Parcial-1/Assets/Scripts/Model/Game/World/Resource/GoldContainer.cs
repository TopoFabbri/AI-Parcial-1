using System;

namespace Model.Game.World.Resource
{
    public class GoldContainer : IResourceContainer<float>
    {
        private readonly float maxQty;

        public event Action Depleted;
        public event Action Filled;

        public float ContainingQty { get; private set; }
        public bool IsEmpty => ContainingQty <= 0;
        public bool IsFull => ContainingQty >= maxQty;
        
        public GoldContainer(float startingQty, float maxQty)
        {
            ContainingQty = startingQty;
            this.maxQty = maxQty;
        }
        
        public float Add(float qty)
        {
            ContainingQty += qty;

            if (ContainingQty < maxQty) return qty;
            
            qty = maxQty - ContainingQty;
            ContainingQty = maxQty;
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