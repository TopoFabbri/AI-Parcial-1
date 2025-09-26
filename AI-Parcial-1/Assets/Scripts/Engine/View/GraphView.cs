using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Voronoi;
using UnityEngine;

namespace Engine.View
{
    public static class GraphView
    {
        public enum DrawModes
        {
            None,
            Default,
            Voronoi,
            Content
        }

        private const int MaxObjsPerDrawCall = 1000;
        private static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = 32 };

        private static readonly int BaseColorPropertyID = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorPropertyID = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorPropertyID = Shader.PropertyToID("_EmissionColor");

        private static readonly Dictionary<DrawModes, ConcurrentDictionary<Material, List<Matrix4x4[]>>> DrawMatrices = new();
        private static readonly Dictionary<Material, List<Coordinate>> GroupedCoords = new();
        private static readonly Dictionary<Coordinate, Material> MineMaterials = new();

        public static void OnModifiedGraph(GraphModifiedEvent graphModifiedEvent)
        {
            DrawMatrices.Clear();
            GroupedCoords.Clear();
            MineMaterials.Clear();
        }

        public static void DrawGraph(IGraph<Node<Coordinate>, Coordinate> graph, Dictionary<INode.NodeType, Material> nodeTypeMaterials, Vector3 tileScale, Mesh tileMesh,
            Material tileMaterial, DrawModes drawMode)
        {
            if (drawMode == DrawModes.None) return;
            if (graph == null || !tileMesh || !tileMaterial) return;

            float nodeDistance = graph.GetNodeDistance();

            if (Mine.Mines.Count <= 0 && drawMode == DrawModes.Voronoi) drawMode = DrawModes.Default;

            if (drawMode == DrawModes.Default)
            {
                DrawAsDefault(graph, tileScale, tileMesh, tileMaterial, nodeDistance);
                return;
            }

            if (drawMode == DrawModes.Voronoi)
            {
                EnsureMineMaterials(tileMaterial, graph);

                DrawAsVoronoi(graph, tileScale, tileMesh, nodeDistance);
            }

            if (drawMode == DrawModes.Content)
                DrawAsContent(graph, nodeTypeMaterials, tileScale, tileMesh, nodeDistance);
        }

        private static void DrawAsDefault(IGraph<Node<Coordinate>, Coordinate> graph, Vector3 tileScale, Mesh tileMesh, Material tileMaterial, float nodeDistance)
        {
            if (!DrawMatrices.ContainsKey(DrawModes.Default))
                DrawMatrices[DrawModes.Default] = new ConcurrentDictionary<Material, List<Matrix4x4[]>>();

            List<Coordinate> graphCoords = new();

            foreach (Node<Coordinate> node in graph.GetNodes())
                graphCoords.Add(node.GetCoordinate());

            if (!DrawMatrices[DrawModes.Default].ContainsKey(tileMaterial) || DrawMatrices[DrawModes.Default][tileMaterial] == null ||
                DrawMatrices[DrawModes.Default][tileMaterial].Count <= 0)
                DrawMatrices[DrawModes.Default][tileMaterial] = BuildMatrices(new List<Coordinate>(graphCoords), nodeDistance, tileScale);

            DrawMeshInstances(tileMesh, tileMaterial, DrawMatrices[DrawModes.Default][tileMaterial]);
        }

        private static void DrawAsVoronoi(IGraph<Node<Coordinate>, Coordinate> graph, Vector3 tileScale, Mesh tileMesh, float nodeDistance)
        {
            if (!DrawMatrices.ContainsKey(DrawModes.Voronoi))
                DrawMatrices[DrawModes.Voronoi] = new ConcurrentDictionary<Material, List<Matrix4x4[]>>();

            List<Coordinate> graphCoords = new();

            foreach (Node<Coordinate> node in graph.GetNodes())
                graphCoords.Add(node.GetCoordinate());

            if (GroupedCoords.Count == 0)
            {
                foreach (Coordinate coordinate in graphCoords)
                {
                    Coordinate mineCoord = VoronoiRegistry<Node<Coordinate>, Coordinate>.GetClosestTo(typeof(Mine), coordinate);
                    if (!GroupedCoords.TryGetValue(MineMaterials[mineCoord], out List<Coordinate> coordGroup))
                    {
                        coordGroup = new List<Coordinate>();
                        GroupedCoords[MineMaterials[mineCoord]] = coordGroup;
                    }

                    coordGroup.Add(coordinate);
                }
            }

            if (DrawMatrices[DrawModes.Voronoi].Count != GroupedCoords.Count)
            {
                Parallel.ForEach(GroupedCoords, ParallelOptions,
                    coordGroup => { DrawMatrices[DrawModes.Voronoi].TryAdd(coordGroup.Key, BuildMatrices(coordGroup.Value, nodeDistance, tileScale)); });
            }

            foreach (KeyValuePair<Material, List<Matrix4x4[]>> matGroup in DrawMatrices[DrawModes.Voronoi])
                DrawMeshInstances(tileMesh, matGroup.Key, matGroup.Value);
        }

