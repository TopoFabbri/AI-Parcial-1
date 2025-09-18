using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Game.World.Objects;
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

        public static void DrawGraph(Graph<Node<Coordinate>, Coordinate> graph, Dictionary<INode.NodeType, Material> nodeTypeMaterials, Vector3 tileScale, Mesh tileMesh,
            Material tileMaterial, DrawModes drawMode)
        {
            if (drawMode == DrawModes.None) return;
            if (graph == null || !tileMesh || !tileMaterial) return;

            float nodeDistance = graph.GetNodeDistance();

            if (Mine.Mines.Count <= 0) drawMode = DrawModes.Default;

            if (drawMode == DrawModes.Default)
            {
                DrawAsDefault(graph, tileScale, tileMesh, tileMaterial, nodeDistance);
                return;
            }

            if (drawMode == DrawModes.Voronoi)
            {
                EnsureMineMaterials(tileMaterial);

                DrawAsVoronoi(graph, tileScale, tileMesh, nodeDistance);
            }
            
            if (drawMode == DrawModes.Content)
                DrawAsContent(graph, nodeTypeMaterials, tileScale, tileMesh, nodeDistance);
        }

        private static void DrawAsDefault(Graph<Node<Coordinate>, Coordinate> graph, Vector3 tileScale, Mesh tileMesh, Material tileMaterial, float nodeDistance)
        {
            if (!DrawMatrices.ContainsKey(DrawModes.Default))
                DrawMatrices[DrawModes.Default] = new ConcurrentDictionary<Material, List<Matrix4x4[]>>();

            if (!DrawMatrices[DrawModes.Default].ContainsKey(tileMaterial) || DrawMatrices[DrawModes.Default][tileMaterial] == null ||
                DrawMatrices[DrawModes.Default][tileMaterial].Count <= 0)
                DrawMatrices[DrawModes.Default][tileMaterial] = BuildMatrices(new List<Coordinate>(graph.Nodes.Keys), nodeDistance, tileScale);

            DrawMeshInstances(tileMesh, tileMaterial, DrawMatrices[DrawModes.Default][tileMaterial]);
        }

        private static void DrawAsVoronoi(Graph<Node<Coordinate>, Coordinate> graph, Vector3 tileScale, Mesh tileMesh, float nodeDistance)
        {
            if (!DrawMatrices.ContainsKey(DrawModes.Voronoi))
                DrawMatrices[DrawModes.Voronoi] = new ConcurrentDictionary<Material, List<Matrix4x4[]>>();

            if (GroupedCoords.Count == 0)
            {
                foreach (Coordinate coordinate in graph.Nodes.Keys)
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

        private static void DrawAsContent(Graph<Node<Coordinate>, Coordinate> graph, Dictionary<INode.NodeType, Material> nodeTypeMaterials, Vector3 tileScale, Mesh tileMesh, float nodeDistance)
        {
            if (!DrawMatrices.ContainsKey(DrawModes.Content))
                DrawMatrices[DrawModes.Content] = new ConcurrentDictionary<Material, List<Matrix4x4[]>>();

            if (DrawMatrices[DrawModes.Content].Count <= 0)
            {
                ConcurrentDictionary<Material, List<Coordinate>> mineCoords = new();

                foreach (Coordinate coordinate in graph.Nodes.Keys)
                {
                    if (!nodeTypeMaterials.TryGetValue(graph.Nodes[coordinate].GetNodeType(), out Material material)) return;
                
                    if (!mineCoords.ContainsKey(material))
                        mineCoords[material] = new List<Coordinate>();

                    mineCoords[material].Add(coordinate);
                }
            
                Parallel.ForEach(mineCoords, ParallelOptions, coordGroup =>
                {
                    DrawMatrices[DrawModes.Content].TryAdd(coordGroup.Key, BuildMatrices(coordGroup.Value, nodeDistance, tileScale));
                });
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

        private static void EnsureMineMaterials(Material baseMaterial)
        {
            List<IVoronoiObject<Coordinate>> mines = Mine.Mines;
            if (mines == null) return;

            foreach (IVoronoiObject<Coordinate> vorObj in mines)
            {
                Coordinate coord = vorObj.GetCoordinates();

                if (!MineMaterials.ContainsKey(coord))
                    MineMaterials[coord] = CreateColoredMaterial(baseMaterial, coord);
            }
        }

        private static Material CreateColoredMaterial(Material baseMaterial, Coordinate coord)
        {
            Material mat = new(baseMaterial);
            Color c = GetVibrantColor(coord);
            if (mat.HasProperty(BaseColorPropertyID)) mat.SetColor(BaseColorPropertyID, c);
            if (mat.HasProperty(ColorPropertyID)) mat.SetColor(ColorPropertyID, c);
            if (mat.HasProperty(EmissionColorPropertyID)) mat.SetColor(EmissionColorPropertyID, c * 0.5f);
            return mat;
        }

        private static Color GetVibrantColor(Coordinate coord)
        {
            int hash = coord.GetHashCode() & 0x7FFFFFFF;
            float u = hash / (float)int.MaxValue;
            float h = Mathf.Repeat(u * 0.983f + 0.123f, 1f);
            float s = 0.65f + 0.35f * Mathf.Repeat(u * 3.137f, 1f);
            float v = 0.85f + 0.15f * Mathf.Repeat(u * 5.789f, 1f);
            Color c = Color.HSVToRGB(h, s, v);
            c.a = 1f;
            return c;
        }
    }
}