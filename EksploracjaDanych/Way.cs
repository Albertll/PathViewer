using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EksploracjaDanych
{
    public class Way
    {
        private bool? _oneWay;
        private Dictionary<string, string> _tags;

        public Way Parent { get; set; }
        public Way Root { get; set; }
        public IList<Way> Children { get; set; } = new List<Way>();

        public long Id { get; set; }
        public IList<long> Nodes { get; set; } = new List<long>();

        public Dictionary<string, string> Tags
        {
            get { return _tags ?? Root.Tags; }
            set { _tags = value; }
        }

        public double Length { get; set; }

        public double Time { get; set; }

        public long Start => Nodes.First();
        public long End => Nodes.Last();

        public bool OneWay
        {
            get { return _oneWay ?? Root?.OneWay ?? Tags.ContainsKey("oneway") && Tags["oneway"] == "yes"; }
            set { _oneWay = value; }
        }

        public long GetEnd(long startNode)
            => Start == startNode ? End : Start;
    }

    public class Node
    {
        public long Id { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public Point Loc { get; set; }

        public double DistanceTo(Node node) => DistanceTo(Lat, Lon, node.Lat, node.Lon);

        private static double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            var rlat1 = Math.PI * lat1 / 180;
            var rlat2 = Math.PI * lat2 / 180;
            var theta = lon1 - lon2;
            var rtheta = Math.PI * theta / 180;
            var dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            switch (unit)
            {
                case 'K': //Kilometers -> default
                    return dist * 1.609344;
                case 'N': //Nautical Miles 
                    return dist * 0.8684;
                case 'M': //Miles
                    return dist;
            }

            return dist;
        }
    }

    //public class Intersection
    //{
    //    public long NodeId { get; set; }
    //    public List<Node> Nodes { get; set; } = new List<Node>();
    //}
}