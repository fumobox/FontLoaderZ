using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace FontLoaderZ
{
    public static class GlyphContourConverter
    {

        public static GlyphContourInfoV2 ToVector2(this GlyphContourInfo g, FontModel model, FontSource source)
        {
            switch (model.Type)
            {
                case FontModel.FontType.TrueType:
                    return FromTtfGlyph(g, model, source);
                case FontModel.FontType.OpenType:
                    return FromOtfGlyph(g, model, source);
                default:
                    throw new InvalidEnumArgumentException(model.Type.ToString());
            }
        }

        private static GlyphContourInfoV2 FromTtfGlyph(GlyphContourInfo g, FontModel model, FontSource source)
        {
            var advancedWidth = g.BaseInfo.AdvancedWidth * model.BaseScale;
            var leftSideBearing = g.BaseInfo.LeftSideBearing * model.BaseScale;

            if (g.Header.NumberOfContours > 0)
            {
                var con = new List<List<ContourPoint>>();

                var points = g.Header.GetGlyphPoints(source);
                if (points == null || points.Length == 0) throw new FontException("Cannot get points");

                var list = new List<ContourPoint>();
                con.Add(list);
                for (var i = 0; i < points.Length; i++)
                {
                    var point = points[i];
                    var contourPoint = new ContourPoint(point.X, point.Y, point.OnCurve);
                    list.Add(contourPoint);

                    if (point.IsEndPoint && i != points.Length - 1)
                    {
                        list = new List<ContourPoint>();
                        con.Add(list);
                    }
                }

                return new GlyphContourInfoV2(advancedWidth, leftSideBearing, ContourToVector(con, model.BaseScale, true));
            }
            else
            {
                // Composite Glyph
                var components = g.Header.GetGlyphComponents(source);

                var con = new List<List<ContourPoint>>();

                foreach (var component in components)
                {
                    var cg = model.GetGlyphFromGid(component.GlyphId, source);
                    if (cg.Header.NumberOfContours > 0)
                    {
                        var points = cg.Header.GetGlyphPoints(source);
                        if (points == null || points.Length == 0) throw new FontException("Cannot get points");

                        if (component.MatchedPoints == null)
                            GlyphTable.GlyphHeader.GlyphComponent.TransformPoints(points, component);
                        else
                            // MatchedPoints are not supported.
                            break;

                        var list = new List<ContourPoint>();
                        con.Add(list);
                        for (var i = 0; i < points.Length; i++)
                        {
                            var point = points[i];

                            var contourPoint = new ContourPoint(point.X, point.Y, point.OnCurve);
                            list.Add(contourPoint);

                            if (point.IsEndPoint && i != points.Length - 1)
                            {
                                list = new List<ContourPoint>();
                                con.Add(list);
                            }
                        }
                    }
                }
                return new GlyphContourInfoV2(advancedWidth, leftSideBearing, ContourToVector(con, model.BaseScale, true));
            }
        }

        private static GlyphContourInfoV2 FromOtfGlyph(GlyphContourInfo g, FontModel model, FontSource source)
        {
            var advancedWidth = g.BaseInfo.AdvancedWidth * model.BaseScale;
            var leftSideBearing = g.BaseInfo.LeftSideBearing * model.BaseScale;

            var contours = new List<List<ContourPoint>>();
            List<ContourPoint> contour = null;
            foreach (var cp in g.Path.Points)
            {
                if (cp.isContour)
                {
                    contour = new List<ContourPoint>();
                    contours.Add(contour);
                }

                if (cp.isCurve)
                    contour.Add(new ContourPoint((short) cp.x, (short) cp.y, false));
                else
                    contour.Add(new ContourPoint((short) cp.x, (short) cp.y, true));
            }

            foreach (var c in contours)
            {
                var first = c[0];
                var last = c[c.Count - 1];
                if (first.X == last.X && first.Y == last.Y)
                    c.RemoveAt(c.Count - 1);
            }

            return new GlyphContourInfoV2(advancedWidth, leftSideBearing, ContourToVector(contours, model.BaseScale, false));
        }

        private static Vector2[][] ContourToVector(IReadOnlyList<List<ContourPoint>> con, float baseScale, bool splitContour)
        {
            var vContours = new Vector2[con.Count][];
            for (var i = 0; i < con.Count; i++)
            {
                vContours[i] = ContourToVector(con[i], baseScale, splitContour);
            }
            return vContours;
        }

        private static Vector2[] ContourToVector(IReadOnlyList<ContourPoint> contour, float baseScale, bool splitContour)
        {
            if (!contour[0].OnCurve)
            {
                var c2 = new List<ContourPoint>(contour.Count + 1);
                var c0 = new ContourPoint(
                    (short)((contour[0].X + contour[contour.Count - 1].X) / 2),
                    (short)((contour[0].Y + contour[contour.Count - 1].Y) / 2), true);
                c2.Add(c0);
                c2.AddRange(contour);
                contour = c2;
            }

            var points = new List<Vector2>();
            var segment = new List<Vector2> {contour[0].ToVector2(baseScale)};

            for (var i = 1; i < contour.Count; i++)
            {
                var p = contour[i];
                var v = p.ToVector2(baseScale);
                if (p.OnCurve)
                {
                    if (segment.Count == 0)
                    {
                        segment.Add(v);
                    }
                    else
                    {
                        segment.Add(v);

                        AddBezierCurve(segment, points);

                        if (i == contour.Count - 1)
                        {
                            points.Add(v);
                        }
                        else
                        {
                            segment.Clear();
                            segment.Add(v);
                        }
                    }
                }
                else
                {
                    if (segment.Count == 2)
                    {
                        if (splitContour)
                        {
                            var mp = new Vector2((segment[1].x + v.x) * 0.5f, (segment[1].y + v.y) * 0.5f);
                            segment.Add(mp);

                            AddBezierCurve(segment, points);

                            segment.Clear();
                            segment.Add(mp);
                            segment.Add(v);
                        }
                        else
                        {
                            segment.Add(v);
                        }
                    }
                    else if (segment.Count == 1)
                    {
                        segment.Add(v);
                    }
                    else
                    {
                        throw new FontException("Font parse error.");
                    }

                    if (i == contour.Count - 1)
                    {
                        segment.Add(contour[0].ToVector2(baseScale));

                        AddBezierCurve(segment, points);
                    }
                }
            }
            return points.ToArray();
        }

        private static void AddBezierCurve(IReadOnlyList<Vector2> input, List<Vector2> output)
        {
            var cnt = input.Count;

            if (cnt < 2) throw new FontException("Input list is too short.");

            if (cnt == 2)
            {
                output.Add(input[0]);
                return;
            }

            // Get line length
            var d = 0f;
            for (var i = 0; i < cnt - 1; i++)
            {
                var p0 = input[i];
                var p1 = input[i + 1];
                d += Mathf.Sqrt((p0.x - p1.x) * (p0.x - p1.x) + (p0.y - p1.y) * (p0.y - p1.y));
            }

            var resolution = 1 + (int) (d / 0.1f);

            var degree = cnt - 1;

            if (resolution <= 0)
            {
                for (var i = 0; i < input.Count - 1; i++) output.Add(input[i]);
                return;
            }

            var n = degree * resolution;
            var tick = 1f / n;

            for (var i = 0; i < n; i++)
            {
                var p = Vector2.zero;
                for (var j = 0; j < cnt; j++)
                {
                    var b = Bernstein(cnt - 1, j, tick * i);
                    p.x += input[j].x * b;
                    p.y += input[j].y * b;
                }
                output.Add(p);
            }
        }

        private static float Bernstein(int n, int i, float t)
        {
            return Binomial(n, i) * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);
        }

        private static float Binomial(int n, int k)
        {
            return Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        private static float Factorial(int a)
        {
            var result = 1f;
            for (var i = 2; i <= a; i++) result *= i;
            return result;
        }

        private static Vector2 ToVector2(this ContourPoint p)
        {
            return new Vector2(p.X, p.Y);
        }

        private static Vector2 ToVector2(this ContourPoint p, float scale)
        {
            return new Vector2(p.X * scale, p.Y * scale);
        }

    }
}