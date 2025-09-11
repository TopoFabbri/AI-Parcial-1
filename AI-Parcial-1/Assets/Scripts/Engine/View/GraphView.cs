using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Game.Graph;
using Model.Tools.Pathfinder.Node;
using UnityEngine;

namespace Engine.View
{
    public class GraphView : MonoBehaviour
    {
        [SerializeField] private Vector2Int graphSize = new(10, 10);
        [SerializeField] private float nodeDistance = 1.0f;
        [SerializeField] private bool circumnavigable;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private float debugDrawSize = .9f;
        
        private const int MaxObjsPerDrawCall = 1000;
        private readonly ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 32 };
        
        private Graph<Node<Coordinate>, Coordinate> graph;
        
        private Mesh prefabMesh;
        private Material prefabMaterial;

        private Vector3 tileScale;

        private void Awake()
        {
            prefabMaterial = tilePrefab.GetComponent<MeshRenderer>().sharedMaterial;
            prefabMesh = tilePrefab.GetComponent<MeshFilter>().sharedMesh;
            tileScale = tilePrefab.transform.localScale * nodeDistance * debugDrawSize;
            
            graph = new Graph<Node<Coordinate>, Coordinate>(graphSize.x, graphSize.y, nodeDistance, circumnavigable);
        }

        private void LateUpdate()
        {
            List<Matrix4x4[]> drawMatrices = new();
            
            int meshes = graph.Nodes.Count;
            List<Coordinate> keys = new(graph.Nodes.Keys);
            
            for (int i = 0; i < meshes; i += MaxObjsPerDrawCall)
            {
                drawMatrices.Add(new Matrix4x4[meshes > MaxObjsPerDrawCall ? MaxObjsPerDrawCall : meshes]);
                meshes -= MaxObjsPerDrawCall;
            }
            
            ParallelLoopResult result = Parallel.For(0, keys.Count, parallelOptions, i =>
            {
                Vector3 position = new(graph.Nodes[keys[i]].GetCoordinate().X, 0f, graph.Nodes[keys[i]].GetCoordinate().Y);
                
                drawMatrices[i / MaxObjsPerDrawCall][i % MaxObjsPerDrawCall].SetTRS(position, Quaternion.identity, tileScale);
            });
            
            foreach (Matrix4x4[] matrixArray in drawMatrices)
            {
                Graphics.DrawMeshInstanced(prefabMesh, 0, prefabMaterial, matrixArray);
            }
        }
    }
}
