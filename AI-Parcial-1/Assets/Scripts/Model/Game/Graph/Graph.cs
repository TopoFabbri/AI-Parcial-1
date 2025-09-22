using System;
using System.Collections.Generic;
using System.Linq;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.Graph
{
    public class Graph<TNode, TCoordinate> : IGraph<TNode, TCoordinate> where TNode : Node<TCoordinate>, new() where TCoordinate : Coordinate, new()
    {
        private readonly TCoordinate size;

        private readonly bool circumnavigable;

        public Dictionary<TCoordinate, TNode> Nodes { get; } = new();

        private readonly float nodeDistance;

        public Graph(int x, int y, float nodeDistance = 1f, bool circumnavigable = false)
        {
            this.circumnavigable = circumnavigable;
            this.nodeDistance = nodeDistance;

            size = new TCoordinate();
            size.Set(x, y);

            Random random = new();
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    TCoordinate coordinate = new();
                    TNode node = new();

                    coordinate.Set(i, j);

                    node.SetCoordinate(coordinate);
                    node.SetType((INode.NodeType)random.Next(0, typeof(INode.NodeType).GetEnumValues().Length));
                    Nodes.Add(coordinate, node);
                }
            }
        }

        public Graph(INode.NodeType[,] nodeTypes, float nodeDistance = 1f, bool circumnavigable = false)
        {
            this.circumnavigable = circumnavigable;
            this.nodeDistance = nodeDistance;
            
            size = new TCoordinate();
            size.Set(nodeTypes.GetLength(0), nodeTypes.GetLength(1));
            
            for (int i = 0; i < size.X; i++)
            {
                for (int j = 0; j < size.Y; j++)
                {
                    TCoordinate coordinate = new();
                    TNode node = new();

                    coordinate.Set(i, size.Y - 1 - j);

                    node.SetCoordinate(coordinate);
                    node.SetType(nodeTypes[i, j]);
                    Nodes.Add(coordinate, node);
                }
            }
        }

        ~Graph()
        {
            Nodes.Clear();
        }

        public ICollection<TNode> GetAdjacents(TNode node)
        {
            List<TNode> adjacents = new();

            foreach (ICoordinate adjacentCoordinate in node.GetCoordinate().GetAdjacents())
            {
                if (adjacentCoordinate is not TCoordinate coordinate) continue;

                if (circumnavigable)
                    WrapCoordinate(coordinate);
                else
                    ClampCoordinate(coordinate);

                if (Nodes.TryGetValue(coordinate, out TNode adjacentNode))
                    adjacents.Add(adjacentNode);
            }

            return adjacents;
        }

        public ICollection<TCoordinate> GetAdjacents(TCoordinate coordinate)
        {
            List<TCoordinate> adjacents = new();
            
            foreach (TNode node in GetAdjacents(Nodes[coordinate]))
                adjacents.Add(node.GetCoordinate());

            return adjacents;
        }

        public ICollection<TNode> GetNodes()
        {
            return Nodes.Values;
        }

        public TCoordinate GetSize()
        {
            return size;
        }

        public Node<TCoordinate> GetNodeAtIndexes(int x, int y)
        {
            TCoordinate coordinate = new();
            coordinate.Set(x, y);

            return GetNodeAt(coordinate);
        }

        public Node<TCoordinate> GetNodeAt(TCoordinate coordinate)
        {
            if (circumnavigable)
                WrapCoordinate(coordinate);
            else
                ClampCoordinate(coordinate);
            
            return Nodes.GetValueOrDefault(coordinate);
        }

        private void ClampCoordinate(TCoordinate coordinate)
        {
            if (coordinate.X < 0)
                coordinate.Set(0, coordinate.Y);
            else if (coordinate.X >= size.X)
                coordinate.Set(size.X - 1, coordinate.Y);

            if (coordinate.Y < 0)
                coordinate.Set(coordinate.X, 0);
            else if (coordinate.Y >= size.Y)
                coordinate.Set(coordinate.X, size.Y - 1);
        }

        private void WrapCoordinate(TCoordinate coordinate)
        {
            while (coordinate.X < 0)
                coordinate.Set(coordinate.X + size.X, coordinate.Y);
            while (coordinate.X >= size.X)
                coordinate.Set(coordinate.X - size.X, coordinate.Y);
                
            while (coordinate.Y < 0)
                coordinate.Set(coordinate.X, coordinate.Y + size.Y);
            while (coordinate.Y >= size.Y)
                coordinate.Set(coordinate.X, coordinate.Y - size.Y);
        }

        public Node<TCoordinate> GetNodeFromPosition(float x, float y)
        {
            int xi = (int)Math.Round(x / nodeDistance);
            int yi = (int)Math.Round(y / nodeDistance);

            return GetNodeAtIndexes(xi, yi);
        }

        public (float x, float y) GetPositionFromCoordinate(Coordinate coordinate)
        {
            return (coordinate.X * nodeDistance, coordinate.Y * nodeDistance);
        }

        public ICollection<TNode> GetBresenhamNodes(TCoordinate start, TCoordinate end)
        {
            List<TNode> result = new();

            if (start == null || end == null) return result;

            int x0 = start.X;
            int y0 = start.Y;
            int x1 = end.X;
            int y1 = end.Y;

            // If the graph wraps around, choose the shortest wrapped delta to the end
            if (circumnavigable)
            {
                // Choose x1 and y1 in an expanded grid so that |x1-x0| and |y1-y0| are minimal
                int bestX = x1;
                int bestY = y1;

                // Evaluate wrapping options for X
                int[] xCandidates = { x1, x1 + size.X, x1 - size.X };
                int minDx = int.MaxValue;
                foreach (int xc in xCandidates)
                {
                    int dxc = Math.Abs(xc - x0);
                    if (dxc < minDx)
                    {
                        minDx = dxc;
                        bestX = xc;
                    }
                }

                // Evaluate wrapping options for Y
                int[] yCandidates = { y1, y1 + size.Y, y1 - size.Y };
                int minDy = int.MaxValue;
                foreach (int yc in yCandidates)
                {
                    int dyc = Math.Abs(yc - y0);
                    if (dyc < minDy)
                    {
                        minDy = dyc;
                        bestY = yc;
                    }
                }

                x1 = bestX;
                y1 = bestY;
            }

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int x = x0;
            int y = y0;

            AddNodeAt(x, y);

            if (dx >= dy)
            {
                int err = dx / 2;
                while (x != x1)
                {
                    x += sx;
                    err -= dy;
                    if (err < 0)
                    {
                        y += sy;
                        err += dx;
                    }
                    AddNodeAt(x, y);
                }
            }
            else
            {
                int err = dy / 2;
                while (y != y1)
                {
                    y += sy;
                    err -= dx;
                    if (err < 0)
                    {
                        x += sx;
                        err += dy;
                    }
                    AddNodeAt(x, y);
                }
            }

            return result;

            // Local helper to add a node at the (possibly wrapped) position, avoiding consecutive duplicates
            void AddNodeAt(int xi, int yi)
            {
                int wx = xi;
                int wy = yi;

                if (circumnavigable)
                {
                    if (size.X > 0)
                    {
                        wx %= size.X;
                        if (wx < 0) wx += size.X;
                    }

                    if (size.Y > 0)
                    {
                        wy %= size.Y;
                        if (wy < 0) wy += size.Y;
                    }
                }

                TCoordinate coord = new();
                coord.Set(wx, wy);

                if (Nodes.TryGetValue(coord, out TNode node))
                {
                    if (result.Count == 0 || !result[^1].GetCoordinate().Equals(coord))
                        result.Add(node);
                }
            }
        }

        public float GetNodeDistance()
        {
            return nodeDistance;
        }

        public float GetDistanceBetweenNodes(TNode a, TNode b)
        {
            if (!circumnavigable) return a.GetCoordinate().GetDistanceTo(b.GetCoordinate()) * nodeDistance;
            
            List<int> xDis = new();
            List<int> yDis = new();
                
            xDis.Add(Math.Abs(a.GetCoordinate().X - b.GetCoordinate().X));
            xDis.Add(Math.Abs(a.GetCoordinate().X - (b.GetCoordinate().X + size.X)));
            xDis.Add(Math.Abs(a.GetCoordinate().X - (b.GetCoordinate().X - size.X)));
                
            yDis.Add(Math.Abs(a.GetCoordinate().Y - b.GetCoordinate().Y));
            yDis.Add(Math.Abs(a.GetCoordinate().Y - (b.GetCoordinate().Y + size.Y)));
            yDis.Add(Math.Abs(a.GetCoordinate().Y - (b.GetCoordinate().Y - size.Y)));
                
            int minX = xDis.Min();
            int minY = yDis.Min();
                
            return (minX + minY) * nodeDistance;
        }

        public float GetDistanceBetweenCoordinates(TCoordinate a, TCoordinate b)
        {
            return GetDistanceBetweenNodes(Nodes[a], Nodes[b]);
        }

        public bool IsCircumnavigable()
        {
            return circumnavigable;
        }

        public void MoveContainableTo(INodeContainable<TCoordinate> containable, TCoordinate coordinate)
        {
            Nodes[containable.NodeCoordinate].RemoveNodeContainable(containable);
            Nodes[coordinate].AddNodeContainable(containable);
        }
    }
}