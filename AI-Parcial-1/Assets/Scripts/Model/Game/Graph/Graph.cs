using System;
using System.Collections.Generic;
using Model.Tools.Pathfinder.Coordinate;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;

namespace Model.Game.Graph
{
    public class Graph<TNode, TCoordinate> : IGraph<TNode, TCoordinate> 
        where TNode : Node<TCoordinate>, new() 
        where TCoordinate : Coordinate, new()
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
            size.SetPosition(x, y);
            
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    TCoordinate coordinate = new();
                    TNode node = new();

                    coordinate.SetPosition(i, j);
                    
                    node.SetCoordinate(coordinate);
                    Nodes.Add(coordinate, node);
                }
            }
        }

        public ICollection<TNode> GetAdjacents(TNode node)
        {
            List<TNode> adjacents = new();

            foreach (ICoordinate adjacentCoordinate in node.GetCoordinate().GetAdjacents())
            {
                if (adjacentCoordinate is not TCoordinate coordinate) continue;

                if (circumnavigable)
                {
                    while (coordinate.X < 0)
                        coordinate.SetPosition(coordinate.X + size.X, coordinate.Y);
                    
                    while (coordinate.X >= size.X)
                        coordinate.SetPosition(coordinate.X - size.X, coordinate.Y);
                    
                    while (coordinate.Y < 0)
                        coordinate.SetPosition(coordinate.X, coordinate.Y + size.Y);
                    
                    while (coordinate.Y >= size.Y)
                        coordinate.SetPosition(coordinate.X, coordinate.Y - size.Y);
                }
                
                if (Nodes.TryGetValue(coordinate, out TNode adjacentNode))
                    adjacents.Add(adjacentNode);
            }

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
            coordinate.SetPosition(x, y);

            return GetNodeAt(coordinate);
        }

        public Node<TCoordinate> GetNodeAt(TCoordinate coordinate)
        {
            return Nodes.GetValueOrDefault(coordinate);
        }
        
        public void BlockNodes(ICollection<TCoordinate> coordinates)
        {
            foreach (TCoordinate coordinate in coordinates)
                Nodes[coordinate].SetBlocked(true);
        }

        public void BlockNodes(ICollection<TNode> nodes)
        {
            foreach (TNode node in nodes)
                Nodes[node.GetCoordinate()].SetBlocked(true);
        }

        public ICollection<TNode> GetBresenhamNodes(TCoordinate start, TCoordinate end)
        {
            // Returns all nodes that lie on the integer grid line between start and end (inclusive)
            // using the classic integer Bresenham line algorithm. This implementation is adapted to:
            // - Work with generic TCoordinate/TNode types stored in a dictionary (Nodes)
            // - Skip coordinates that are outside the graph (by TryGetValue check)
            // - Avoid emitting duplicate nodes (a guard for safety when generics/hash collide)
            // Pure Bresenham, by contrast, outputs every (x, y) integer point regardless of bounds/availability
            // and does not need to create or look up objects in a container.
            List<TNode> result = new();
            /*
            
            // dx, dy are the absolute deltas along X and Y. In the "pure" algorithm dy is commonly negated
            // to allow a single error accumulator (err) and symmetric stepping for all octants.
            int dx = Math.Abs(end.X - start.X);
            int sx = start.X < end.X ? 1 : -1;   // sx: step direction on X (pure Bresenham uses the same)
            int dy = -Math.Abs(end.Y - start.Y);
            int sy = start.Y < end.Y ? 1 : -1;   // sy: step direction on Y (pure Bresenham uses the same)
            int err = dx + dy;                    // err starts as dx + dy (dy is negative). Classic variant uses this form.

            // Current integer position, initialised to start
            int x = start.X;
            int y = start.Y;

            // Main loop (identical structure to the classic integer Bresenham)
            while (x != end.X || y != end.Y)
            {
                // PURE BRESENHAM: would directly record the integer point (x, y)
                // HERE: we adapt it to the graph by creating a coordinate key and trying to fetch the node
                TCoordinate key = new();
                key.Set(x, y);
                if (Nodes.TryGetValue(key, out TNode value))
                {
                    // PURE BRESENHAM: no duplication concern because points are unique
                    // HERE: we add a defensive duplicate guard due to generic equality/hash behavior
                    if (result.Count == 0 || !EqualityComparer<TNode>.Default.Equals(result[^1], value))
                        result.Add(value);
                }

                // Inclusive termination condition: once we reached the end point, we stop.
                if (x == end.X && y == end.Y)
                    break;

                // Error-doubling trick: compare 2*err against dx/dy to decide which axis to step.
                // Modified to advance only one axis per iteration to ensure orthogonal adjacency between nodes.
                int e2 = 2 * err;
                bool stepX = e2 >= dy;
                bool stepY = e2 <= dx;

                if (stepX && stepY)
                {
                    // Both steps are possible; pick one axis this iteration to avoid diagonal advancement.
                    // Prefer stepping along the dominant axis (shallow vs. steep):
                    if (dx >= -dy) // shallow (|dx| >= |dy|) -> step X first
                    {
                        err += dy;
                        x += sx;
                    }
                    else           // steep -> step Y first
                    {
                        err += dx;
                        y += sy;
                    }
                }
                else if (stepX)
                {
                    err += dy;
                    x += sx;
                }
                else // stepY
                {
                    err += dx;
                    y += sy;
                }
            }

            // PURE BRESENHAM: would return all integer points. We return the nodes that exist in the graph
            // along that line. Points outside the graph are silently ignored by TryGetValue.
            */
            return result;
        }

        public float GetNodeDistance()
        {
            return nodeDistance;
        }

        public float GetDistanceBetweenNodes(TNode a, TNode b)
        {
            return a.GetCoordinate().GetDistanceTo(b.GetCoordinate()) * nodeDistance;
        }
    }
}