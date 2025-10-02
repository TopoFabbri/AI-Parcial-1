using System;
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
        private readonly IGraph<Node<Coordinate>, Coordinate> graph;
        private readonly Dictionary<Coordinate, List<VoronoiPlane>> bisectorPlanes = new();
        private readonly Dictionary<Coordinate, List<VoronoiPlane>> altBisectorPlanes = new();

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
            // RemoveOuterPlanes(intersections);
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
                    if (plane.GetSide(coordAsPoint)) continue;
                    
                    isOnRegion = false;
                    break;
                }

                if (isOnRegion)
                    return planeGroup.Key;
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
                Vector3 coordAsVector = new(planeGroup.Key.X, planeGroup.Key.Y, 0);

                planeGroup.Value.Sort((planeA, planeB) =>
                {
                    float distanceA = Vector3.Distance(coordAsVector, intersections[planeA]);
                    float distanceB = Vector3.Distance(coordAsVector, intersections[planeB]);
                    return distanceA.CompareTo(distanceB);
                });

                List<VoronoiPlane> insidePlanes = new() { planeGroup.Value[0] };

                foreach (VoronoiPlane planeA in planeGroup.Value)
                {
                    bool isInside = true;
                    
                    foreach (VoronoiPlane planeB in insidePlanes)
                    {
                        if (planeA == planeB)
                            continue;

                        if (planeA.GetSide(intersections[planeB])) continue;
                        
                        isInside = false;
                        break;
                    }
                    
                    if (isInside)
                        insidePlanes.Add(planeA);
                }

                planeGroup.Value.Clear();
                
                foreach (VoronoiPlane plane in insidePlanes)
                    planeGroup.Value.Add(plane);
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

                    Vector3 point = new((siteA.X + siteB.X) / 2f, (siteA.Y + siteB.Y) / 2f, 0);
                    Vector3 normal = Vector3.Normalize(new Vector3(siteA.X - point.X, siteA.Y - point.Y, 0));

                    float aDistanceToEdgeX = normal.X > 0f ? graph.GetSize().X - siteA.X + .5f : -(siteA.X + .5f);
                    float aDistanceToEdgeY = normal.Y > 0f ? graph.GetSize().Y - siteA.Y + .5f : -(siteA.Y + .5f);

                    float bDistanceToEdgeX = normal.X < 0f ? graph.GetSize().X - siteB.X + .5f : -(siteB.X + .5f);
                    float bDistanceToEdgeY = normal.Y < 0f ? graph.GetSize().Y - siteB.Y + .5f : -(siteB.Y + .5f);

                    Vector3 aDistanceToEdgeVector = Math.Abs(aDistanceToEdgeX) / Math.Abs(normal.X) < Math.Abs(aDistanceToEdgeY) / Math.Abs(normal.Y)
                        ? new Vector3(aDistanceToEdgeX, normal.Y * (Math.Abs(aDistanceToEdgeX) / Math.Abs(normal.X)), 0)
                        : new Vector3(normal.X * (Math.Abs(aDistanceToEdgeY) / Math.Abs(normal.Y)), aDistanceToEdgeY, 0);
                    
                    Vector3 bDistanceToEdgeVector = Math.Abs(bDistanceToEdgeX) / Math.Abs(normal.X) < Math.Abs(bDistanceToEdgeY) / Math.Abs(normal.Y)
                        ? new Vector3(bDistanceToEdgeX, -normal.Y * (Math.Abs(bDistanceToEdgeX) / Math.Abs(normal.X)), 0)
                        : new Vector3(-normal.X * (Math.Abs(bDistanceToEdgeY) / Math.Abs(normal.Y)), bDistanceToEdgeY, 0);

                    Vector3 altPoint = new Vector3(siteB.X, siteB.Y, 0) + (bDistanceToEdgeVector - aDistanceToEdgeVector) / 2f;
                    
                    if (altPoint.X < -.5f || altPoint.X > graph.GetSize().X + .5f || altPoint.Y < -.5f || altPoint.Y > graph.GetSize().Y + .5f)
                        altPoint = new Vector3(siteA.X, siteA.Y, 0) + (aDistanceToEdgeVector - bDistanceToEdgeVector) / 2f;
                    
                    VoronoiPlane plane = new(normal, point, altPoint);

                    if (!bisectorPlanes.ContainsKey(siteA))
                        bisectorPlanes.TryAdd(siteA, new List<VoronoiPlane>());

                    bisectorPlanes[siteA].Add(plane);
                    intersections.TryAdd(plane, point);
                }
            }
        }
    }
}