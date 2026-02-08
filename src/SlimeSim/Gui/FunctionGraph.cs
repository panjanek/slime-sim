using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SlimeSim.Models;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace SlimeSim.Gui
{
    public class FunctionGraph : Canvas
    {
        private List<Stats> stats;

        private List<StatsSeries> series;

        private TextBlock text;

        private Ellipse circle;

        private double selectedTime;

        private StatsSeries selectedSerie;

        public void Draw(List<Stats> stats)
        {
            this.stats = stats;
            Redraw();
        }

        public void UpdateSeries(List<StatsSeries> series)
        {
            this.series = series;
            Redraw();
        }

        public void Redraw()
        {
            if (series == null || series.Count == 0 || stats == null || stats.Count < 2)
                return;

            if (text == null || circle == null)
            {
                text = CanvasUtil.AddTextBlock(this, 5, 5, 14, Brushes.Black, Brushes.White);
                text.Visibility = System.Windows.Visibility.Collapsed;
                circle = CanvasUtil.AddEllipse(this, 0, 0, 10, 10, 2, Brushes.White, Brushes.Transparent);
                circle.Visibility = System.Windows.Visibility.Collapsed;
            }

            try
            {
                var configWindow = Window.GetWindow(this) as ConfigWindow;
                int historyCount = configWindow?.GraphHistory ?? 100;
                bool commonScale = configWindow?.GraphCommonScale ?? false;


                var toDraw = stats.OrderBy(s => s.time).ToList();
                if (toDraw.Count > historyCount)
                    toDraw = toDraw.Skip(toDraw.Count - historyCount).ToList();

                var width = ActualWidth;
                var height = ActualHeight;
                Children.Clear();
                Children.Add(text);
                Children.Add(circle);
                Background = Brushes.Black;
                ClipToBounds = true;

                double minY = series.Min(serie => toDraw.Select(x => serie.Selector(x)).Min());
                double maxY = series.Max(serie => toDraw.Select(x => serie.Selector(x)).Max());
                double dotSize = 6;
                foreach (var serie in series)
                {
                    if (!commonScale)
                    {
                        minY = toDraw.Select(x => serie.Selector(x)).Min();
                        maxY = toDraw.Select(x => serie.Selector(x)).Max();
                    }
                    
                    var dy = maxY - minY;
                    maxY += dy * 0.1;
                    minY -= dy * 0.1;
                    dy = maxY - minY;
                    double scaleY = dy > 0.0000001 ? height / dy : height / 0.0000001;


                    double scaleX = width / (toDraw.Count-1);
                    
                    for (int i = 0; i < toDraw.Count; i++)
                    {
                        var s1 = toDraw[i];
                        var x1 = i * scaleX;
                        var y1 = serie.Selector(s1);
                        var dot = CanvasUtil.AddEllipse(this, x1- dotSize/2, height - (y1 - minY) * scaleY- dotSize/2, dotSize, dotSize, 0, Brushes.Transparent, serie.Style.Stroke, null, 1);

                        if (selectedSerie == serie && selectedTime == s1.time)
                        {
                            circle.SetValue(Canvas.LeftProperty, x1 - 5);
                            circle.SetValue(Canvas.TopProperty, height - (y1 - minY) * scaleY - 5);
                            circle.Visibility = System.Windows.Visibility.Visible;
                        }

                        string info = serie.Name + ": " + y1.ToString("0.00000", CultureInfo.InvariantCulture);
                        var time = toDraw[i].time;
                        var currentSerie = serie;
                        dot.MouseDown += (s, e) =>
                        {
                            text.Text = info;
                            text.Foreground = serie.Style.Stroke;
                            text.Visibility = System.Windows.Visibility.Visible;
                            selectedTime = time;
                            selectedSerie = currentSerie;
                            /*
                            circle.SetValue(Canvas.LeftProperty, x1 - 5);
                            circle.SetValue(Canvas.TopProperty, height - (y1 - minY) * scaleY - 5);
                            circle.Visibility = System.Windows.Visibility.Visible;
                            */
                            Redraw();
                            e.Handled = true;
                        };

                        if (i < toDraw.Count - 1)
                        {
                            var s2 = toDraw[i + 1];
                            var x2 = (i + 1) * scaleX;
                            var y2 = serie.Selector(s2);
                            var line = CanvasUtil.AddStyledLine(this, x1, height - (y1 - minY) * scaleY, x2, height - (y2 - minY) * scaleY, serie.Style, null, 2);
                        }
                    }


                    

                    /*
                    var b = serie.line as SolidColorBrush;
                    var axisY = height + minY * scaleY;
                    Line axis = new Line();
                    axis.Stroke = new SolidColorBrush(Color.FromArgb(128, b.Color.R, b.Color.G, b.Color.B));
                    axis.StrokeThickness = 2;
                    axis.X1 = 0;
                    axis.Y1 = axisY;
                    axis.X2 = width;
                    axis.Y2 = axisY;
                    Children.Add(axis);*/
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class StatsSeries
    {
        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public SeriesStyle Style { get; set; }

        public Func<Stats, double> Selector { get; set; }
    }

    public class SeriesStyle
    {
        public Brush Stroke { get; set; }
        public double StrokeThickness { get; set; }
        public DoubleCollection StrokeDashArray { get; set; }
        public PenLineCap LineCap { get; set; }
    }
}
