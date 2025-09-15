using Model.Tools.EventSystem;

namespace Model.Game.Events
{
    public class RequestedCaravanCreationEvent : IEvent
    {
        public float moveSpeed;
        public int carryCapacity;
        
        public void Reset()
        {
        }

        public void Assign(params object[] parameters)
        {
            moveSpeed = (float)parameters[0];
            carryCapacity = (int)parameters[1];
        }
    }
}