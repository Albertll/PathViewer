using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GpxPathViewer
{
    public class Path : IEnumerable<Node>
    {
        public IList<Node> Nodes { get; }
        public string FilePath { get; set; }
        public double Length => GetLength(Nodes);
        public int Id => int.Parse(string.Concat(FilePath.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit)));

        public TimeSpan Duration => ((TimeNode) Nodes.Last()).Time - ((TimeNode) Nodes.First()).Time;
        public DateTime TimeStart => ((TimeNode)Nodes.First()).Time;
        public DateTime TimeEnd => ((TimeNode)Nodes.Last()).Time;
        public string Start => Hub.GetWaveloHubs().FirstOrDefault(a => a.IsInside(Nodes.First()))?.Name; 
        public string Stop => Hub.GetWaveloHubs().FirstOrDefault(a => a.IsInside(Nodes.Last()))?.Name; 
        
        public Path(IEnumerable<Node> nodes)
        {
            Nodes = nodes.ToList();
        }

        public override string ToString()
            => Id + "\t" +
               Length.ToString("F2") + "\t" +
               Duration + "\t" +
               Start + "\t" +
               Stop + "\t" +
               TimeStart + "\t" +
               TimeEnd;

        public IEnumerator<Node> GetEnumerator() => Nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static double GetLength(IEnumerable<Node> nodes)
        {
            var length = 0.0;

            Node previous = null;
            foreach (var node in nodes)
            {
                if (previous != null)
                    length += node.DistanceTo(previous);

                previous = node;
            }

            return length;
        }
    }
}