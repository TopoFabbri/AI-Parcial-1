using System.Numerics;

namespace Model.Tools.Voronoi
{
    public sealed class VoronoiPlane
    {
        private readonly Vector3 normal;
        private readonly float distance;

        public VoronoiPlane(Vector3 normal, Vector3 point)
        {
            this.normal = Vector3.Normalize(normal);
            distance = -Vector3.Dot(this.normal, point);
        }

        internal float GetDistanceToPoint(Vector3 point)
        {
            return Vector3.Dot(normal, point) + distance;
        }
        
        public bool GetSide(Vector3 point)
        {
            return GetDistanceToPoint(point) >= -float.Epsilon;
        }

        public Vector3 GetClosestPoint(Vector3 point)
        {
            float dis = GetDistanceToPoint(point);
            return point - normal * dis;
        }
        
        // Expose plane data for drawing/debug visualization
        public Vector3 GetNormal()
        {
            return normal;
        }
        
        public Vector3 GetAnyPoint()
        {
            // Closest point to origin lies on the plane
            return GetClosestPoint(Vector3.Zero);
        }

    }
}