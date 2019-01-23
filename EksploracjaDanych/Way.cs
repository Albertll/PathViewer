using System.Collections.Generic;
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
}