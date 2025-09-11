using System.Numerics;
using Model.Game.World.Agents.MinerStates;
using Model.Tools.Drawing;
using Model.Tools.FSM;

namespace Model.Game.World.Agents
{
    public class Miner : IDrawable
    {
        #region Fields

        private float mineSpeed = 1f;
        private float moveSpeed = 1f;

        private Vector3 position;

        #endregion
        
        #region Fsm

        public enum States
        {
            Idle,
            FindMine,
            FindPath,
            Move,
            Mine,
            Hide
        }

        public enum Flags
        {
            MineFound,
            PathFound,
            ReachedTarget,
            AlarmTriggered,
        }

        public FSM<States, Flags> fsm;

        #endregion

        string IDrawable.Name { get; set; } = "Miner";

        int IDrawable.Id { get; set; }
        
        public Miner(Vector3 position)
        {
            fsm = new FSM<States, Flags>(States.Idle);

            fsm.AddState<IdleState>(States.Idle, () => new object[] { });
            
            this.position = position;
            ((IDrawable)this).Id = Drawables.AddDrawable(this);
        }
        
        public void Update()
        {
            fsm.Tick();
        }

        public Vector3 GetPosition()
        {
            return position;
        }
    }
}