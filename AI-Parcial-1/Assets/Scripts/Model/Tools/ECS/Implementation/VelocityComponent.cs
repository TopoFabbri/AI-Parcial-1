using Model.Tools.ECS.Patron;

namespace Model.Tools.ECS.Implementation
{
    public class VelocityComponent : EcsComponent
    {
        public float directionX;
        public float directionY;
        public float directionZ;
        public float velocity;

        public VelocityComponent(float velocity, float directionX, float directionY, float directionZ)
        {
            this.velocity = velocity;
            this.directionX = directionX;
            this.directionY = directionY;
            this.directionZ = directionZ;
        }
    }
}