namespace Model.Game.World.Mining
{
    public class GoldContainer
    {
        private int startingGold;
        private int containingGold;
        
        public GoldContainer(int startingGold)
        {
            this.startingGold = startingGold;
            containingGold = startingGold;
        }
        
        public void AddGold(int qty)
        {
            containingGold += qty;
        }

        public int GetGold(int qty)
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