using System;

namespace Model.Game.World.Mining
{
    public class GoldContainer
    {
        private readonly float startingGold;
        private readonly float maxGold;

        public event Action Depleted;
        public event Action Filled;
        
        public GoldContainer(float startingGold, float maxGold)
        {
            this.startingGold = startingGold;
            ContainingGold = startingGold;
            this.maxGold = maxGold;
        }
        
        public float ContainingGold { get; private set; }
        public bool IsEmpty => ContainingGold <= 0;
        public bool IsFull => ContainingGold >= maxGold;

        public void AddGold(float qty)
        {
            ContainingGold += qty;

            if (ContainingGold < maxGold) return;
            
            ContainingGold = maxGold;
            Filled?.Invoke();
        }

        public float GetGold(float qty)
        {
            if (qty > ContainingGold)
            {
                ContainingGold = 0;
                Depleted?.Invoke();
                
                return ContainingGold;
            }
            
            ContainingGold -= qty;
            return qty;
        }
    }
}