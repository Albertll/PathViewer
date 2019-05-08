using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Albertl.FileParsing;

namespace GpxPathViewer
{
    public partial class MainForm : Form
    {
        #region Constants

        private const string DataFile = @"..\..\..\data\set4.json";
        //private const bool UseClusteringCoefficient = false;
        //private const bool UseBetweennessCentrality = false;
        //private const int BetweennessCentralityIterations = 1000;

        #endregion

        #region Fields

        private Map Map { get; set; }

        private ICollection<Way> _deadWays = new List<Way>();

        //private IDictionary<long, double> _clusteringWage = new Dictionary<long, double>();
        //private IDictionary<long, double> _betweennessWage = new Dictionary<long, double>();

        private double? _avgX;
        private double? _avgY;

        private double _scale = 1.3;
        private Point _moveStart;
        private Size _move = new Size(1, 1);
        private Size _move0;
        private double _scaledPoints;

        #endregion

        #region Constructor

        public MainForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            Logger.Logging += OnLogging;
        }

        #endregion

        #region Events

        private void OnLogging(string msg)
        {
            Invoke((Action) (() =>
            {
                label1.Text = msg;
                label1.Invalidate();
            }));
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            tb.Text = "asd";
            tb.Top = 100;
            Controls.Add(tb);
            await Task.Run((Action)PrepareData);
        }
        Label tb = new Label();

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _moveStart = e.Location;
            _move0 = _move;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if ((e.Button & MouseButtons.Left) != 0)
            {
                _move = _move0 + new Size(e.X - _moveStart.X, e.Y - _moveStart.Y);
                Invalidate();
            }

            if (Map?.IsReady ?? false)
            {
                var x = (-Width / 2 + e.X - _move.Width) / _scale / 1000 + _avgX;
                var y = (Height / 2 - e.Y + _move.Height) / _scale / 1545 + _avgY;
                tb.Text = x + "\r\n" + y;
                //(int)((node.Lon - _avgX) * 1000 * _scale),
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            const double factor = 1.5;

            base.OnMouseWheel(e);
            if (e.Delta > 0)
            {
                _move = new Size((int) (_move.Width * factor), (int) (_move.Height * factor));
                _scale *= factor;
            }

            if (e.Delta < 0)
            {
                _scale /= factor;
                _move = new Size((int) (_move.Width / factor), (int) (_move.Height / factor));
            }

            Invalidate();
        }

        #endregion

        #region Painting

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            if (Map == null || !Map.IsReady)
                return;

