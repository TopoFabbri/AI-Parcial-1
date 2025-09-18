using System;

namespace Model.Game.World.Resource
{
    public class FoodContainer : IResourceContainer<int>
    {
        public event Action Depleted;
        public event Action Filled;

        public int ContainingQty { get; private set; }
        public int Max { get; }
        public int SpaceAvailable => Max - ContainingQty;
        public bool IsEmpty => ContainingQty <= 0;
        public bool IsFull => ContainingQty >= Max;
        
        public FoodContainer(int startingQty, int maxQty)
        {
            ContainingQty = startingQty;
            Max = maxQty;
        }
        
        public int Add(int qty)
        {
            ContainingQty += qty;

            if (ContainingQty < Max) return qty;
            
            qty = Max - ContainingQty;
            ContainingQty = Max;
            Filled?.Invoke();
            
            return qty;
        }

        public int Get(int qty)
        {
            if (qty > ContainingQty)
            {
                qty = ContainingQty;
                ContainingQty = 0;
                Depleted?.Invoke();
                
                return qty;
            }
            
            ContainingQty -= qty;
            return qty;
        }
    }
}