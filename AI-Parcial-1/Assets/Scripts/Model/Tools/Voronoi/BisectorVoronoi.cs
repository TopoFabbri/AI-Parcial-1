using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Model.Game.Graph;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Tools.Voronoi
{
    internal sealed class BisectorVoronoi : IVoronoi<Node<Coordinate>, Coordinate>
    {
        private IGraph<Node<Coordinate>, Coordinate> graph;
        private readonly Dictionary<Coordinate, List<VoronoiPlane>> bisectorPlanes = new();
        
        public ParallelOptions ParallelOptions { get; } = new() { MaxDegreeOfParallelism = 32 };

        internal BisectorVoronoi(IGraph<Node<Coordinate>, Coordinate> graph)
        {
            this.graph = graph;
        }

        public void Generate(List<IVoronoiObject<Coordinate>> voronoiObjects)
        {
            bisectorPlanes.Clear();
            List<Coordinate> sites = new();
            Dictionary<VoronoiPlane, Vector3> intersections = new();

            foreach (IVoronoiObject<Coordinate> voronoiObject in voronoiObjects)
            {
                sites.Add(voronoiObject.GetCoordinates());
            }

            CalculateBisectorPlanes(sites, intersections);
            RemoveOuterPlanes(intersections);
        }

        public Coordinate GetClosestTo(Coordinate coordinate)
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

        public Dictionary<Coordinate, List<VoronoiPlane>> GetPlanes()
        {
            return bisectorPlanes;
        }
        
        private void RemoveOuterPlanes(Dictionary<VoronoiPlane, Vector3> intersections)
        {
            foreach (KeyValuePair<Coordinate, List<VoronoiPlane>> planeGroup in bisectorPlanes)
            {
                List<VoronoiPlane> planesToRemove = new();
                Vector3 coordAsVector = new(planeGroup.Key.X, planeGroup.Key.Y, 0);

                foreach (VoronoiPlane voronoiPlane in planeGroup.Value)
                    voronoiPlane.distanceFromNode = Vector3.Distance(intersections[voronoiPlane], coordAsVector);

                planeGroup.Value.Sort((a, b) => a.distanceFromNode.CompareTo(b.distanceFromNode));
                
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

        private void CalculateBisectorPlanes(List<Coordinate> sites, Dictionary<VoronoiPlane, Vector3> intersections)
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