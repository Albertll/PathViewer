using System.Collections.Generic;
using System.Linq;

namespace GpxPathViewer
{
    public class Way
    {
        private bool? _oneWay;
        private Dictionary<string, string> _tags;

        public Way Parent { get; set; }
        public Way Root { get; set; }
        public IList<Way> Children { get; set; } = new List<Way>();

        public long WayId { get; set; }
        public IList<long> NodeIds { get; set; }

        public IList<Node> Nodes { get; set; }

        public Dictionary<string, string> Tags
        {
            get { return _tags ?? Root.Tags; }
            set { _tags = value; }
        }

        public double Length { get; set; }

        public double Time { get; set; }

        public long StartId => NodeIds.First();
        public long EndId => NodeIds.Last();

        public bool OneWay
        {
            get { return _oneWay ?? (_oneWay = Root?.OneWay ?? Tags.ContainsKey("oneway") && Tags["oneway"] == "yes") ?? false; }
            set { _oneWay = value; }
        }

        public long GetEndId(long startNode)
            => StartId == startNode ? EndId : StartId;

        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double MinX { get; set; }
        public double MinY { get; set; }
    }
}