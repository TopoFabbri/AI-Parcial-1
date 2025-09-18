using System.Collections.Generic;
using Model.Game.Graph;

namespace Model.Game.World.Resource
{
    public static class FoodRequestSystem
    {
        private static readonly Queue<Coordinate> FoodRequests = new();
        
        public static void RequestFood(Coordinate requestCoordinate)
        {
            if (!FoodRequests.Contains(requestCoordinate))
                FoodRequests.Enqueue(requestCoordinate);
        }
        
        public static Coordinate GetNextRequest()
        {
            if (FoodRequests.Count == 0) return null;
            return FoodRequests.Dequeue();
        }
    }
}