using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Game.Graph;
using Model.Game.World.Objects;
using Model.Tools.Pathfinder.Node;
using Model.Tools.Voronoi;
using UnityEngine;

namespace Engine.View
{
    public static class GraphView
    {
        private const int MaxObjsPerDrawCall = 1000;
        private static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = 32 };
        
        // Cache of materials per Voronoi region (per Mine coordinate)
        private static readonly Dictionary<Coordinate, Material> MineMaterials = new();
        
        // Generate a vibrant, deterministic color per mine based on its coordinate
        private static Color GetVibrantColor(Coordinate coord)
        {
            int hash = coord.GetHashCode();
            // Map hash to [0,1]
            float u = (hash & 0x7FFFFFFF) / (float)int.MaxValue;
            // Distribute hues across the wheel and vary saturation/value for contrast
            float h = Mathf.Repeat(u * 0.983f + 0.123f, 1f);
            float s = 0.65f + 0.35f * Mathf.Repeat(u * 3.137f, 1f);
            float v = 0.85f + 0.15f * Mathf.Repeat(u * 5.789f, 1f);
            Color c = Color.HSVToRGB(h, s, v);
            c.a = 1f;
            return c;
        }
        
        private static void EnsureMineMaterials(Material baseMaterial)
        {
            // Create instanced materials only once per mine coordinate
            try
            {
                var mines = Mine.Mines; // static list maintained by Mine
                if (mines == null) return;

                // Step to spread grayscale values (avoid 0 and 1 extremes for visibility)
                int total = mines.Count;
                float step = total > 0 ? 1f / (total + 1) : 1f;

                foreach (var vorObj in mines)
                {
                    Coordinate coord = vorObj.GetCoordinates();
                    if (MineMaterials.ContainsKey(coord)) continue;

                    var mat = new Material(baseMaterial);
                    Color c = GetVibrantColor(coord);
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                    if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
                    if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", c * 0.5f);

                    MineMaterials[coord] = mat;
                }
            }
            catch
            {
                // If anything fails (e.g., no Mine system yet), we silently fall back to single-material rendering in DrawGraph
            }
        }

        public static void DrawGraph(Graph<Node<Coordinate>, Coordinate> graph, Vector3 tileScale, Mesh tileMesh, Material tileMaterial, bool drawVoronoi)
        {
            // If the toggle is off, do the EXACT original behavior: single-material, multithreaded batching, no Voronoi, no material cache.
            if (!drawVoronoi)
            {
                List<Matrix4x4[]> drawMatrices = new();
                
                int meshes = graph.Nodes.Count;
                List<Coordinate> keys = new(graph.Nodes.Keys);
                
                for (int i = 0; i < meshes; i += MaxObjsPerDrawCall)
                {
                    drawMatrices.Add(new Matrix4x4[meshes > MaxObjsPerDrawCall ? MaxObjsPerDrawCall : meshes]);
                    meshes -= MaxObjsPerDrawCall;
                }
                
                Parallel.For(0, keys.Count, ParallelOptions, i =>
                {
                    Vector3 position = new Vector3(graph.Nodes[keys[i]].GetCoordinate().X, 0f, graph.Nodes[keys[i]].GetCoordinate().Y) * graph.GetNodeDistance();
                    
                    drawMatrices[i / MaxObjsPerDrawCall][i % MaxObjsPerDrawCall].SetTRS(position, Quaternion.identity, tileScale);
                });
                
                foreach (Matrix4x4[] matrixArray in drawMatrices)
                {
                    Graphics.DrawMeshInstanced(tileMesh, 0, tileMaterial, matrixArray);
                }
                return;
            }
            
            // Try Voronoi-based grouping by nearest Mine. If unavailable, fall back to original single-material draw.
            bool canUseVoronoi = false;
            try { canUseVoronoi = Mine.Mines is { Count: > 0 }; } catch { canUseVoronoi = false; }

            if (!canUseVoronoi)
            {
                // Fallback: original behavior (single material). This path is only reached when drawVoronoi == true but mines/Voronoi aren't available.
                List<Matrix4x4> matrices = new(graph.Nodes.Count);
                foreach (var key in graph.Nodes.Keys)
                {
                    Vector3 pos = new Vector3(key.X, 0f, key.Y) * graph.GetNodeDistance();
                    matrices.Add(Matrix4x4.TRS(pos, Quaternion.identity, tileScale));
                }

                for (int i = 0; i < matrices.Count; i += MaxObjsPerDrawCall)
                {
                    int count = Math.Min(MaxObjsPerDrawCall, matrices.Count - i);
                    var batch = new Matrix4x4[count];
                    matrices.CopyTo(i, batch, 0, count);
                    Graphics.DrawMeshInstanced(tileMesh, 0, tileMaterial, batch);
                }
                return;
            }

            // Ensure materials exist for current mines
            EnsureMineMaterials(tileMaterial);

            // Group tile transforms by their nearest mine's material
            Dictionary<Material, List<Matrix4x4>> grouped = new();

            foreach (var key in graph.Nodes.Keys)
            {
                Material matToUse = tileMaterial; // default
                try
                {
                    // Find the closest Voronoi seed (mine)
                    Coordinate closest = Voronoi<Node<Coordinate>, Coordinate>.GetClosestTo(typeof(Mine), key);

                    if (!MineMaterials.TryGetValue(closest, out matToUse))
                    {
                        // If missing (e.g., mine added after cache), create on the fly with a unique vibrant color
                        var mat = new Material(tileMaterial);
                        Color c = GetVibrantColor(closest);
                        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                        if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
                        if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", c * 0.5f);
                        MineMaterials[closest] = mat;
                        matToUse = mat;
                    }
                }
                catch
                {
                    // If Voronoi lookup fails for some reason, fall back to base material for this tile.
                    matToUse = tileMaterial;
                }

                if (!grouped.TryGetValue(matToUse, out var list))
                {
                    list = new List<Matrix4x4>();
                    grouped[matToUse] = list;
                }

                Vector3 position = new Vector3(key.X, 0f, key.Y) * graph.GetNodeDistance();
                list.Add(Matrix4x4.TRS(position, Quaternion.identity, tileScale));
            }

            // Draw each group in instanced batches
            foreach (var kv in grouped)
            {
                var material = kv.Key;
                var matrices = kv.Value;
                for (int i = 0; i < matrices.Count; i += MaxObjsPerDrawCall)
                {
                    int count = Math.Min(MaxObjsPerDrawCall, matrices.Count - i);
                    var batch = new Matrix4x4[count];
                    matrices.CopyTo(i, batch, 0, count);
                    Graphics.DrawMeshInstanced(tileMesh, 0, material, batch);
                }
            }
        }

        public static void ClearMaterials()
        {
            MineMaterials.Clear();
        }
    }
}
