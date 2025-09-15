using Model.Tools.EventSystem;

namespace Model.Game.Events
{
    public class DebugEvent : IEvent
    {
        public string Message { get; private set; }
        
        public void Reset()
        {
            Message = "";
        }

        public void Assign(params object[] parameters)
        {
            Message = (string)parameters[0];
        }
    }
}