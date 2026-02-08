using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace SlimeSim.Gui
{
    public static class CanvasUtil
    {
        public static TextBlock AddTextBlock(Canvas canvas, double left, double top, double fontSize,  Brush back, Brush fore, int? zIndex = null)
        {
            var txt = new TextBlock() { FontSize = fontSize, Background = back, Foreground = fore };
            txt.SetValue(Canvas.LeftProperty, left);
            txt.SetValue(Canvas.TopProperty, top);
            if (zIndex.HasValue)
                txt.SetValue(Canvas.ZIndexProperty, zIndex);
            canvas.Children.Add(txt);
            return txt;
        }

        public static Line AddLine(Canvas canvas, double x1, double y1, double x2, double y2, double thickness, Brush stroke, object tag = null, int? zIndex = null)
        {
            Line line = new Line() { StrokeThickness = thickness, Stroke = stroke, X1 = x1, Y1 = y1, X2 = x2, Y2 = y2 };
            if (zIndex.HasValue)
                line.SetValue(Canvas.ZIndexProperty, zIndex.Value);
            if (tag != null)
                line.Tag = tag;
            canvas.Children.Add(line);
            return line;
        }

        public static Ellipse AddEllipse(Canvas canvas, double left, double top, double width, double height, double thickness, Brush stroke, Brush fill, object tag = null, int? zIndex = null)
        {
            Ellipse el = new Ellipse() { Fill = fill, Stroke = stroke, StrokeThickness = thickness, Width = width, Height = height };
            el.SetValue(Canvas.LeftProperty, left);
            el.SetValue(Canvas.TopProperty, top);
            if (zIndex.HasValue)
                el.SetValue(Canvas.ZIndexProperty, zIndex.Value);
            if (tag != null)
                el.Tag = tag;
            canvas.Children.Add(el);
            return el;
        }
    }
}
