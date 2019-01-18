using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EksploracjaDanych
{
    public class MapLoader
    {
        public static Map LoadFromFile(string filePath)
        {
            var map = MapFileParser.LoadFromFile(filePath);

            /*
            // autostrada do Wieliczki
            //2090492528,325927630  , 5116259356

            // autostrada do 2
            //2060318486,5884370177  , 5302232353

            var w = new Way();
            w.Nodes = new List<long>
            {
                2060318486,
                5884370177,
                5302232353,

            };
            w.Id = 123;
            w.Tags = new Dictionary<string, string>();
            w.Tags["maxspeed"] = "50";
            map.Ways.Add(w);



            // królewska

            w = new Way();
            w.Nodes = new List<long>
            {
                2419959800,
                5926931609,
                470618562,

                1079937395,
                1496410012,
                2559833922,
                3023046447,
                4692967587
            };
            w.Id = 123;
            w.Tags = new Dictionary<string, string>();
            w.Tags["maxspeed"] = "50";
            //1079937395 1496410012 2559833922 3023046447 4692967587
            map.Ways.Add(w);

            w = new Way();
            w.Nodes = new List<long>
            {
                2419894894,
                226836665,
                5219326000
            };
            w.Id = 124;
            w.Tags = new Dictionary<string, string>();
            w.Tags["maxspeed"] = "50";
            //1079937395 1496410012 2559833922 3023046447 4692967587
            map.Ways.Add(w);

            w = new Way();
            w.Nodes = new List<long>
            {
                251693631,
                2068886615
            };
            w.Id = 124;
            w.Tags = new Dictionary<string, string>();
            w.Tags["maxspeed"] = "50";
            //1079937395 1496410012 2559833922 3023046447 4692967587
            map.Ways.Add(w);


            w = new Way();
            w.Nodes = new List<long>
            {
                207516242,
                1080683889,
                2280439144
            };
            w.Id = 124;
            w.Tags = new Dictionary<string, string>();
            w.Tags["maxspeed"] = "50";
            //1079937395 1496410012 2559833922 3023046447 4692967587
            map.Ways.Add(w);

            w = new Way();
            w.Nodes = new List<long>
            {
                240977615,
                3741515640,
                251693603
            };
            w.Id = 124;
            w.Tags = new Dictionary<string, string>();
            w.Tags["maxspeed"] = "50";
            //1079937395 1496410012 2559833922 3023046447 4692967587
            map.Ways.Add(w);
            */

            DivideRoads(map.Ways);

            MergeRoads(map.Ways);

            CalcWaysLengths(map.Ways, map.AllNodes);

            return map;
        }

        public static void MergeRoads(ICollection<Way> ways)
        {
            Logger.Log("Merging roads");

            var heads = ways.GroupBy(a => a.Start, a => a).Where(a => a.Count() == 1).ToDictionary(a => a.Key, a => a.First());
            var tails = ways.GroupBy(a => a.End, a => a).Where(a => a.Count() == 1).ToDictionary(a => a.Key, a => a.First());
            var joined = heads.Join(tails, a => a.Key, b => b.Key, Tuple.Create).ToList();

            foreach (var tuple in joined)
            {
                var key = tuple.Item2.Key;
                var tail = tuple.Item2.Value;
                var head = tuple.Item1.Value;

                while (tail.Parent != null && tail.Parent.Id == -2)
                {
                    tail = tail.Parent;
                }

                while (head.Parent != null && head.Parent.Id == -2)
                {
                    head = head.Parent;
                }

                if (tail.OneWay != head.OneWay) continue;

                var newWay = new Way
                {
                    Id = -2,
                    Children = new[] { tail, head },
                    Nodes = tail.Nodes.Concat(head.Nodes.Skip(1)).ToList(),
                    Parent = tail.Id == -2 ? tail.Parent : tail,
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

            var all = ways.SelectMany(a => a.Nodes).OrderBy(a => a).ToList();

            var intersects = new HashSet<long>(all.GroupBy(a => a).Where(a => a.Count() > 1).Select(a => a.Key));

            foreach (var way in ways.ToList())
            {
                var newWayNodes = way.Nodes.Take(way.Nodes.Count - 1).Skip(1).Where(a => intersects.Contains(a)).ToList();
                if (!newWayNodes.Any())
                    continue;

                var startNode = way.Start;
                foreach (var newWayNode in newWayNodes)
                {
                    var newWay = new Way
                    {
                        Parent = way,
                        Root = way.Root ?? way,
                        Id = -1,
                        Nodes = way.Nodes.SkipWhile(a => a != startNode).TakeWhile(a => a != newWayNode).ToList()
                    };
                    newWay.Nodes.Add(newWayNode);

                    startNode = newWayNode;
                    ways.Add(newWay);
                    way.Children.Add(newWay);
                }

                var newWayEnd = new Way
                {
                    Parent = way,
                    Root = way.Root ?? way,
                    Id = -1,
                    Nodes = way.Nodes.SkipWhile(a => a != startNode).ToList()
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

                foreach (var node in way.Nodes.Select(n => nodes[n]))
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
            }
        }
    }
}
