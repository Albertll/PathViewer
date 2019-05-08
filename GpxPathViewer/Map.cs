using System;
using System.Collections.Generic;
using System.Linq;

namespace GpxPathViewer
{
    public class Map
    {
        private IDictionary<long, Node> _nodes;

        public bool IsReady { get; set; }

        public ICollection<Way> Ways { get; set; } = new HashSet<Way>();

        public IDictionary<long, Node> AllNodes { get; set; } = new Dictionary<long, Node>();

        public IDictionary<long, Node> Nodes
        {
            get
            {
                if (_nodes != null)
                    return _nodes;

                _nodes = new Dictionary<long, Node>();
                
                foreach (var way in Ways)
                {
                    if (!_nodes.ContainsKey(way.StartId))
                        _nodes.Add(way.StartId, AllNodes[way.StartId]);
                    if (!_nodes.ContainsKey(way.EndId))
                        _nodes.Add(way.EndId, AllNodes[way.EndId]);
                }

                return _nodes;
            }
        }

        public ICollection<Way> RemoveDeadNodes()
        {
            var edges = GetEdges();
            var deadWays = new HashSet<Way>();

            var visited = new HashSet<long>();
            var queue = new Queue<long>();
            queue.Enqueue(Ways.First().StartId);

            while (queue.Any())
            {
                var node = queue.Dequeue();
                foreach (var neighbour in edges[node])
                {
                    if (visited.Contains(neighbour))
                        continue;

                    visited.Add(neighbour);
                    queue.Enqueue(neighbour);
                }
            }

            foreach (var way in Ways.ToList())
            {
                if (visited.Contains(way.StartId))
                    continue;

                Ways.Remove(way);
                deadWays.Add(way);
            }

            return deadWays;
        }

        public IDictionary<long, List<long>> GetEdges(bool directional = false)
        {
            var edges = new Dictionary<long, List<long>>();

            foreach (var way in Ways)
            {
                AddValue(edges, way.StartId, way.EndId);

                if (directional && way.OneWay)
                    continue;

                AddValue(edges, way.EndId, way.StartId);
            }

            return edges;
        }

        private static void AddValue(IDictionary<long, List<long>> dictionary, long key, long value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new List<long>());
            dictionary[key].Add(value);
        }

        public IDictionary<long, List<Tuple<long, double>>> GetEdgesWithDistance(bool directional = false)
        {
            var edges = new Dictionary<long, List<Tuple<long, double>>>();

            foreach (var way in Ways)
            {
                AddValue(edges, way.StartId, Tuple.Create(way.EndId, way.Time));

                if (directional && way.OneWay)
                    continue;

                AddValue(edges, way.EndId, Tuple.Create(way.StartId, way.Time));
            }

            return edges;
        }

        private static void AddValue(IDictionary<long, List<Tuple<long, double>>> dictionary, long key, Tuple<long, double> value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new List<Tuple<long, double>>());
            dictionary[key].Add(value);
        }
        
        public void Simplify()
        {
            foreach (var way in Ways)
                way.NodeIds = new[] { way.StartId, way.EndId };
        }

        public ICollection<Way> RemoveEndRoads()
        {
            var endRoads = new HashSet<Way>();

            var e = GetEdges().Where(a => a.Value.Count < 2).Select(a => a.Key).ToList();
            foreach (var way in Ways.ToList())
            {
                if (e.Contains(way.StartId) || e.Contains(way.EndId))
                {
                    endRoads.Add(way);
                    Ways.Remove(way);
                }
            }

            return endRoads;
        }
    }
}