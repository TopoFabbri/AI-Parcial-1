using System;

namespace Model.Game.World.Resource
{
    public class FoodContainer : IResourceContainer<int>
    {
        private readonly int maxQty;

        public event Action Depleted;
        public event Action Filled;

        public int ContainingQty { get; private set; }
        public bool IsEmpty => ContainingQty <= 0;
        public bool IsFull => ContainingQty >= maxQty;
        
        public FoodContainer(int startingQty, int maxQty)
        {
            ContainingQty = startingQty;
            this.maxQty = maxQty;
        }
        
        public int Add(int qty)
        {
            ContainingQty += qty;

            if (ContainingQty < maxQty) return qty;
            
            qty = maxQty - ContainingQty;
            ContainingQty = maxQty;
            Filled?.Invoke();
            
            return qty;
        }

        public int Get(int qty)
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