using System.Collections.Generic;
using Model.Tools.EventSystem;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.Events
{
    public class RequestedMinerCreationEvent : IEvent
    {
        public float mineSpeed;
        public float moveSpeed;
        public float maxGold;
        public List<INode.NodeType> blockedTypes;
        
        public void Reset()
        {
        }

        public void Assign(params object[] parameters)
        {
            moveSpeed = (float)parameters[0];
            mineSpeed = (float)parameters[1];
            maxGold = (float)parameters[2];
            blockedTypes = parameters[3] as List<INode.NodeType>;
        }
    }
}