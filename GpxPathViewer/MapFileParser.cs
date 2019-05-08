using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Albertl.FileParsing;
using QuickGraph.Serialization;

namespace GpxPathViewer
{
    //internal class ParsedNode : IParsedNode
    //{
    //    public string Name { get; set; }

    //    public string Value { get; set; }

    //    public IReadOnlyList<IParsedNode> Nodes { get; set; }

    //    public IReadOnlyDictionary<string, string> Attributes { get; set; }

    //    public IParsedNode this[string name]
    //        => Nodes.FirstOrDefault(n => n.Name == name);

    //    public override string ToString()
    //        => string.IsNullOrEmpty(Value)
    //            ? Name
    //            : $"{Name}: {Value}";
    //}
    //public interface IParsedNode
    //{
    //    string Name { get; }

    //    string Value { get; }

    //    IReadOnlyList<IParsedNode> Nodes { get; }

    //    IReadOnlyDictionary<string, string> Attributes { get; }

    //    IParsedNode this[string name] { get; }
    //}

    //public static class FileParser
    //{
    //    public static IParsedNode ParseXml(string filePath)
    //        => ParseCore(XmlReader.Create(filePath));

    //    public static IParsedNode ParseJson(string filePath)
    //        => ParseCore(CreateJsonReader(filePath));

    //    private static IParsedNode ParseCore(XmlReader reader)
    //        => FindANodes(XElement.Load(reader));

    //    private static XmlReader CreateJsonReader(string filePath)
    //    {
    //        return JsonReaderWriterFactory.CreateJsonReader(
    //            Encoding.UTF8.GetBytes(File.ReadAllText(filePath)),
    //            new XmlDictionaryReaderQuotas());
    //    }

    //    public static Stopwatch s2 = new Stopwatch();
    //    private static IParsedNode FindANodes(XElement element, int i = 0)
    //    {
    //        //if (i == 4)
    //        //    return null;
    //        s2.Start();
    //        var nodes = element.Nodes().ToList();

    //        var node = new ParsedNode
    //        {
    //            Name = element.Name.LocalName,
    //            Attributes = element.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value),
    //        };
    //        s2.Stop();

    //        node.Nodes = nodes.OfType<XElement>().Select(xElement => FindANodes(xElement, i + 1)).ToList();
    //        if (!node.Nodes.Any() && nodes.OfType<XText>().Any())
    //            node.Value = nodes.OfType<XText>().First().Value;

    //        //Debug.WriteLine(s.ElapsedMilliseconds);
    //        return node;
    //    }
    //}
    public static class MapFileParser
    {
        public static Map LoadFromFile(string filePath)
        {
            Logger.Log("Loading data from file...");

            var map = new Map();
            
            var mainNode = FileParser.ParseJson(filePath);

            map.Ways = mainNode["elements"].Nodes.Where(n => n["type"].Value == "way").Select(LoadWay).ToList();
            map.AllNodes = mainNode["elements"].Nodes.Where(n => n["type"].Value == "node").Select(LoadNode).ToDictionary(n => n.NodeId, n => n);

            //Albertl.Serializer.SerializeToFile(map, @"C:\Users\Albert\Desktop\xx2.obj");
            //Albertl.Serializer.DeserializeFromFile(out map, @"C:\Users\Albert\Desktop\xx2.obj");

            foreach (var element in mainNode["elements"].Nodes)
            {
                if (element["type"].Value == "way")
                {
                    //map.Ways.Add(LoadWay(element));
                }
                else
                {
                    //var node = LoadNode(element);

                    //map.AllNodes.Add(node.Id, node);
                }
            }

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