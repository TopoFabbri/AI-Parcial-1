using System.Collections.Generic;
using Engine.Controller;
using Engine.View;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Tools.EventSystem;
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

        #region References

        [TabGroup("Settings", "References"), SerializeField] private CameraController cameraController;

        #endregion
        
        #region Fields

        private Vector3 tileScale;
        private Mesh tileMesh;
        private Material tileMaterial;

        #endregion
        
        private Model.Game.Model model;
        private Drawer drawer;
        private Graph<Node<Coordinate>, Coordinate> graph;
        
        private void Start()
        {
            model = new Model.Game.Model();
            drawer = new Drawer(prefabs, defaultPrefab);
            
            graph = model.CreateGraph(graphSize.x, graphSize.y, nodeDistance, circumnavigableGraph);
            
            tileScale = tilePrefab.transform.localScale * drawSize * nodeDistance;
            tileMesh = tilePrefab.GetComponent<MeshFilter>().sharedMesh;
            tileMaterial = tilePrefab.GetComponent<MeshRenderer>().sharedMaterial;
            
            cameraController.PositionCamera(graph);
        }

        private void Update()
        {
            model.Update();
        }

        private void LateUpdate()
        {
            GraphView.DrawGraph(model.Graph, tileScale, tileMesh, tileMaterial);
            drawer.Draw();
        }

        private void OnCreateMiner()
        {
            EventSystem.Raise<RequestedMinerCreationEvent>();
        }
        
        private void CreateCaravan()
        {
            EventSystem.Raise<RequestedCaravanCreationEvent>();
        }
        
        private void RaiseAlarm()
        {
            EventSystem.Raise<RaiseAlarmEvent>();
        }
    }
}