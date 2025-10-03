using System.Collections.Generic;
using System.IO;
using Model.Game.Graph;
using UnityEngine;
using NodeType = Model.Tools.Pathfinder.Node.INode.NodeType;

namespace Engine.Controller
{
    public static class MapCreationData
    {
        public static float NodeDistance { get; set; } = 1f;
        public static int MineQty { get; set; } = 5;

        public static Coordinate Size
        {
            get => new(NodeTypes.GetLength(0), NodeTypes.GetLength(1));
            set
            {
                if (value.X == NodeTypes.GetLength(0) && value.Y == NodeTypes.GetLength(1)) return;
                
                NodeType[,] nodeTypes = new NodeType[value.X, value.Y];
                    
                for (int x = 0; x < value.X; x++)
                {
                    for (int y = 0; y < value.Y; y++)
                    {
                        if (x < NodeTypes.GetLength(0) && y < NodeTypes.GetLength(1))
                            nodeTypes[x, y] = NodeTypes[x, y];
                        else
                            nodeTypes[x, y] = NodeType.Road; 
                    }
                }
                    
                _nodeTypes = nodeTypes;
            }
        }

        public static NodeType[,] NodeTypes
        {
            get
            {
                _nodeTypes ??= LoadMapData();

                return _nodeTypes;
            }
            set => _nodeTypes = value;
        }

        private static NodeType[,] _nodeTypes;

        private static NodeType[,] LoadMapData()
        {
            string mapCsv;
            TextAsset csvAsset = Resources.Load<TextAsset>("Maps/Map");

            if (csvAsset)
                mapCsv = csvAsset.text;
            else
                throw new FileNotFoundException("Map CSV not found. Ensure it exists at Resources/Maps/Map.csv.");

            List<List<NodeType>> mapColumns = new();

            int x = 0;
            int y = 0;

            int minSizeX = mapCsv.Length;

            foreach (char c in mapCsv)
            {
                if (mapColumns.Count <= x)
                    mapColumns.Add(new List<NodeType>());

                if (c == '\n')
                {
                    minSizeX = Mathf.Min(minSizeX, x + 1);
                    x = 0;
                    y++;
                    continue;
                }

                if (c == ',')
                {
                    x++;
                    continue;
                }

                if (c == '0')
                    mapColumns[x].Add(NodeType.Grass);
                else if (c == '1')
                    mapColumns[x].Add(NodeType.Road);
                else if (c == '2')
                    mapColumns[x].Add(NodeType.Water);
            }

            foreach (List<NodeType> mapColumn in mapColumns)
                mapColumn.Reverse();

            NodeType[,] nodeTypes = new NodeType[minSizeX, y];

            for (int i = 0; i < minSizeX; i++)
            {
                for (int j = 0; j < y; j++)
                    nodeTypes[i, j] = mapColumns[i][j];
            }

            return nodeTypes;
        }
    }
}