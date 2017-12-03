using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SBC
{
    public class Edge
    {
        public int From { set; get;}
        public int To { set; get; }
        public int Weight { set; get; }

        public Edge(int from, int to, int weight)
        {
            this.From = from;
            this.To = to;
            this.Weight = weight;
        }

    }

    public class EdgeList
    {
        private int verticeCount;
        public int VerticeCount { get => verticeCount; }
        public int EdgeCount { get => edges.Count; }
        private List<Edge> edges;
        private Random rand = new Random();

        public EdgeList(int numVertices, int numEdges = 10)
        {
            this.edges = new List<Edge>(numEdges);
            this.verticeCount = numVertices;
        }

        public bool HasEdge(int from, int to)
            => GetEdge(from, to) != null;

        public bool AddEdge(Edge edge)
        {
            if (HasEdge(edge.From, edge.To)) return false;
            edges.Add(edge);
            return true;
        }

        public Edge GetEdge(int from, int to)
        {
            foreach (Edge edge in edges)
                if (edge.From == from && edge.To == to
                    || edge.From == to && edge.To == from)
                    return edge;
            return null;
        }

        public Int32 GetWeight(int from, int to)
        {
            Edge edge = GetEdge(from, to);
            return (edge == null) ? -1 : edge.Weight;
        }

        public int[] GetNeighbours(int vertex)
        {
            HashSet<int> result = new HashSet<int>();
            foreach (Edge edge in edges)
            {
                if (edge.From == vertex) result.Add(edge.To);
                if (edge.To == vertex) result.Add(edge.From);
            }
            return result.ToArray();
        }

        public int MeasureDistance(int[] path)
        {
            int result = 0;
            for (int i = 0; i < path.Length - 1; ++i)
            {
                int from = path[i];
                int to = path[i + 1];
                int weight = GetWeight(from, to);
                if (weight < 0) return -1;
                result += weight;
            }
            return result;
        }

        public override string ToString()
        {
            string s = VerticeCount.ToString() + " vertices, "
                + EdgeCount.ToString() + " edges:\n";
            foreach (Edge edge in edges)
                s += edge.From.ToString() + " - " 
                    + edge.To.ToString() 
                    + ", weighted " + edge.Weight.ToString() 
                    + "\n";
            return s;
        }

        public int[] RandomPath(int start, int dest, int[] visitedVertices = null)
        {
            HashSet<int> visited = (visitedVertices == null) 
                ? new HashSet<int>() 
                : new HashSet<int>(visitedVertices);
            visited.Add(start);
            List<int> path = new List<int> { start };
            int currentVertex = path.Last();
            while (currentVertex != dest)
            {
                int[] neighs = GetNeighbours(currentVertex).Except(visited).ToArray();
                if (neighs.Length == 0)
                {
                    if (path.Count == 1) return null;
                    path.RemoveAt(path.Count - 1);
                }
                else
                {
                    int nextVertex = neighs[rand.Next(neighs.Length)];
                    path.Add(nextVertex);
                }
                currentVertex = path.Last();
                visited.Add(currentVertex);
            }
            return path.ToArray();
        }

        public int[] ModifyRandomPath(int[] path)
        {
            int[] unchangedPathPart = null,
                changedPathPart = null;
            while (changedPathPart == null)
            {
                int changeIndex = rand.Next(1, path.Length - 1);
                unchangedPathPart = new int[changeIndex];
                Array.Copy(path, unchangedPathPart, changeIndex);
                changedPathPart = RandomPath(path[changeIndex], path[path.Length - 1], unchangedPathPart);
            }
            return unchangedPathPart.Concat(changedPathPart).ToArray();
        }

        private bool IsValidPath(int[] path)
        {
            for (int i = 0; i < path.Length - 1; ++i)
                if (!HasEdge(path[i], path[i + 1]))
                    return false;
            return true;
        }
    }
}
