using System.Collections.Generic;
using Engine.View;
using Model.Game.Graph;
using Model.Game.World.Agents;
using Model.Tools.Pathfinder.Node;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Engine
{
    public class EngineManager : SerializedMonoBehaviour
    {
        #region Graph

        [TabGroup("Settings", "Graph"), SerializeField] private Vector2Int graphSize = new(10, 10);
        [TabGroup("Settings", "Graph"), SerializeField] private bool circumnavigableGraph = true;
        [TabGroup("Settings", "Graph"), SerializeField] private float nodeDistance = 1f;
        [TabGroup("Settings", "Graph"), Required, SerializeField] private GameObject tilePrefab;

        #endregion

        #region Drawing

        [TabGroup("Settings", "Drawing"), SerializeField] private float drawSize = .9f;
        [TabGroup("Settings", "Drawing"), SerializeField] private GameObject defaultPrefab;
        [TabGroup("Settings", "Drawing"), SerializeField] private Dictionary<string, GameObject> prefabs = new();

        #endregion

        #region Fields

        private Vector3 tileScale;
        private Mesh tileMesh;
        private Material tileMaterial;

        #endregion
        
        private Model.Game.Model model;
        private Drawer drawer;
        private Graph<Node<Coordinate>, Coordinate> graph;

        private Miner testMiner;
        
        private void Start()
        {
            model = new Model.Game.Model();
            drawer = new Drawer(prefabs, defaultPrefab);
            
            graph = model.CreateGraph(graphSize.x, graphSize.y, nodeDistance, circumnavigableGraph);
            
            testMiner = new Miner(graph.GetNodeAtIndexes(graphSize.x / 2, graphSize.y / 2));
            
            tileScale = tilePrefab.transform.localScale * (drawSize * nodeDistance);
            tileMesh = tilePrefab.GetComponent<MeshFilter>().sharedMesh;
            tileMaterial = tilePrefab.GetComponent<MeshRenderer>().sharedMaterial;
        }

        private void Update()
        {
            testMiner.Update();
        }

        private void LateUpdate()
        {
            GraphView.DrawGraph(model.Graph, tileScale, tileMesh, tileMaterial);
            drawer.Draw();
        }
    }
}