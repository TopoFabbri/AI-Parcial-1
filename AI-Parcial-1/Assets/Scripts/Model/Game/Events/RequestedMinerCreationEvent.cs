using Model.Tools.EventSystem;

namespace Model.Game.Events
{
    public class RequestedMinerCreationEvent : IEvent
    {
        public float mineSpeed;
        public float moveSpeed;
        
        public void Reset()
        {
        }

        public void Assign(params object[] parameters)
        {
            moveSpeed = (float)parameters[0];
            mineSpeed = (float)parameters[1];
        }
    }
}