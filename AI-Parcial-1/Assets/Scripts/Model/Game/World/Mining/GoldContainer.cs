using System;

namespace Model.Game.World.Mining
{
    public class GoldContainer
    {
        private float startingGold;
        private float containingGold;
        
        public event Action Depleted;
        
        public GoldContainer(float startingGold)
        {
            this.startingGold = startingGold;
            containingGold = startingGold;
            IsEmpty = false;
        }

        public bool IsEmpty { get; private set; }

        public void AddGold(float qty)
        {
            containingGold += qty;
        }

        public float GetGold(float qty)
        {
            if (qty > containingGold)
            {
                containingGold = 0;
                IsEmpty = true;
                Depleted?.Invoke();
                
                return containingGold;
            }
            
            containingGold -= qty;
            return qty;
        }
    }
}