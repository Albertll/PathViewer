using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using QuickGraph;
using QuickGraph.Algorithms.RankedShortestPath;

namespace EksploracjaDanych
{
    public partial class MainForm : Form
    {
        private const string File = @"C:\Users\Albert\Desktop\Eksploracja danych\Program\EksploracjaDanych\set5.json";
        private const bool CalcClusteringCoefficient = false;
        private const bool CalcBetweennessCentrality = true;
        private const int BetweennessCentralityIterations = 1000;



        private Map Map { get; set; }

        private ICollection<Way> _deadWays = new List<Way>();

        private readonly Dictionary<long, double> _clusteringWage = new Dictionary<long, double>();
        private Dictionary<long, double> _betweennessWage = new Dictionary<long, double>();

        private IList<long> _path;

        private double? _minX;
        private double? _minY;
        private double? _maxX;
        private double? _maxY;
        private double? _avgX;
        private double? _avgY;

        private double _scale = 1.3;
        private Point _moveStart;
        private Size _move = new Size(1, 1);
        private double _scaledPoints;




        public MainForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            Logger.Logging += msg => Invoke((Action) (() =>
            {
                label1.Text = msg;
                label1.Invalidate();
            }));
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            await Task.Run((Action) PrepareData);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _moveStart = e.Location;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if ((e.Button & MouseButtons.Left) != 0)
            {
                _move += new Size(e.X - _moveStart.X, e.Y - _moveStart.Y);
                Invalidate();
            }
        }
        
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            const double factor = 1.5;

            base.OnMouseWheel(e);
            if (e.Delta > 0)
            {
                _move = new Size((int)(_move.Width * factor), (int)(_move.Height * factor));
                _scale *= factor;
            }
            if (e.Delta < 0)
            {
                _scale /= factor;
                _move = new Size((int)(_move.Width / factor), (int)(_move.Height / factor));
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Map == null || !Map.IsReady)
                return;

            var s = new Stopwatch();
            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();
            var sw3 = new Stopwatch();

            s.Start();

            _minX = _minX ?? (_minX = Map.Nodes.Min(n => n.Value.Lon));
            _minY = _minY ?? (_minY = Map.Nodes.Min(n => n.Value.Lat));
            _maxX = _maxX ?? (_maxX = Map.Nodes.Max(n => n.Value.Lon));
            _maxY = _maxY ?? (_maxY = Map.Nodes.Max(n => n.Value.Lat));
            _avgX = _avgX ?? (_avgX = Map.Nodes.Average(n => n.Value.Lon));
            _avgY = _avgY ?? (_avgY = Map.Nodes.Average(n => n.Value.Lat));

            if (_scaledPoints != _scale)
            {
                _scaledPoints = _scale;

                foreach (var node in Map.AllNodes.Values)
                {
                    node.Loc = new Point(
                        (int) ((node.Lon - _avgX) * 1000 * _scale),
                        (int) (Height - (node.Lat - _avgY) * 1545 * _scale));
                }
            }

            var screenMove = new Size(Width / 2, -Height / 2) + _move;

            //Func<Node, Point> p = n => n.Loc + screenMove;

            var pen1 = new Pen(Color.DarkGreen);
            foreach (var way in Map.Ways)
            {
                if (way.Nodes.Count < 2)
                    continue;

                //if (!way.Tags.ContainsKey("highway") || !new [] { "motorway", "motorway_link", "trunk", "trunk_link", "primary", "primary_link", "secondary", "secondary_link"  }.Contains(way.Tags["highway"]))
                //    continue;

                sw1.Start();
                var source = way.Nodes.Select(n => Map.AllNodes[n]).Select(n => n.Loc + screenMove).ToArray();
                sw1.Stop();

                sw2.Start();
                e.Graphics.DrawLines(pen1, source);
                sw2.Stop();
            }

            var pen2 = new Pen(Color.Tomato);
            foreach (var way in _deadWays)
            {
                if (way.Nodes.Count < 2)
                    continue;
                
                var source = way.Nodes.Select(n => Map.AllNodes[n]).Select(n => n.Loc + screenMove).ToArray();
                e.Graphics.DrawLines(pen2, source);
            }

            sw3.Start();
            foreach (var pair in _clusteringWage)
            {
                var node = Map.AllNodes[pair.Key];
                
                var v = pair.Value;
                var size = pair.Value == 0 ? 3 : 8;

                var r = v < 1.0 ? MinMax(v * 1020 - 510) : MinMax(v * -1020 + 1275);
                var g = v < 0.5 ? MinMax(v * 1020) : MinMax(v * -1020 + 1020);
                var b = MinMax(v * -1020 + 510);

                var color = Color.FromArgb(r, g, b);

                e.Graphics.FillEllipse(new SolidBrush(color), node.Loc.X + screenMove.Width, node.Loc.Y + screenMove.Height, size, size);
                e.Graphics.DrawEllipse(new Pen(color), node.Loc.X + screenMove.Width, node.Loc.Y + screenMove.Height, size, size);
            }
            sw3.Stop();

            sw3.Start();

