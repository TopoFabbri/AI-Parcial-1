using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Tools.ECS.Patron;

namespace Model.Tools.ECS.Implementation
{
    public sealed class MovementSystem : EcsSystem
    {
        private ParallelOptions parallelOptions;

        private IDictionary<uint, PositionComponent> positionComponents;
        private IEnumerable<uint> queryedEntities;
        private IDictionary<uint, VelocityComponent> velocityComponents;

        public override void Initialize()
        {
            parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 32 };
        }

        protected override void PreExecute(float deltaTime)
        {
            positionComponents ??= EcsManager.GetComponents<PositionComponent>();
            velocityComponents ??= EcsManager.GetComponents<VelocityComponent>();
            queryedEntities ??= EcsManager.GetEntitiesWithComponentTypes(typeof(PositionComponent), typeof(VelocityComponent));
        }

        protected override void Execute(float deltaTime)
        {
            Parallel.ForEach(queryedEntities, parallelOptions, i =>
            {
                positionComponents[i].x += velocityComponents[i].directionX * velocityComponents[i].velocity * deltaTime;
                positionComponents[i].y += velocityComponents[i].directionY * velocityComponents[i].velocity * deltaTime;
                positionComponents[i].z += velocityComponents[i].directionZ * velocityComponents[i].velocity * deltaTime;
            });
        }

        protected override void PostExecute(float deltaTime)
        {
        }
    }
}