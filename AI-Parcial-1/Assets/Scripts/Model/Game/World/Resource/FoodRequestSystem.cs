using System.Collections.Generic;
using Model.Game.Graph;

namespace Model.Game.World.Resource
{
    public static class FoodRequestSystem
    {
        private static readonly Queue<Coordinate> FoodRequests = new();
        private static readonly Dictionary<int, Coordinate> AssignedRequests = new();
        
        public static void RequestFood(Coordinate requestCoordinate)
        {
            if (!FoodRequests.Contains(requestCoordinate))
                FoodRequests.Enqueue(requestCoordinate);
        }
        
        public static Coordinate GetNextRequest(int id)
        {
            if (FoodRequests.Count == 0) return null;
            
            Coordinate requestLocation = FoodRequests.Dequeue();
            AssignedRequests.TryAdd(id, requestLocation);
            return requestLocation;
        }

        public static void RequestCompleted(int id)
        {
            AssignedRequests.Remove(id);
        }

        public static void RequestReleased(int id)
        {
            if (AssignedRequests.TryGetValue(id, out Coordinate requestLocation))
                FoodRequests.Enqueue(requestLocation);
        }
        
        public static void Clear()
        {
            FoodRequests.Clear();
        }
    }
}