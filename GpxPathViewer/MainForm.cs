using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        private const string DataFile = @"..\..\..\data\set5.json";
        private const int ImageSize = 1024;
        private const double ScaleChangeFactor = 1.5;
        private const double ScaleFactorBase = 1000;

        #endregion

        #region Fields

        private double _avgX;
        private double _avgY;

        private double _latLonFactor;

        private double _scaleFactor = ScaleFactorBase;

        private int _scale;

        private Point _moveStartLocation;
        private Size _moveStartPoint;
        private Size _move;
        private int _pointsScale;

        private readonly ICollection<Path> _paths = new List<Path>();

        private readonly IDictionary<Tuple<int, Point>, Image> _images = new ConcurrentDictionary<Tuple<int, Point>, Image>();

        private readonly Queue<TimeSpan> _timeQueue = new Queue<TimeSpan>();

        //Stopwatch s1 = new Stopwatch();
        //Stopwatch s2 = new Stopwatch();
        //Stopwatch s3 = new Stopwatch();
        //Stopwatch s4 = new Stopwatch();
        //Stopwatch s5 = new Stopwatch();

        #endregion


        #region Properties

        private Map Map { get; set; }

        private int Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                _scaleFactor = Math.Pow(ScaleChangeFactor, _scale) * ScaleFactorBase;
            }
        }

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
            
            _move = new Size(ClientSize.Width / 2, ClientSize.Height / 2);

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
                var x = (-ClientSize.Width / 2 + e.X - _move.Width) / _scaleFactor + _avgX;
                var y = (ClientSize.Height / 2 - e.Y + _move.Height) / _scaleFactor / _latLonFactor + _avgY;
                _logLabel.Text = x + Environment.NewLine + y;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0)
            {
                _move -= new Size(ClientSize.Width / 2, ClientSize.Height / 2);
                
                _move = new Size((int) (_move.Width * ScaleChangeFactor), (int) (_move.Height * ScaleChangeFactor));

                Scale++;

                _move += new Size(ClientSize.Width / 2, ClientSize.Height / 2);
            }

            if (e.Delta < 0)
            {
                _move -= new Size(ClientSize.Width / 2, ClientSize.Height / 2);

                Scale--;

                _move = new Size((int) (_move.Width / ScaleChangeFactor), (int) (_move.Height / ScaleChangeFactor));

                _move += new Size(ClientSize.Width / 2, ClientSize.Height / 2);
            }


            Invalidate();
        }

        #endregion

        #region Painting

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Map != null && Map.IsReady)
                CheckScale();

            var ss = Stopwatch.StartNew();

            Parallel.ForEach(GetVisibleLocations(), PaintLocation);
            //GetVisibleLocations().ForEach(PaintLocation);

            foreach (var value in _images.Where(v => v.Key.Item1 == _scale))
            {
                lock (value.Value)
                    e.Graphics.DrawImage(value.Value,
                        _move.Width + value.Key.Item2.X * ImageSize,
                        _move.Height + value.Key.Item2.Y * ImageSize);
            }

            PrintAveragePaintTime(e, ss);
        }

        private void PrintAveragePaintTime(PaintEventArgs e, Stopwatch ss)
        {
            if (_timeQueue.Count == 10)
                _timeQueue.Dequeue();
            _timeQueue.Enqueue(ss.Elapsed);
            e.Graphics.DrawString(_timeQueue.Average(a => a.Milliseconds).ToString(CultureInfo.InvariantCulture),
                DefaultFont, new SolidBrush(Color.Black), 10, 50);
        }

        private IEnumerable<Point> GetVisibleLocations()
        {
            var fromX = -(int)Math.Ceiling(_move.Width / (double)ImageSize);
            var fromY = -(int)Math.Ceiling(_move.Height / (double)ImageSize);
            var toX = -(int)Math.Ceiling((_move.Width - ClientSize.Width) / (double)ImageSize);
            var toY = -(int)Math.Ceiling((_move.Height - ClientSize.Height) / (double)ImageSize);

            return Enumerable.Range(fromX, toX - fromX + 1)
                .SelectMany(x => Enumerable.Range(fromY, toY - fromY + 1), (x, y) => new Point(x, y));
        }


        private void PaintLocation(Point location)
        {
            var key = Tuple.Create(_scale, location);

            if (_images.ContainsKey(key)) return;

            var fileName = $"tmp\\{_scale} {location.X} {location.Y}.png";

            if (File.Exists(fileName))
            {
                _images[key] = Image.FromFile(fileName);
            }
            else
            {
                if (Map == null || !Map.IsReady)
                    return;

                var image = PaintArea(location);

                _images[key] = image;

                Task.Run(() =>
                {
                    lock (image) image.Save(fileName);
                });
            }
        }


        private Image PaintArea(Point location)
        {
            var image = new Bitmap(ImageSize, ImageSize);
            var graphics = Graphics.FromImage(image);

            if (checkBox1.Checked)
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var screenMove = new Size(location.X * ImageSize, location.Y * ImageSize);

            DrawRoads(graphics, screenMove);

            DrawPath(graphics, screenMove, Color.Black);

            if (Scale > 5)
                DrawDots(graphics, screenMove);
            
            return image;
        }

        private void CheckScale()
        {
            if (_avgX == 0)
            {
                _avgX = Map.Nodes.Average(n => n.Value.Lon);
                _avgY = Map.Nodes.Average(n => n.Value.Lat);
            }

            if (_latLonFactor == 0)
            {
                var node = Map.Nodes.Values.First();
                var latLength = Node.DistanceTo(node.Lat, node.Lon, node.Lat + 0.01, node.Lon);
                var lonLength = Node.DistanceTo(node.Lat, node.Lon, node.Lat, node.Lon + 0.01);
                _latLonFactor = latLength / lonLength;
            }


            if (_pointsScale != _scaleFactor)
            {
                _pointsScale = Scale;

                foreach (var node in Map.AllNodes.Values)
                {
                    node.Loc = new Point(
                        (int) ((node.Lon - _avgX) * _scaleFactor),
                        (int) (-(node.Lat - _avgY) * _scaleFactor * _latLonFactor));
                }

                foreach (var node in _paths.SelectMany(n => n).ToList())
                {
                    node.Loc = new Point(
                        (int) ((node.Lon - _avgX) * _scaleFactor),
                        (int) (-(node.Lat - _avgY) * _scaleFactor * _latLonFactor));
                }
            }
        }

        private void DrawRoads(Graphics graphics, Size screenMove)
        {
            var pens = new Dictionary<Color, Pen>();
            
            foreach (var way in Map.Ways)
            {
                var points = SkipSame(way.Nodes, screenMove).ToArray();

                if (points.Length < 2)
                    continue;
                var color = Color.DarkGoldenrod;
                if (way.OneWay)
                    color = Color.Orange;
                if (way.Tags.ContainsKey("highway"))
                {
                    if (way.Tags["highway"].StartsWith("motorway"))
                    {
                        color = Color.MediumVioletRed;
                    }
                    else if (way.Tags["highway"].StartsWith("primary"))
                    {
                        color = Color.Fuchsia;
                    }
                    else if (way.Tags["highway"].StartsWith("trunk"))
                    {
                        if (Scale < 0)
                            continue;

                        color = Color.DarkOrchid;
                    }
                    else if (way.Tags["highway"].StartsWith("secondary"))
                    {
                        if (Scale < 1)
                            continue;

                        color = Color.DarkOrchid;
                    }
                    else if (way.Tags["highway"].StartsWith("tertiary"))
                    {
                        if (Scale < 3)
                            continue;

                        color = Color.DarkBlue;
                    }
                    else if (way.Tags["highway"].StartsWith("residential"))
                    {
                        if (Scale < 6)
                            continue;

                        color = Color.ForestGreen;
                    }
                    else if (Scale < 7)
                        continue;
                }

                if (!pens.ContainsKey(color))
                    pens[color] = new Pen(color);

                    graphics.DrawLines(pens[color], points);
            }
        }

        //private void DrawRoads(Graphics graphics, Size screenMove, IEnumerable<Way> ways, Color color)
        //{
        //    var pen = new Pen(color);

        //    foreach (var way in ways.ToList())
        //    {
        //        var points = SkipSame(way.Nodes, screenMove).ToArray();

        //        if (points.Length > 1)
        //            graphics.DrawLines(pen, points);
        //    }
        //}

        private void DrawPath(Graphics graphics, Size screenMove, Color color)
        {
            var pen = new Pen(color);

            foreach (var path in _paths)
            {
                var points = SkipSame(path.Nodes, screenMove).ToArray();
                if (points.Length > 1)
                    graphics.DrawLines(pen, points);
            }
        }

        private static IEnumerable<Point> SkipSame(IEnumerable<Node> nodes, Size screenMove)
        {
            var list = new List<Point>();
            Point? previous = null;

            foreach (var point in nodes.Select(n => n.Loc - screenMove))
            {
                if (point != previous)
                    list.Add(point);

                previous = point;
            }

            return list;
        }

        private void DrawDots(Graphics graphics, Size screenMove)
        {
            foreach (var hub in Hub.GetWaveloHubs())
            {
                graphics.FillEllipse(new SolidBrush(Color.DeepPink), 
                    (int)((hub.Lon - _avgX) * _scaleFactor - screenMove.Width - 3),
                    (int)(- (hub.Lat - _avgY) * _scaleFactor * _latLonFactor - screenMove.Height - 3), 
                    6, 6);
            }
        }

        #endregion

        private void PrepareData()
        {
            Map = MapLoader.LoadFromFile(DataFile);

            PrepareGpx();

            Map.IsReady = true;

            Invalidate();
        }


        private void PrepareGpx()
        {
            Logger.Log("Reading GPX files");

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
    }
}
