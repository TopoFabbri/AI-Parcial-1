using System.Collections.Generic;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Voronoi;
using UnityEngine;
using Vector3 = System.Numerics.Vector3;

namespace Engine.View
{
    public class VoronoiPlaneDebugger : MonoBehaviour
    {
        [SerializeField] private float drawHeight = 0.2f;
        [SerializeField] private float drawDistance = 20f;
        
        [SerializeField] private List<bool> minesVisible = new();
        
        private readonly Vector3 center = new(25, 25, 0);
        private Dictionary<Coordinate, List<VoronoiPlane>> planeGroups = new();
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            
            if (planeGroups.Count <= 0)
            {
                planeGroups = VoronoiRegistry<Node<Coordinate>, Coordinate>.GetPlanes(typeof(Mine));
                minesVisible.Clear();
                
                for (int i = 0; i < planeGroups.Count; i++)
                    minesVisible.Add(false);
            }

            int counter = 0;

            foreach (KeyValuePair<Coordinate, List<VoronoiPlane>> planeGroup in planeGroups)
            {
                if (counter < minesVisible.Count && counter > 0 && minesVisible[counter])
                {
                    Gizmos.color = Color.red;

                    foreach (VoronoiPlane plane in planeGroup.Value)
                    {
                        Vector3 closestPoint = plane.GetClosestPoint(center);
                        Vector3 cross = Vector3.Cross(Vector3.UnitZ, closestPoint);
                        Vector3 start = closestPoint + cross * drawDistance + Vector3.UnitZ * drawHeight;
                        Vector3 end = closestPoint - cross * drawDistance + Vector3.UnitZ * drawHeight;
                        
                        Gizmos.DrawLine(new UnityEngine.Vector3(start.X, start.Z, start.Y), new UnityEngine.Vector3(end.X, end.Z, end.Y));
                    }
                }
                
                counter++;
            }
        }
    }
}