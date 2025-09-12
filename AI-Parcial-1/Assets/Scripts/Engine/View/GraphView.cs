using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Game.Graph;
using Model.Tools.Pathfinder.Node;
using UnityEngine;

namespace Engine.View
{
    public static class GraphView
    {
        private const int MaxObjsPerDrawCall = 1000;
        private static readonly ParallelOptions ParallelOptions = new() { MaxDegreeOfParallelism = 32 };

        public static void DrawGraph(Graph<Node<Coordinate>, Coordinate> graph, Vector3 tileScale, Mesh tileMesh, Material tileMaterial)
        {
            List<Matrix4x4[]> drawMatrices = new();
            
            int meshes = graph.Nodes.Count;
            List<Coordinate> keys = new(graph.Nodes.Keys);
            
            for (int i = 0; i < meshes; i += MaxObjsPerDrawCall)
            {
                drawMatrices.Add(new Matrix4x4[meshes > MaxObjsPerDrawCall ? MaxObjsPerDrawCall : meshes]);
                meshes -= MaxObjsPerDrawCall;
            }
            
            ParallelLoopResult result = Parallel.For(0, keys.Count, ParallelOptions, i =>
            {
                Vector3 position = new Vector3(graph.Nodes[keys[i]].GetCoordinate().X, 0f, graph.Nodes[keys[i]].GetCoordinate().Y) * graph.GetNodeDistance();
                
                drawMatrices[i / MaxObjsPerDrawCall][i % MaxObjsPerDrawCall].SetTRS(position, Quaternion.identity, tileScale);
            });
            
            foreach (Matrix4x4[] matrixArray in drawMatrices)
            {
                Graphics.DrawMeshInstanced(tileMesh, 0, tileMaterial, matrixArray);
            }
        }
    }
}
