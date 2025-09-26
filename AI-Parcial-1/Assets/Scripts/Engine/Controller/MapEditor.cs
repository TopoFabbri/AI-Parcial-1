using System;
using System.Collections.Generic;
using Engine.View;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Tools.EventSystem;
using Model.Tools.Pathfinder.Node;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Engine.Controller
{
    public class MapEditor : SerializedMonoBehaviour
    {
        private enum Mode
        {
            MapData,
            MapEdit
        }
        
        [Required, SerializeField] private Dictionary<INode.NodeType, Material> nodeTypeMaterials = new();
        [Required, SerializeField] private GameObject tilePrefab;
        [Required, SerializeField] private CameraController cameraController;
        [Required, SerializeField] private GameObject nodeTypeMenu;
        [Required, SerializeField] private Slider widthSlider;
        [Required, SerializeField] private Slider heightSlider;
        [SerializeField] private TextMeshProUGUI widthTxt;
        [SerializeField] private TextMeshProUGUI heightTxt;
        
        private Material tileMaterial;
        private Mesh tileMesh;
        
        private Mode curMode = Mode.MapData;
        
        private MapEditorGraph<Node<Coordinate>, Coordinate> graph;
        
        private Coordinate selectedCoordinate;
        private Camera cam;

        public void OnGrassClicked()
        {
            Node<Coordinate> selectedNode = graph.GetNodeAt(selectedCoordinate);
            nodeTypeMenu.SetActive(false);
            
            if (selectedNode == null) return;
            
            selectedNode.SetType(INode.NodeType.Grass);
            
            EventSystem.Raise<GraphModifiedEvent>(new GraphModifiedEvent());
        }
        
        public void OnRoadClicked()
        {
            Node<Coordinate> selectedNode = graph.GetNodeAt(selectedCoordinate);
            nodeTypeMenu.SetActive(false);

            if (selectedNode == null) return;
            
            selectedNode.SetType(INode.NodeType.Road);
            
            EventSystem.Raise<GraphModifiedEvent>(new GraphModifiedEvent());
        }
        
        public void OnWaterClicked()
        {
            Node<Coordinate> selectedNode = graph.GetNodeAt(selectedCoordinate);
            nodeTypeMenu.SetActive(false);

            if (selectedNode == null) return;
            
            selectedNode.SetType(INode.NodeType.Water);
            
            EventSystem.Raise<GraphModifiedEvent>(new GraphModifiedEvent());
        }
        
        public void OnSwitchMode(int newMode)
        {
            curMode = (Mode)newMode;
            
            if (curMode == Mode.MapData)
                graph.SaveGraph();
        }
        
        public void OnMapWidthChanged(float value)
        {
            MapCreationData.Size = new Coordinate(Mathf.RoundToInt(value), MapCreationData.Size.Y);
            
            CreateGraph();

            if (widthTxt)
                widthTxt.text = MapCreationData.Size.X.ToString();
        }
        
        public void OnMapHeightChanged(float value)
        {
            MapCreationData.Size = new Coordinate(MapCreationData.Size.X, Mathf.RoundToInt(value));

            CreateGraph();

            if (heightTxt)
                heightTxt.text = MapCreationData.Size.Y.ToString();
        }
        
        private void Awake()
        {
            EventSystem.Subscribe<GraphModifiedEvent>(GraphView.OnModifiedGraph);

            cam = Camera.main;
            CreateGraph();
            cameraController.PositionCamera(graph);
            
            tileMaterial = tilePrefab.GetComponent<MeshRenderer>().sharedMaterial;
            tileMesh = tilePrefab.GetComponent<MeshFilter>().sharedMesh;
        }

        private void OnDestroy()
        {
            EventSystem.Unsubscribe<GraphModifiedEvent>(GraphView.OnModifiedGraph);
        }

        private void Start()
        {
            widthSlider.value = MapCreationData.Size.X;
            heightSlider.value = MapCreationData.Size.Y;
            
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButton(0))
            {
                if (nodeTypeMenu.activeSelf || curMode != Mode.MapEdit) return;

                Vector3 pos = cam.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                selectedCoordinate = new Coordinate(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
                
                if (graph.GetNodeAt(selectedCoordinate).GetCoordinate() == null) return;
                
                nodeTypeMenu.SetActive(true);
                nodeTypeMenu.transform.position = UnityEngine.Input.mousePosition;
            }
        }

        private void LateUpdate()
        {
            GraphView.DrawGraph(graph, nodeTypeMaterials, tilePrefab.transform.localScale * 0.9f, tileMesh, tileMaterial, GraphView.DrawModes.Content);
        }

        private void CreateGraph()
        {
            graph = new MapEditorGraph<Node<Coordinate>, Coordinate>(MapCreationData.NodeTypes);
            EventSystem.Raise<GraphModifiedEvent>(new GraphModifiedEvent());
        }
    }
}
