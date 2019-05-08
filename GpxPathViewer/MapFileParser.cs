using System.Globalization;
using System.Linq;
using Albertl.FileParsing;

namespace GpxPathViewer
{
    public static class MapFileParser
    {
        public static Map LoadFromFile(string filePath)
        {
            Logger.Log("Loading data from file...");

            var map = new Map();
            
            var mainNode = FileParser.ParseJson(filePath);

            map.Ways = mainNode["elements"].Nodes.Where(n => n["type"].Value == "way").Select(LoadWay).ToList();
            map.AllNodes = mainNode["elements"].Nodes.Where(n => n["type"].Value == "node").Select(LoadNode).ToDictionary(n => n.NodeId, n => n);
            
            Logger.Log("Loaded data from file.");

            return map;
        }
        
        private static Way LoadWay(IParsedNode node) => new Way
        {
            WayId = long.Parse(node["id"].Value),
            NodeIds = node["nodes"].Nodes.Select(n => long.Parse(n.Value)).ToList(),
            Tags = node["tags"].Nodes.ToDictionary(n => n.Name, n => n.Value)
        };

        private static Node LoadNode(IParsedNode node) => new Node
        {
            NodeId = long.Parse(node["id"].Value),
            Lat = double.Parse(node["lat"].Value, new NumberFormatInfo()),
            Lon = double.Parse(node["lon"].Value, new NumberFormatInfo())
        };
    }
}