            var max = 0.4;
            foreach (var pair in _betweennessWage)
            {
                var node = Map.AllNodes[pair.Key];

                var v = Math.Log(1 + pair.Value) * 2.5;

                var r = v < 1.0 ? MinMax(v * 1020 - 510) : MinMax(v * -1020 + 1275);
                var g = v < 0.5 ? MinMax(v * 1020) : MinMax(v * -1020 + 1020);//MinMax(v * 1020 - 255);
                var b = MinMax(v * -1020 + 510);

                var color = Color.FromArgb(r, g, b);
                    
                var size = pair.Value == 0 ? 3 : 8;

                e.Graphics.FillEllipse(new SolidBrush(color), node.Loc.X + screenMove.Width, node.Loc.Y + screenMove.Height, size, size);
                e.Graphics.DrawEllipse(new Pen(color), node.Loc.X + screenMove.Width, node.Loc.Y + screenMove.Height, size, size);
            }
            
            sw3.Stop();

            s.Stop();

            if (_path != null && _path.Count > 1)
                e.Graphics.DrawLines(new Pen(Color.Red), _path.Select(n => Map.Nodes[n])
                    .Select(n => n.Loc + screenMove).ToArray());
        }

        private int MinMax(double v0)
        {
            var v = (int)v0;
            return v > 255 ? 255 : (v < 0 ? 0 : v);
        }


        private void PrepareData()
        {
            Map = MapLoader.LoadFromFile(File);
            _deadWays = Map.RemoveDeadNodes();

            if (CalcClusteringCoefficient)
                ClusteringCoefficient();

            //Map.Simplify();

            //_deadWays = Map.RemoveEndRoads();
            //_deadWays = Map.RemoveEndRoads();
            //_deadWays = Map.RemoveEndRoads();
            //MapLoader.MergeRoads(Map.Ways);
            Map.IsReady = true;

            Invalidate();
            if (CalcBetweennessCentrality)
                BetweennessCentrality();
            Invalidate();
        }
        
        private void BetweennessCentrality()
        {
            //var pathGraph = new BidirectionalGraph<long, TaggedEdge<long, long>>();
            var pathGraph = new BidirectionalGraph<long, TaggedEdge<long, double>>();

            foreach (var vertex in Map.GetEdges().Keys)
            {
                pathGraph.AddVertex(vertex);
            }

            foreach (var keyValuePair in Map.GetEdgesWithDistance(true))
            {
                foreach (var way in keyValuePair.Value)
                {
                    //var edge = new Edge<long>(keyValuePair.Key, way);
                    var edge = new TaggedEdge<long, double>(keyValuePair.Key, way.Item1, way.Item2);
                    pathGraph.AddEdge(edge);
                }

            }

            var alg = new HoffmanPavleyRankedShortestPathAlgorithm<long, TaggedEdge<long, double>>(pathGraph, e => e.Tag);
            var sw = new Stopwatch();
            sw.Start();


            sw.Stop();
            _path = new List<long>();
            double last = 0;
            

            var r = new Random();
            var ways = Map.Ways.ToList();

            var nodeWages = Map.Nodes.Select(a => a.Key).ToDictionary(a => a, a => 0);

            var time = DateTime.Now;
            var proc = 0;


            for (var i = 0; i < BetweennessCentralityIterations; i++)
            {
                var nProc = 100 * i / BetweennessCentralityIterations;
                if (nProc != proc)
                {
                    proc = nProc;
                    var estimate = new DateTime((DateTime.Now - time).Ticks / proc * (100 - proc));
                    Logger.Log(proc + "%, pozostało: " + estimate.ToString("mm:ss"));
                }


                var r1 = ways[r.Next(ways.Count)];
                var r2 = ways[r.Next(ways.Count)];

                if (r1.Nodes.Contains(r2.Start))
                    continue;

                alg.Compute(r1.Start, r2.Start);
                foreach (var shortestPath in alg.ComputedShortestPaths)
                {
                    foreach (var edge in shortestPath)
                    {
                        nodeWages[edge.Source]++;
                    }
                }

                if (i % 100 == 99)
                {
                    var n22 = nodeWages.Where(a => a.Value > 0).OrderBy(a => a.Value);

                    _betweennessWage = n22.ToDictionary(a => a.Key, a => a.Value * 1.0 / i);
                }
            }

            var n2 = nodeWages.Where(a => a.Value > 0).OrderBy(a => a.Value);

            _betweennessWage = n2.ToDictionary(a => a.Key, a => a.Value * 1.0 / BetweennessCentralityIterations);

            Logger.Log($"Centralność max: {_betweennessWage.Max(a => a.Value)}, " +
                       $"średnia: {_betweennessWage.Average(a => a.Value)}");
        }

        private void ClusteringCoefficient()
        {
            Logger.Log("Calc clustering coefficient");

            var nodes = Map.GetEdges();
            foreach (var node in nodes.Keys)
            {
                var nn = nodes[node];

                var sum = 0;

                for (var i = 0; i < nn.Count; i++)
                {
                    for (var j = i + 1; j < nn.Count; j++)
                    {
                        var vi = nn[i];
                        var vj = nn[j];
                        if (nodes.ContainsKey(vi) && nodes[vi].Contains(vj))
                            sum++;
                    }
                }

                if (nn.Count > 1)
                    _clusteringWage[node] = 2.0 * sum / nn.Count / (nn.Count - 1);
            }

            Logger.Log(string.Empty);
        }
    }
}
