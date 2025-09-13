using System;
using Model.Tools.FSM;
using Model.Tools.Time;

namespace Model.Game.World.Agents.MinerStates
{
    public class IdleState : State
    {
        public override Type[] OnTickParamTypes => new[]
        {
            typeof(float)
        };

        private DateTime enterTime;

        public override BehaviourActions GetOnEnterBehaviours(params object[] parameters)
        {
            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { enterTime = Time.DateTime; });

            return behaviourActions;
        }

        public override BehaviourActions GetOnTickBehaviours(params object[] parameters)
        {
            float idleTime = (float)parameters[0];

            BehaviourActions behaviourActions = Pool.Get<BehaviourActions>();

            behaviourActions.AddMultiThreadableBehaviour(0, () => { CheckIfEnded(idleTime); });

            return behaviourActions;
        }

        private void CheckIfEnded(float idleTime)
        {
            float currentTime = (float)(Time.DateTime - enterTime).TotalSeconds;
            
            if (currentTime > idleTime)
            {
                OnFlag?.Invoke(Miner.Flags.IdleEnded);
            }
        }
    }
}