            PaintCore(e);
        }

        private void PaintCore(PaintEventArgs e)
        {
            var ss = Stopwatch.StartNew();
            s1.Start();

            CheckScale();

            var screenMove = new Size(Width / 2, -Height / 2) + _move;

            s2.Start();
            DrawRoads(e.Graphics, screenMove, Map.Ways.Where(w => w.OneWay), Color.Orange);
            DrawRoads(e.Graphics, screenMove, Map.Ways.Where(w => !w.OneWay), Color.DarkGoldenrod);
            DrawRoads(e.Graphics, screenMove, _deadWays, Color.Tomato);
            s2.Stop();

            //DrawDots(e.Graphics, screenMove, _clusteringWage, false);
            //DrawDots(e.Graphics, screenMove, _betweennessWage, true);

            s3.Start();
            DrawPath(e.Graphics, screenMove, Color.Black);
            s3.Stop();

            DrawDots(e.Graphics, screenMove);
            
            if (q.Count == 10)
                q.Dequeue();
            q.Enqueue(ss.Elapsed);

            e.Graphics.DrawString(q.Average(a => a.Milliseconds).ToString(), DefaultFont, new SolidBrush(Color.Black), 10, 10);
            //e.Graphics.DrawString(ss.Elapsed.ToString(), DefaultFont, new SolidBrush(Color.Black), 10, 10);
            
            s1.Stop();
        }
        

        Queue<TimeSpan> q = new Queue<TimeSpan>();

        private void CheckScale()
        {
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

                foreach (var node in _paths.SelectMany(n => n).ToList())
                {
                    node.Loc = new Point(
                        (int) ((node.Lon - _avgX) * 1000 * _scale),
                        (int) (Height - (node.Lat - _avgY) * 1545 * _scale));
                }
            }
        }

        Stopwatch s1 = new Stopwatch();
        Stopwatch s2 = new Stopwatch();
        Stopwatch s3 = new Stopwatch();
        Stopwatch s4 = new Stopwatch();
        Stopwatch s5 = new Stopwatch();
        Stopwatch s6 = new Stopwatch();
        
        private void DrawRoads(Graphics graphics, Size screenMove, IEnumerable<Way> ways, Color color)
        {
            var waysList = ways.ToList();
            var counter = waysList.Count;

            var pen = new Pen(color);
            var bag = new ConcurrentBag<Point[]>();
            
            Task.Run(() =>
            {
                foreach (var way in waysList)
                {
                    var points = SkipSame(way.Nodes, screenMove).ToArray();

                    bag.Add(points);
                }
            });
            
            while (counter > 0)
            {
                Point[] points;
                if (!bag.TryTake(out points))
                    continue;

                counter--;

                s4.Start();
                if (points.Length > 1)
                    graphics.DrawLines(pen, points);
                s4.Stop();
            }
        }

        private void DrawPath(Graphics graphics, Size screenMove, Color color)
        {
            var counter = _paths.Count;

            var pen = new Pen(color);
            var bag = new ConcurrentBag<Point[]>();
            
            if (checkBox1.Checked)
                Task.Run(() =>
                {
                    foreach (var path in _paths)
                    {
                        var points = SkipSame(path.Nodes, screenMove).ToArray();

                        bag.Add(points);
                    }
                });
            else
                foreach (var path in _paths)
                {
                    var points = SkipSame(path.Nodes, screenMove).ToArray();
                    bag.Add(points);
                }

            while (counter > 0)
            {
                Point[] points;
                if (!bag.TryTake(out points))
                    continue;

                counter--;

                s4.Start();
                if (points.Length > 1)
                    graphics.DrawLines(pen, points);
                s4.Stop();
            }

        }

        private IEnumerable<Point> SkipSame(IEnumerable<Node> nodes, Size screenMove)
        {
            if (checkBox1.Checked)
                return SkipSame1(nodes, screenMove);
                return SkipSame0(nodes, screenMove);
        }
        private IEnumerable<Point> SkipSame0(IEnumerable<Node> nodes, Size screenMove)
        {
            var l = new List<Point>();
            Point? previous = null;
            foreach (var point in nodes.Select(n => n.Loc + screenMove))
            {
                if (point != previous 
                    && point.X > -50 && point.Y > -50
                    && point.X < Width + 50 && point.Y < Height + 50)
                    //yield return point;
                l.Add(point);

                previous = point;
            }

            return l;
        }

        private IEnumerable<Point> SkipSame1(IEnumerable<Node> nodes, Size screenMove)
        {
            var l = new List<Point>();
            Point? previous = null;
            var previousAdded = false;

            foreach (var point in nodes.Select(n => n.Loc + screenMove))
            {
                if (point != previous
                    && point.X > -50 && point.Y > -50
                    && point.X < Width + 50 && point.Y < Height + 50)
                {
                    if (!previousAdded && previous.HasValue)
                        l.Add(previous.Value);

                    l.Add(point);

                    previousAdded = true;
                }
                else
                {
                    if (previousAdded)
                        l.Add(point);
                    else if (previous.HasValue && point != previous &&
                             (point.X > 0 && previous.Value.X < 0 ||
                              point.X < 0 && previous.Value.X > 0 ||
                              point.Y < 0 && previous.Value.Y > 0 ||
                              point.Y > 0 && previous.Value.Y < 0))
                    {
                        //if (!previousAdded && previous.HasValue)
                        //l.Add(previous.Value);

                        l.Add(point);

                        //previousAdded = true;
                        previous = point;
                        continue;
                    }

                    previousAdded = false;
                }

                previous = point;
            }

            return l;
        }

        //private IEnumerable<Point> SkipSame2(IEnumerable<Node> nodes, Size screenMove)
        //{
        //    var l = new List<Point>();
        //    Point? previous = null;
        //    var previousAdded = false;

        //    foreach (var point in nodes.Select(n => n.Loc + screenMove))
        //    {
        //        if (point != previous
        //            && point.X > -50 && point.Y > -50
        //            && point.X < Width + 50 && point.Y < Height + 50)
        //            //yield return point;
        //        {
        //            if (!previousAdded && previous.HasValue)
        //                l.Add(previous.Value);

        //            l.Add(point);

        //            previousAdded = true;
        //        }
        //        else
        //        {
        //            if (previousAdded)
        //                l.Add(point);

        //            previousAdded = false;
        //        }

        //        previous = point;
        //    }

        //    return l;
        //}

        //private void DrawDots(Graphics graphics, Size screenMove, IDictionary<long, double> wages, bool convertValue)
        //{
        //    foreach (var pair in wages)
        //    {
        //        var node = Map.AllNodes[pair.Key];

        //        var v = convertValue
        //            ? Math.Log(1 + pair.Value) * 2.5
        //            : pair.Value;

        //        // ReSharper disable once CompareOfFloatsByEqualityOperator
        //        var size = pair.Value == 0 ? 5 : 10;

        //        var color = GetColor(v);

        //        graphics.FillEllipse(new SolidBrush(color), node.Loc.X + screenMove.Width - 3,
        //            node.Loc.Y + screenMove.Height - 3, size, size);
        //    }
        //}

        private void DrawDots(Graphics graphics, Size screenMove)
        {
            foreach (var hub in Hub.GetWaveloHubs())
            {
                graphics.FillEllipse(new SolidBrush(Color.DeepPink), 
                    (int)((hub.Lon - _avgX.Value) * 1000 * _scale + screenMove.Width - 3),
                    (int)(Height - (hub.Lat - _avgY.Value) * 1545 * _scale + screenMove.Height - 3), 
                    6, 6);
            }
        }

        private static Color GetColor(double v)
        {
            var r = v < 1.0 ? MinMax(v * 1020 - 510) : MinMax(v * -1020 + 1275);
            var g = v < 0.5 ? MinMax(v * 1020) : MinMax(v * -1020 + 1020);
            var b = MinMax(v * -1020 + 510);

            var color = Color.FromArgb(r, g, b);
            return color;
        }

        private static int MinMax(double value)
        {
            var intValue = (int) value;
            return intValue > 255
                ? 255
                : (intValue < 0 ? 0 : intValue);
        }

        #endregion

        private void PrepareData()
        {
            Map = MapLoader.LoadFromFile(DataFile);

            _deadWays = Map.RemoveDeadNodes();

            //if (UseClusteringCoefficient)
            //    _clusteringWage = CentralityCalculator.CalcClusteringCoefficient(Map);

            //Map.Simplify();

            Map.IsReady = true;

            //Invalidate();

            //if (UseBetweennessCentrality)
            //    CentralityCalculator.CalcBetweennessCentrality(Map, BetweennessCentralityIterations, out _betweennessWage);

            PrepareGpx();
            Invalidate();
        }




        private void PrepareGpx()
        {
            Adfdsf();
            var dirSource = File.ReadAllLines(@"..\..\..\Path.txt").First();
            foreach (var file in Directory.GetFiles(dirSource, "*.gpx"))
            {
                var path = ReadGpxFile(file);
                if (path.Count() > 1)
                {
                    _paths.Add(path);
                    _distances.Add(file, path.Length);
                }
            }

            File.WriteAllLines(@"C:\Users\Albert\Desktop\Nowy folder\Wavelo\stats.txt",
                _paths.OrderBy(p => p.Id).Select(p => p.ToString()));

            Logger.Log("");
        }

        private static Path ReadGpxFile(string filePath)
        {
            var nodes = FileParser.ParseXml(filePath)["trk"]["trkseg"].Nodes;

            return new Path(nodes.Select(n =>
                new TimeNode
                {
                    Lat = double.Parse(n.Attributes["lat"], new NumberFormatInfo()),
                    Lon = double.Parse(n.Attributes["lon"], new NumberFormatInfo()),
                    Time = DateTime.Parse(n["time"].Value)
                })) {FilePath = filePath};
        }

        private void DrawPath0(Graphics graphics, Size screenMove, Color color)
        {
            var pen = new Pen(color);
            foreach (var path in _paths)
            {
                var source = SkipSame(path.Nodes, screenMove).ToArray();

                if (source.Length > 1)
                    graphics.DrawLines(pen, source);
            }
        }

        private static IEnumerable<Point> SkipSame(IEnumerable<Point> points)
        {
            Point? previous = null;
            foreach (var point in points)
            {
                if (point != previous)
                    yield return point;

                previous = point;
            }
        }

        private readonly ICollection<Path> _paths = new List<Path>();
        private readonly IDictionary<string, double> _distances = new Dictionary<string, double>();

        private void Adfdsf()
        {
            //var p = @"C:\Users\Albert\Desktop\Nowy folder\Wavelo\sobi_ride_11086082_fix.gpx";
            //var aa = ReadGpxFile(p).ToList();

            //double length = GetLength(aa);

            //length = 0;
        }


    }

    public class Path : IEnumerable<Node>
    {
        public IList<Node> Nodes { get; }
        public string FilePath { get; set; }
        public double Length => GetLength(Nodes);
        public int Id => int.Parse(string.Concat(FilePath.SkipWhile(c => !char.IsDigit(c)).TakeWhile(char.IsDigit)));

        public TimeSpan Duration => ((TimeNode) Nodes.Last()).Time - ((TimeNode) Nodes.First()).Time;
        public DateTime TimeStart => ((TimeNode)Nodes.First()).Time;
        public DateTime TimeEnd => ((TimeNode)Nodes.Last()).Time;
        //public string Start => Area.Get().FirstOrDefault(a => a.IsBetween(Nodes.First()))?.Name; 
        //public string Stop => Area.Get().FirstOrDefault(a => a.IsBetween(Nodes.Last()))?.Name; 
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
               //(TimeStart.Date == TimeEnd.Date
               //    ? TimeEnd.ToLongTimeString()
               //    : TimeEnd.ToString());

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
