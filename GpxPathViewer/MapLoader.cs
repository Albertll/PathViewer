﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace GpxPathViewer
{
    public class MapLoader
    {
        public static Map LoadFromFile(string filePath, bool withMapTooling = false)
        {
            var map = MapFileParser.LoadFromFile(filePath);
            
            if (withMapTooling)
            {
                DivideRoads(map.Ways);

                MergeRoads(map.Ways);
            }

            CalcWaysLengths(map.Ways, map.AllNodes);

            return map;
        }

        public static void MergeRoads(ICollection<Way> ways)
        {
            Logger.Log("Merging roads");

            var heads = ways.GroupBy(a => a.StartId, a => a).Where(a => a.Count() == 1).ToDictionary(a => a.Key, a => a.First());
            var tails = ways.GroupBy(a => a.EndId, a => a).Where(a => a.Count() == 1).ToDictionary(a => a.Key, a => a.First());
            var joined = heads.Join(tails, a => a.Key, b => b.Key, Tuple.Create).ToList();

            foreach (var tuple in joined)
            {
                var key = tuple.Item2.Key;
                var tail = tuple.Item2.Value;
                var head = tuple.Item1.Value;

                while (tail.Parent != null && tail.Parent.WayId == -2)
                {
                    tail = tail.Parent;
                }

                while (head.Parent != null && head.Parent.WayId == -2)
                {
                    head = head.Parent;
                }

                if (tail.OneWay != head.OneWay) continue;

                var newWay = new Way
                {
                    WayId = -2,
                    Children = new[] { tail, head },
                    NodeIds = tail.NodeIds.Concat(head.NodeIds.Skip(1)).ToList(),
                    Parent = tail.WayId == -2 ? tail.Parent : tail,
                    Root = tail.Root ?? tail,
                    OneWay = tail.OneWay,
                    Length = tail.Length + head.Length,
                    Time = tail.Time + head.Time
                };

                tail.Parent = newWay;
                head.Parent = newWay;

                ways.Remove(tail);
                ways.Remove(head);
                ways.Add(newWay);

                tails[key] = newWay;
                heads.Remove(key);
            }
        }

        private static void DivideRoads(ICollection<Way> ways)
        {
            Logger.Log("Dividing roads");

            var all = ways.SelectMany(a => a.NodeIds).OrderBy(a => a).ToList();

            var intersects = new HashSet<long>(all.GroupBy(a => a).Where(a => a.Count() > 1).Select(a => a.Key));

            foreach (var way in ways.ToList())
            {
                var newWayNodes = way.NodeIds.Take(way.NodeIds.Count - 1).Skip(1).Where(a => intersects.Contains(a)).ToList();
                if (!newWayNodes.Any())
                    continue;

                var startNode = way.StartId;
                foreach (var newWayNode in newWayNodes)
                {
                    var newWay = new Way
                    {
                        Parent = way,
                        Root = way.Root ?? way,
                        WayId = -1,
                        NodeIds = way.NodeIds.SkipWhile(a => a != startNode).TakeWhile(a => a != newWayNode).ToList()
                    };
                    newWay.NodeIds.Add(newWayNode);

                    startNode = newWayNode;
                    ways.Add(newWay);
                    way.Children.Add(newWay);
                }

                var newWayEnd = new Way
                {
                    Parent = way,
                    Root = way.Root ?? way,
                    WayId = -1,
                    NodeIds = way.NodeIds.SkipWhile(a => a != startNode).ToList()
                };
                ways.Add(newWayEnd);
                way.Children.Add(newWayEnd);

                ways.Remove(way);
            }
        }

        private static void CalcWaysLengths(IEnumerable<Way> ways, IDictionary<long, Node> nodes)
        {
            foreach (var way in ways)
            {
                var length = 0.0;
                Node lastNode = null;

                var wayNodes = way.NodeIds.Select(n => nodes[n]).ToList();

                foreach (var node in wayNodes)
                {
                    if (lastNode == null)
                    {
                        lastNode = node;
                        continue;
                    }

                    length += node.DistanceTo(lastNode);

                    lastNode = node;
                }

                way.Length = length * 1000;
                int aa;
                var speed = way.Tags.ContainsKey("maxspeed")
                    ? (int.TryParse(way.Tags["maxspeed"], out aa) ? aa : 50)
                    : 20;
                way.Time = way.Length / speed / 1000 * 60;

                way.MinX = wayNodes.Min(n => n.Lon);
                way.MinY = wayNodes.Min(n => n.Lat);
                way.MaxX = wayNodes.Max(n => n.Lon);
                way.MaxY = wayNodes.Max(n => n.Lat);

                way.Nodes = wayNodes;
            }
        }
    }
}
