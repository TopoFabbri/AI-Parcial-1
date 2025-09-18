using System.Collections.Generic;
using Model.Tools.EventSystem;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.Events
{
    public class RequestedCaravanCreationEvent : IEvent
    {
        public float moveSpeed;
        public int carryCapacity;
        public List<INode.NodeType> blockedNodes;
        
        public void Reset()
        {
        }

        public void Assign(params object[] parameters)
        {
            moveSpeed = (float)parameters[0];
            carryCapacity = (int)parameters[1];
            blockedNodes = parameters[2] as List<INode.NodeType>;
        }
    }
}