        private static void DrawAsContent(IGraph<Node<Coordinate>, Coordinate> graph, Dictionary<INode.NodeType, Material> nodeTypeMaterials, Vector3 tileScale, Mesh tileMesh,
            float nodeDistance)
        {
            if (!DrawMatrices.ContainsKey(DrawModes.Content))
                DrawMatrices[DrawModes.Content] = new ConcurrentDictionary<Material, List<Matrix4x4[]>>();

            List<Coordinate> graphCoords = new();
            
            foreach (Node<Coordinate> node in graph.GetNodes())
                graphCoords.Add(node.GetCoordinate());

            if (DrawMatrices[DrawModes.Content].Count <= 0)
            {
                ConcurrentDictionary<Material, List<Coordinate>> mineCoords = new();

                foreach (Coordinate coordinate in graphCoords)
                {
                    if (!nodeTypeMaterials.TryGetValue(graph.GetNodeAt(coordinate).GetNodeType(), out Material material)) return;

                    if (!mineCoords.ContainsKey(material))
                        mineCoords[material] = new List<Coordinate>();

                    mineCoords[material].Add(coordinate);
                }

                Parallel.ForEach(mineCoords, ParallelOptions,
                    coordGroup => { DrawMatrices[DrawModes.Content].TryAdd(coordGroup.Key, BuildMatrices(coordGroup.Value, nodeDistance, tileScale)); });
            }

            foreach (KeyValuePair<Material, List<Matrix4x4[]>> matGroup in DrawMatrices[DrawModes.Content])
                DrawMeshInstances(tileMesh, matGroup.Key, matGroup.Value);
        }

        public static void ClearMaterials()
        {
            MineMaterials.Clear();
        }

        private static List<Matrix4x4[]> BuildMatrices(List<Coordinate> coordinates, float nodeDistance, Vector3 scale)
        {
            List<Matrix4x4[]> matrices = new();
            int meshCount = coordinates.Count;

            for (int i = 0; i < meshCount; i += MaxObjsPerDrawCall)
            {
                matrices.Add(new Matrix4x4[meshCount > MaxObjsPerDrawCall ? MaxObjsPerDrawCall : meshCount]);
            }

            Parallel.For(0, coordinates.Count, ParallelOptions, i =>
            {
                Coordinate coordinate = coordinates[i];

                Vector3 pos = new Vector3(coordinate.X, 0f, coordinate.Y) * nodeDistance;
                matrices[i / MaxObjsPerDrawCall][i % MaxObjsPerDrawCall] = Matrix4x4.TRS(pos, Quaternion.identity, scale);
            });

            return matrices;
        }

        private static void DrawMeshInstances(Mesh mesh, Material material, List<Matrix4x4[]> matrices)
        {
            foreach (Matrix4x4[] matrixArray in matrices)
                Graphics.DrawMeshInstanced(mesh, 0, material, matrixArray);
        }

        private static void EnsureMineMaterials(Material baseMaterial, IGraph<Node<Coordinate>, Coordinate> graph)
        {
            List<IVoronoiObject<Coordinate>> mines = Mine.Mines;
            if (mines == null || mines.Count == 0) return;

            // Collect site coordinates
            List<Coordinate> sites = new();
            foreach (IVoronoiObject<Coordinate> vorObj in mines)
                sites.Add(vorObj.GetCoordinates());

            // Initialize adjacency map
            Dictionary<Coordinate, HashSet<Coordinate>> adjacency = new();
            foreach (Coordinate s in sites)
                adjacency[s] = new HashSet<Coordinate>();

            // Build adjacency by checking neighboring tiles that belong to different sites
            foreach (Node<Coordinate> node in graph.GetNodes())
            {
                Coordinate p = node.GetCoordinate();
                Coordinate siteA = VoronoiRegistry<Node<Coordinate>, Coordinate>.GetClosestTo(typeof(Mine), p);
                foreach (Coordinate n in graph.GetAdjacents(p))
                {
                    Coordinate siteB = VoronoiRegistry<Node<Coordinate>, Coordinate>.GetClosestTo(typeof(Mine), n);
                    if (!siteA.Equals(siteB))
                    {
                        adjacency[siteA].Add(siteB);
                        adjacency[siteB].Add(siteA);
                    }
                }
            }

            // Greedy 4-coloring (sufficient for planar graphs like Voronoi adjacencies)
            Dictionary<Coordinate, int> colorIndex = new();
            // Order sites by descending degree to improve greedy coloring success
            sites.Sort((a, b) => adjacency[b].Count.CompareTo(adjacency[a].Count));
            foreach (Coordinate s in sites)
            {
                bool[] used = new bool[4];
                foreach (Coordinate nei in adjacency[s])
                {
                    if (colorIndex.TryGetValue(nei, out int idx) && idx >= 0 && idx < 4)
                        used[idx] = true;
                }
                int chosen = 0;
                while (chosen < 4 && used[chosen]) chosen++;
                if (chosen >= 4) chosen = 0; // Fallback (should rarely/never happen)
                colorIndex[s] = chosen;
            }

            Color[] palette = { Color.red, Color.green, Color.blue, Color.yellow };
            foreach (Coordinate s in sites)
            {
                if (!MineMaterials.ContainsKey(s))
                {
                    Color c = palette[colorIndex[s] % 4];
                    MineMaterials[s] = CreateColoredMaterial(baseMaterial, c);
                }
            }
        }

        private static Material CreateColoredMaterial(Material baseMaterial, Color c)
        {
            Material mat = new(baseMaterial);
            if (mat.HasProperty(BaseColorPropertyID)) mat.SetColor(BaseColorPropertyID, c);
            if (mat.HasProperty(ColorPropertyID)) mat.SetColor(ColorPropertyID, c);
            if (mat.HasProperty(EmissionColorPropertyID)) mat.SetColor(EmissionColorPropertyID, c * 0.5f);
            return mat;
        }
    }
}