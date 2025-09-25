using System;
using System.Collections.Generic;
using System.IO;
using Model.Game.Graph;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Engine.Controller
{
    public class MapEditorGraph<TNode, TCoordinate> : IGraph<TNode, TCoordinate> where TNode : INode<TCoordinate>, INode, new() where TCoordinate : Coordinate, new()
    {
        private Dictionary<TCoordinate, TNode> nodes;
        private TCoordinate size;

        public MapEditorGraph(INode.NodeType[,] nodeTypes)
        {
            size = new TCoordinate();
            size.Set(nodeTypes.GetLength(0), nodeTypes.GetLength(1));
            
            nodes = new Dictionary<TCoordinate, TNode>();

            for (int i = 0; i < size.X; i++)
            {
                for (int j = 0; j < size.Y; j++)
                {
                    TNode node = new();
                    TCoordinate coordinate = new();
            
                    coordinate.Set(i, j);
                    node.SetCoordinate(coordinate);
                    node.SetType(nodeTypes[i, j]);
            
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

        public void SaveGraph()
        {
            INode.NodeType[,] nodeTypes = new INode.NodeType[size.X, size.Y];

            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    TCoordinate coordinate = new();
                    coordinate.Set(x, y);
                    
                    nodeTypes[x, y] = nodes[coordinate].GetNodeType();
                }
            }
            
            MapCreationData.NodeTypes = nodeTypes;
            MapCreationData.Size = size;
        }
    }
}