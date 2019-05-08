using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using Albertl;
using Albertl.FileParsing;

namespace GpxPathViewer
{
    public partial class MainForm : Form
    {
        #region Constants

        private const string DataFile = @"..\..\..\data\set4.json";

        #endregion

        #region Fields

        private Map Map { get; set; }

        private double? _avgX;
        private double? _avgY;

        private double _scale = 1000;
        private Point _moveStartLocation;
        private Size _moveStartPoint;
        private Size _move = new Size(1, 1);
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
                _logLabel.Text = msg;
                _logLabel.Invalidate();
            }));
        }

        protected override async void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            await Task.Run((Action)PrepareData);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _moveStartLocation = e.Location;
            _moveStartPoint = _move;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if ((e.Button & MouseButtons.Left) != 0)
            {
                _move = _moveStartPoint + new Size(e.X - _moveStartLocation.X, e.Y - _moveStartLocation.Y);
                Invalidate();
            }

            if (Map?.IsReady ?? false)
            {
                var x = (-Width / 2 + e.X - _move.Width) / _scale + _avgX;
                var y = (Height / 2 - e.Y + _move.Height) / _scale / _latLonFactor + _avgY;
                _logLabel.Text = x + "\r\n" + y;
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
            s2.Stop();

            s3.Start();
            DrawPath(e.Graphics, screenMove, Color.Black);
            s3.Stop();

            DrawDots(e.Graphics, screenMove);
            
            if (q.Count == 10)
                q.Dequeue();
            q.Enqueue(ss.Elapsed);

            e.Graphics.DrawString(q.Average(a => a.Milliseconds).ToString(), DefaultFont, new SolidBrush(Color.Black), 10, 50);
            //e.Graphics.DrawString(ss.Elapsed.ToString(), DefaultFont, new SolidBrush(Color.Black), 10, 10);
            
            s1.Stop();
        }
        

        Queue<TimeSpan> q = new Queue<TimeSpan>();
        private double _latLonFactor;
        private void CheckScale()
        {
            _avgX = _avgX ?? (_avgX = Map.Nodes.Average(n => n.Value.Lon));
            _avgY = _avgY ?? (_avgY = Map.Nodes.Average(n => n.Value.Lat));


            if (_latLonFactor <= 0)
            {
                var node = Map.Nodes.Values.First();
                var latLength = Node.DistanceTo(node.Lat, node.Lon, node.Lat + 0.01, node.Lon);
                var lonLength = Node.DistanceTo(node.Lat, node.Lon, node.Lat, node.Lon + 0.01);
                _latLonFactor = latLength / lonLength;
            }


            if (_scaledPoints != _scale)
            {
                _scaledPoints = _scale;

                foreach (var node in Map.AllNodes.Values)
                {
                    node.Loc = new Point(
                        (int) ((node.Lon - _avgX) * _scale),
                        (int) (Height - (node.Lat - _avgY) * _scale * _latLonFactor));
                }

                foreach (var node in _paths.SelectMany(n => n).ToList())
                {
                    node.Loc = new Point(
                        (int) ((node.Lon - _avgX) * _scale),
                        (int) (Height - (node.Lat - _avgY) * _scale * _latLonFactor));
                }
            }
        }

        Stopwatch s1 = new Stopwatch();
        Stopwatch s2 = new Stopwatch();
        Stopwatch s3 = new Stopwatch();
        Stopwatch s4 = new Stopwatch();
        
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

        private void DrawDots(Graphics graphics, Size screenMove)
        {
            foreach (var hub in Hub.GetWaveloHubs())
            {
                graphics.FillEllipse(new SolidBrush(Color.DeepPink), 
                    (int)((hub.Lon - _avgX.Value) * _scale + screenMove.Width - 3),
                    (int)(Height - (hub.Lat - _avgY.Value) * _scale * _latLonFactor + screenMove.Height - 3), 
                    6, 6);
            }
        }

        #endregion

        private void PrepareData()
        {
            Map = MapLoader.LoadFromFile(DataFile);

            Map.IsReady = true;

            PrepareGpx();
            Invalidate();
        }


        private void PrepareGpx()
        {
            var dirSource = File.ReadLines(@"..\..\..\Path.txt").First();
            _paths.AddRange(Directory.GetFiles(dirSource, "*.gpx").Select(ReadGpxFile));

            //File.WriteAllLines(@"C:\Users\Albert\Desktop\Nowy folder\Wavelo\stats.txt",
            //    _paths.OrderBy(p => p.Id).Select(p => p.ToString()));

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
    }
}
