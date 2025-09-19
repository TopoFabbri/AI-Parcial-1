using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Time;

namespace Model.Tools.Voronoi
{
    internal sealed class BisectorVoronoi : Voronoi<Node<Coordinate>, Coordinate>
    {
        private readonly ConcurrentDictionary<Coordinate, List<VoronoiPlane>> bisectorPlanes = new();
        
        internal BisectorVoronoi(IVoronoiPolicy<Node<Coordinate>, Coordinate> policy) : base(policy)
        {
        }

        internal override void Generate(IGraph<Node<Coordinate>, Coordinate> graph, List<IVoronoiObject<Coordinate>> voronoiObjects)
        {
            Timer timer = new();
            
            bisectorPlanes.Clear();
            ConcurrentBag<Coordinate> sites = new();
            ConcurrentDictionary<VoronoiPlane, Vector3> intersections = new();

            Parallel.ForEach(voronoiObjects, parallelOptions, voronoiObject =>
            {
                sites.Add(voronoiObject.GetCoordinates());
            });

            CalculateBisectorPlanes(sites, intersections);
            RemoveOuterPlanes(intersections);
            
            EventSystem.EventSystem.Raise<DebugEvent>(timer.TimeElapsed.ToString(CultureInfo.InvariantCulture));
        }

        internal override Coordinate GetClosestTo(Coordinate coordinate)
        {
            Vector3 coordAsPoint = new(coordinate.X, coordinate.Y, 0);
            Coordinate closest = new();

            foreach (KeyValuePair<Coordinate, List<VoronoiPlane>> planeGroup in bisectorPlanes)
            {
                bool isOnRegion = true;
                
                foreach (VoronoiPlane plane in planeGroup.Value)
                {
                    if (!plane.GetSide(coordAsPoint))
                        isOnRegion = false;
                }
                
                if (isOnRegion)
                    closest = planeGroup.Key;
            }

            return closest;
        }

        private void RemoveOuterPlanes(ConcurrentDictionary<VoronoiPlane, Vector3> intersections)
        {
            foreach (KeyValuePair<Coordinate, List<VoronoiPlane>> planeGroup in bisectorPlanes)
            {
                List<VoronoiPlane> planesToRemove = new();

                for (int i = 0; i < planeGroup.Value.Count; i++)
                {
                    for (int j = 0; j < planeGroup.Value.Count; j++)
                    {
                        if (i == j)
                            continue;

                        if (!planeGroup.Value[j].GetSide(intersections[planeGroup.Value[i]]))
                            planesToRemove.Add(planeGroup.Value[i]);
                    }
                }

                foreach (VoronoiPlane plane in planesToRemove)
                {
                    bisectorPlanes[planeGroup.Key].Remove(plane); 
                }
            }
        }

        private void CalculateBisectorPlanes(ConcurrentBag<Coordinate> sites, ConcurrentDictionary<VoronoiPlane, Vector3> intersections)
        {
            foreach (Coordinate siteA in sites)
            {
                foreach (Coordinate siteB in sites)
                {
                    if (siteA.Equals(siteB))
                        continue;
                    
                    Vector3 point = new ((siteA.X + siteB.X) / 2f, (siteA.Y + siteB.Y) / 2f, 0);
                    Vector3 normal = Vector3.Normalize(new Vector3(siteA.X - point.X, siteA.Y - point.Y, 0));

                    VoronoiPlane plane = new(normal, point);
                    
                    if (!bisectorPlanes.ContainsKey(siteA))
                        bisectorPlanes.TryAdd(siteA, new List<VoronoiPlane>());
                    
                    bisectorPlanes[siteA].Add(plane);
                    intersections.TryAdd(plane, point);
                }
            }
        }
    }
}