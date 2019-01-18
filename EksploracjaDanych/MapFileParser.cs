using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace EksploracjaDanych
{
    public static class MapFileParser
    {
        public static Map LoadFromFile(string filePath)
        {
            Logger.Log("Loading data from file...");

            var map = new Map();

            var content = Encoding.UTF8.GetBytes(File.ReadAllText(filePath));

            var reader = JsonReaderWriterFactory.CreateJsonReader(content, new System.Xml.XmlDictionaryReaderQuotas());
            
            var elements = XElement.Load(reader).XPathSelectElement("elements");

            foreach (var element in elements.Nodes().OfType<XElement>())
            {
                var type = element.XPathSelectElement("type");

                switch (type.Value)
                {
                    case "way":
                        var way = LoadWay(element);

                        map.Ways.Add(way);
                        break;
                    case "node":
                        var node = LoadNode(element);

                        map.AllNodes.Add(node.Id, node);
                        break;
                }
            }

            Logger.Log("Loaded data from file.");

            return map;
        }

        private static Way LoadWay(XNode element)
        {
            var id = element.XPathSelectElement("id");
            var nodes = element.XPathSelectElement("nodes");
            var tags = element.XPathSelectElement("tags");

            var way = new Way
            {
                Id = long.Parse(id.Value),
                Tags = new Dictionary<string, string>()
            };

            foreach (var subNode in nodes.Nodes().OfType<XElement>())
            {
                way.Nodes.Add(long.Parse(subNode.Value));
            }

            foreach (var subNode in tags.Nodes().OfType<XElement>())
            {
                if (!subNode.Name.ToString().Contains("item"))
                    way.Tags.Add(subNode.Name.ToString(), subNode.Value);
            }

            return way;
        }

        private static Node LoadNode(XNode element)
        {
            var id = element.XPathSelectElement("id");
            var lat = element.XPathSelectElement("lat");
            var lon = element.XPathSelectElement("lon");

            var node = new Node
            {
                Id = long.Parse(id.Value),
                Lat = double.Parse(lat.Value.Replace(".", ",")),
                Lon = double.Parse(lon.Value.Replace(".", ","))
            };
            return node;
        }
    }
}