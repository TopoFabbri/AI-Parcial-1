using Model.Tools.ECS.Patron;

namespace Model.Tools.Flocking.Scripts
{
    public class BoidComponent : EcsComponent
    {
        public float alignmentFactor;
        public float cohesionFactor;
        public float detectionRadius;
        public float directionFactor;
        public float separationFactor;

        public float speed;
        public float targetX;
        public float targetY;
        public float targetZ;
        public float turnSpeed;

        public float upX;
        public float upY;
        public float upZ;

        public float x;
        public float y;
        public float z;

        public BoidComponent(float speed, float turnSpeed, float detectionRadius, float alignmentFactor, float cohesionFactor, float separationFactor, float directionFactor,
            float x, float y, float z, float upX, float upY, float upZ)
        {
            this.speed = speed;
            this.turnSpeed = turnSpeed;
            this.detectionRadius = detectionRadius;

            this.alignmentFactor = alignmentFactor;
            this.cohesionFactor = cohesionFactor;
            this.separationFactor = separationFactor;
            this.directionFactor = directionFactor;

            this.x = x;
            this.y = y;
            this.z = z;

            this.upX = upX;
            this.upY = upY;
            this.upZ = upZ;
        }
    }
}