using System;

namespace Model.Game.World.Mining
{
    public class GoldContainer
    {
        private readonly float startingGold;
        private readonly float maxGold;
        private float containingGold;
        
        public event Action Depleted;
        public event Action Filled;
        
        public GoldContainer(float startingGold, float maxGold)
        {
            this.startingGold = startingGold;
            containingGold = startingGold;
            this.maxGold = maxGold;
        }

        public bool IsEmpty => containingGold <= 0;
        public bool IsFull => containingGold >= maxGold;

        public void AddGold(float qty)
        {
            containingGold += qty;

            if (containingGold < maxGold) return;
            
            containingGold = maxGold;
            Filled?.Invoke();
        }

        public float GetGold(float qty)
        {
            if (qty > containingGold)
            {
                containingGold = 0;
                Depleted?.Invoke();
                
                return containingGold;
            }
            
            containingGold -= qty;
            return qty;
        }
    }
}