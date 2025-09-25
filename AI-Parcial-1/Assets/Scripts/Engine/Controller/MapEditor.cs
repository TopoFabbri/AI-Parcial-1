using System;
using System.Collections.Generic;
using Engine.View;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;
using Sirenix.OdinInspector;
using UnityEngine;

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
        
        private Material tileMaterial;
        private Mesh tileMesh;
        
        private Mode curMode = Mode.MapData;
        
        private MapEditorGraph<Node<Coordinate>, Coordinate> graph;
        
        private Coordinate selectedCoordinate;
        private Camera cam;

        public void OnGrassClicked()
        {
            Node<Coordinate> selectedNode = graph.GetNodeAt(selectedCoordinate);
            
            if (selectedNode == null) return;
            
            selectedNode.SetType(INode.NodeType.Grass);
            nodeTypeMenu.SetActive(false);
            GraphView.OnModifiedGraph(new GraphModifiedEvent());
        }
        
        public void OnRoadClicked()
        {
            Node<Coordinate> selectedNode = graph.GetNodeAt(selectedCoordinate);
            
            if (selectedNode == null) return;
            
            selectedNode.SetType(INode.NodeType.Road);
            nodeTypeMenu.SetActive(false);
            GraphView.OnModifiedGraph(new GraphModifiedEvent());
        }
        
        public void OnWaterClicked()
        {
            Node<Coordinate> selectedNode = graph.GetNodeAt(selectedCoordinate);
            
            if (selectedNode == null) return;
            
            selectedNode.SetType(INode.NodeType.Water);
            nodeTypeMenu.SetActive(false);
            GraphView.OnModifiedGraph(new GraphModifiedEvent());
        }
        
        public void OnSwitchMode(int newMode)
        {
            curMode = (Mode)newMode;
        }
        
        private void Awake()
        {
            cam = Camera.main;
            graph = new MapEditorGraph<Node<Coordinate>, Coordinate>(new Coordinate(5, 5));
            cameraController.PositionCamera(graph);
            
            tileMaterial = tilePrefab.GetComponent<MeshRenderer>().sharedMaterial;
            tileMesh = tilePrefab.GetComponent<MeshFilter>().sharedMesh;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButton(0))
            {
                if (nodeTypeMenu.activeSelf || curMode != Mode.MapEdit) return;

                Vector3 pos = cam.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
                nodeTypeMenu.SetActive(true);
                nodeTypeMenu.transform.position = UnityEngine.Input.mousePosition;
                
                selectedCoordinate = new Coordinate(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
            }
        }

        private void LateUpdate()
        {
            GraphView.DrawGraph(graph, nodeTypeMaterials, tilePrefab.transform.localScale * 0.9f, tileMesh, tileMaterial, GraphView.DrawModes.Content);
        }
    }
    
    public class MapEditorGraph<TNode, TCoordinate> : IGraph<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode, new() where TCoordinate : Coordinate, new()
    {
        private Dictionary<TCoordinate, TNode> nodes;
        private TCoordinate size;

        public MapEditorGraph(TCoordinate size)
        {
            nodes = new Dictionary<TCoordinate, TNode>();
            this.size = size;

            for (int i = 0; i < size.X; i++)
            {
                for (int j = 0; j < size.Y; j++)
                {
                    TNode node = new();
                    TCoordinate coordinate = new();
            
                    coordinate.Set(i, j);
                    node.SetCoordinate(coordinate);
                    node.SetType(INode.NodeType.Road);
            
                    nodes.Add(coordinate, node);
                }
            }
        }
        
        public ICollection<TNode> GetBresenhamNodes(TCoordinate start, TCoordinate end)
        {
            throw new NotImplementedException();
        }

        public ICollection<TNode> GetNodes()
        {
            return nodes.Values;
        }

        public TCoordinate GetSize()
        {
            return size;
        }

        public ICollection<TNode> GetAdjacents(TNode node)
        {
            List<TNode> adjacents = new();

            foreach (ICoordinate adjacentCoordinate in node.GetCoordinate().GetAdjacents())
            {
                if (adjacentCoordinate is not TCoordinate coordinate) continue;

                if (nodes.TryGetValue(coordinate, out TNode adjacentNode))
                    adjacents.Add(adjacentNode);
            }

            return adjacents;
        }

        public ICollection<TCoordinate> GetAdjacents(TCoordinate coordinate)
        {
            List<TCoordinate> adjacents = new();
            
            foreach (TNode node in GetAdjacents(nodes[coordinate]))
                adjacents.Add(node.GetCoordinate());

            return adjacents;
        }

        public float GetNodeDistance()
        {
            return 1f;
        }

        public float GetDistanceBetweenNodes(TNode a, TNode b)
        {
            return GetDistanceBetweenCoordinates(a.GetCoordinate(), b.GetCoordinate());
        }

        public float GetDistanceBetweenCoordinates(TCoordinate a, TCoordinate b)
        {
            return a.GetDistanceTo(b);
        }

        public bool IsCircumnavigable()
        {
            return false;
        }

        public TNode GetNodeAt(TCoordinate coordinate)
        {
            if (!nodes.TryGetValue(coordinate, out TNode node)) return new TNode();
            return node;
        }
    }
}
