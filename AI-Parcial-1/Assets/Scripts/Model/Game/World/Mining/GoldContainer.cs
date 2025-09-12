namespace Model.Game.World.Mining
{
    public class GoldContainer
    {
        private float startingGold;
        private float containingGold;
        
        public GoldContainer(float startingGold)
        {
            this.startingGold = startingGold;
            containingGold = startingGold;
        }
        
        public void AddGold(float qty)
        {
            containingGold += qty;
        }

        public float GetGold(float qty)
        {
            if (qty > containingGold)
            {
                containingGold = 0;
                return containingGold;
            }
            
            containingGold -= qty;
            return qty;
        }
    }
}