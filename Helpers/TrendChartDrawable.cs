using Microsoft.Maui.Graphics;

namespace Venther.App.Helpers;

public class TrendChartDrawable : IDrawable
{
    public IReadOnlyList<double> Values { get; set; } = Array.Empty<double>();

    private static readonly Color Line = Color.FromArgb("#00326F");
    private static readonly Color Dot = Color.FromArgb("#FC6100");
    private static readonly Color Grid = Color.FromArgb("#C3C6D3");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (Values.Count < 2) return;

        const float padX = 8f, padTop = 16f, padBottom = 8f;
        float w = dirtyRect.Width - padX * 2;
        float h = dirtyRect.Height - padTop - padBottom;
        double min = Values.Min(), max = Values.Max();
        double range = Math.Max(max - min, 0.1);

        PointF P(int i) => new(
            padX + w * i / (Values.Count - 1),
            padTop + h * (float)(1 - (Values[i] - min) / range));

        // Grade horizontal tracejada
        canvas.StrokeColor = Grid;
        canvas.StrokeSize = 1;
        canvas.StrokeDashPattern = new float[] { 3, 4 };
        for (int g = 0; g < 3; g++)
        {
            float y = padTop + h * g / 2f;
            canvas.DrawLine(padX, y, padX + w, y);
        }
        canvas.StrokeDashPattern = null;

        // Curva suave (Catmull-Rom convertida em Bézier)
        var pts = Enumerable.Range(0, Values.Count).Select(P).ToArray();
        var line = new PathF(pts[0]);
        for (int i = 0; i < pts.Length - 1; i++)
        {
            var p0 = i > 0 ? pts[i - 1] : pts[i];
            var p1 = pts[i];
            var p2 = pts[i + 1];
            var p3 = i + 2 < pts.Length ? pts[i + 2] : p2;
            var c1 = new PointF(p1.X + (p2.X - p0.X) / 6f, p1.Y + (p2.Y - p0.Y) / 6f);
            var c2 = new PointF(p2.X - (p3.X - p1.X) / 6f, p2.Y - (p3.Y - p1.Y) / 6f);
            line.CurveTo(c1, c2, p2);
        }

        // Área com gradiente sob a curva
        var fill = new PathF(line);
        fill.LineTo(pts[^1].X, padTop + h);
        fill.LineTo(pts[0].X, padTop + h);
        fill.Close();
        var gradient = new LinearGradientPaint(
            new[]
            {
                new PaintGradientStop(0f, Line.WithAlpha(0.28f)),
                new PaintGradientStop(1f, Line.WithAlpha(0f))
            },
            new Point(0, 0), new Point(0, 1));
        canvas.SetFillPaint(gradient, new RectF(0, padTop, dirtyRect.Width, h));
        canvas.FillPath(fill);

        // Linha
        canvas.StrokeColor = Line;
        canvas.StrokeSize = 2;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawPath(line);

        // Ponto final destacado (laranja com halo)
        var last = pts[^1];
        canvas.FillColor = Dot.WithAlpha(0.25f);
        canvas.FillCircle(last.X, last.Y, 7f);
        canvas.FillColor = Dot;
        canvas.FillCircle(last.X, last.Y, 4f);
    }
}