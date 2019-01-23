using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace EksploracjaDanych
{
    public partial class MainForm : Form
    {
        #region Constants

        private const string File = @"..\..\..\data\set5.json";
        private const bool UseClusteringCoefficient = false;
        private const bool UseBetweennessCentrality = true;
        private const int BetweennessCentralityIterations = 1000;

        #endregion

        #region Fields

        private Map Map { get; set; }

        private ICollection<Way> _deadWays = new List<Way>();

        private IDictionary<long, double> _clusteringWage = new Dictionary<long, double>();
        private IDictionary<long, double> _betweennessWage = new Dictionary<long, double>();
        
        private double? _avgX;
        private double? _avgY;

        private double _scale = 1.3;
        private Point _moveStart;
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
            Invoke((Action)(() =>
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

        #endregion

        #region Painting

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Map == null || !Map.IsReady)
                return;

            CheckScale();

            var screenMove = new Size(Width / 2, -Height / 2) + _move;

            DrawRoads(e.Graphics, screenMove, Map.Ways, Color.DarkGreen);
            DrawRoads(e.Graphics, screenMove, _deadWays, Color.Tomato);

            DrawDots(e.Graphics, screenMove, _clusteringWage, false);
            DrawDots(e.Graphics, screenMove, _betweennessWage, true);
        }

        private void CheckScale()
        {
            _avgX = _avgX ?? (_avgX = Map.Nodes.Average(n => n.Value.Lon));
            _avgY = _avgY ?? (_avgY = Map.Nodes.Average(n => n.Value.Lat));

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_scaledPoints != _scale)
            {
                _scaledPoints = _scale;

                foreach (var node in Map.AllNodes.Values)
                {
                    node.Loc = new Point(
                        (int)((node.Lon - _avgX) * 1000 * _scale),
                        (int)(Height - (node.Lat - _avgY) * 1545 * _scale));
                }
            }
        }

        private void DrawRoads(Graphics graphics, Size screenMove, IEnumerable<Way> ways, Color color)
        {
            var pen = new Pen(color);
            foreach (var way in ways.Where(way => way.Nodes.Count >= 2))
            {
                var source = way.Nodes.Select(n => Map.AllNodes[n].Loc + screenMove).ToArray();

                graphics.DrawLines(pen, source);
            }
        }

        private void DrawDots(Graphics graphics, Size screenMove, IDictionary<long, double> wages, bool convertValue)
        {
            foreach (var pair in wages)
            {
                var node = Map.AllNodes[pair.Key];

                var v = convertValue
                    ? Math.Log(1 + pair.Value) * 2.5
                    : pair.Value;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                var size = pair.Value == 0 ? 5 : 10;

                var color = GetColor(v);
                
                graphics.FillEllipse(new SolidBrush(color), node.Loc.X + screenMove.Width-3, node.Loc.Y + screenMove.Height-3, size, size);
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
            var intValue = (int)value;
            return intValue > 255
                ? 255
                : (intValue < 0 ? 0 : intValue);
        }

        #endregion
        
        private void PrepareData()
        {
            Map = MapLoader.LoadFromFile(File);
            _deadWays = Map.RemoveDeadNodes();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (UseClusteringCoefficient)
                _clusteringWage = CentralityCalculator.CalcClusteringCoefficient(Map);

            //Map.Simplify();
            
            Map.IsReady = true;

            Invalidate();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (UseBetweennessCentrality)
                CentralityCalculator.CalcBetweennessCentrality(Map, BetweennessCentralityIterations, out _betweennessWage);

            Invalidate();
        }
    }
}
