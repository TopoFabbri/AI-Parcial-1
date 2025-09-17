using System.Collections.Generic;
using Engine.Controller;
using Engine.View;
using Model.Game.Events;
using Model.Game.Graph;
using Model.Tools.EventSystem;
using Model.Tools.Pathfinder.Node;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Engine
{
    public class EngineManager : SerializedMonoBehaviour
    {
        [FoldoutGroup("Game", true)]
        [TabGroup("Game/Game", "Map"), SerializeField] private Vector2Int mapSize = new(10, 10);
        [TabGroup("Game/Game", "Map"), SerializeField] private bool circumnavigableMap = true;
        [TabGroup("Game/Game", "Map"), SerializeField] private float nodeDistance = 1f;

        [TabGroup("Game/Game", "Mines"), SerializeField] private int mineQty = 5;
        [TabGroup("Game/Game", "Mines"), SerializeField] private int minMineGoldQty = 100;
        [TabGroup("Game/Game", "Mines"), SerializeField] private int maxMineGoldQty = 1000;

        [field: TabGroup("Game/Game", "Miners"), SerializeField] public float MinerSpeed{ get; private set; } = 1f;
        [field: TabGroup("Game/Game", "Miners"), SerializeField] public float MinerMineSpeed{ get; private set; } = 1f;
        [field: TabGroup("Game/Game", "Miners"), SerializeField] public float MinerMaxGold{ get; private set; } = 15f;
        [field: TabGroup("Game/Game", "Miners"), SerializeField] public int MinerMaxFood{ get; private set; } = 500;
        
        [field: TabGroup("Game/Game", "Caravan"), SerializeField] public float CaravanSpeed { get; private set; } = 2f;
        [field: TabGroup("Game/Game", "Caravan"), SerializeField] public int CaravanCapacity { get; private set; } = 10;

        [FoldoutGroup("View", true)]
        [TabGroup("View/View", "Graph"), Required, SerializeField] private GameObject tilePrefab;
        
        [TabGroup("View/View", "Drawing"), SerializeField] private GraphView.DrawModes drawMode;
        [TabGroup("View/View", "Drawing"), SerializeField] private float drawSize = .9f;
        [TabGroup("View/View", "Drawing"), SerializeField] private GameObject defaultPrefab;
        [TabGroup("View/View", "Drawing"), SerializeField] private Dictionary<string, GameObject> prefabs = new();

        [FoldoutGroup("References"), Required, SerializeField] private CameraController cameraController;
        [FoldoutGroup("References"), Required, SerializeField] private TextMeshProUGUI goldTxt;
        [FoldoutGroup("References"), Required, SerializeField] private TextMeshProUGUI alarmTxt;
        
        #region Fields

        private Vector3 tileScale;
        private Mesh tileMesh;
        private Material tileMaterial;

        #endregion

        private Model.Game.Model model;
        private Drawer drawer;
        private Graph<Node<Coordinate>, Coordinate> graph;

        private void Awake()
        {
            EventSystem.Subscribe<DebugEvent>(OnModelDebugEvent);
        }

        private void Start()
        {
            model = new Model.Game.Model();
            drawer = new Drawer(prefabs, defaultPrefab);

            graph = model.CreateGraph(mapSize.x, mapSize.y, mineQty, minMineGoldQty, maxMineGoldQty, nodeDistance, circumnavigableMap);

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
            GraphView.DrawGraph(model.Graph, tileScale, tileMesh, tileMaterial, drawMode);
            drawer.Draw();
            UpdateHudTxt();
        }

        private void OnDestroy()
        {
            model = null;
            drawer = null;
            graph = null;
            
            GraphView.ClearMaterials();
            
            EventSystem.Unsubscribe<DebugEvent>(OnModelDebugEvent);
        }
        
        private static void OnModelDebugEvent(DebugEvent debugEvent)
        {
            Debug.Log(debugEvent.Message);
        }

        private void UpdateHudTxt()
        {
            goldTxt.text = "Gold: " + model.GetCenterGold();
            alarmTxt.text = Model.Game.Model.AlarmRaised ? "CANCEL" : "ALARM";
        }
    }
}