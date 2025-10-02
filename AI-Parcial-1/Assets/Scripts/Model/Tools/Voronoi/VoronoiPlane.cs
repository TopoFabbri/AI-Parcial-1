using System;
using System.Numerics;

namespace Model.Tools.Voronoi
{
    public sealed class VoronoiPlane
    {
        private readonly Vector3 normal;
        private readonly float distance;

        private readonly Vector3 altNormal;
        private readonly float altDistance;

        private readonly bool separated;
        
        public VoronoiPlane(Vector3 normal, Vector3 point, Vector3 altPoint)
        {
            this.normal = Vector3.Normalize(normal);
            distance = -Vector3.Dot(this.normal, point);

            altNormal = -Vector3.Normalize(normal);
            altDistance = -Vector3.Dot(altNormal, altPoint);

            separated = !GetSide(altPoint);
        }

        internal float GetDistanceToPoint(Vector3 point)
        {
            float dis = Vector3.Dot(normal, point) + distance;
            float altDis = Vector3.Dot(altNormal, point) + altDistance;

            if ((dis >= 0 && altDis >= 0) || !separated)
                return Math.Min(dis, altDis);
            
            return Math.Max(dis, altDis);
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
    }
}