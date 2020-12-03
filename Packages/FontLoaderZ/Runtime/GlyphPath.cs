using System.Collections.Generic;
using System.Linq;

namespace FontLoaderZ
{
    public sealed class GlyphPath
    {
        private int _index;

        public GlyphPath()
        {
            Points = new List<ContourPoint>();
        }

        public List<ContourPoint> Points { get; }

        public ContourPoint LastPoint => Points.Last();

        public int Count => Points.Count;

        public void NewContour(float x, float y)
        {
            if (Points.Count == 0)
            {
                Points.Add(new ContourPoint(x, y, true, false));
            }
            else
            {
                var p = Points[_index - 1];
                Points.Add(new ContourPoint(p.x + x, p.y + y, true, false));
            }

            _index++;
        }

        public void Add(float x, float y, bool isCurve = false)
        {
            var p = Points[_index - 1];
            p.Add(x, y);
            p.isContour = false;
            p.isCurve = isCurve;
            Points.Add(p);
            _index++;
        }

        public void ReplaceLastPoint(float x, float y)
        {
            var p = Points[_index - 1];
            p.x = x;
            p.y = y;
            Points[_index - 1] = p;
        }

        public struct ContourPoint
        {
            public float x;
            public float y;
            public bool isContour;
            public bool isCurve;

            public ContourPoint(float x, float y, bool isContour, bool isCurve)
            {
                this.x = x;
                this.y = y;
                this.isContour = isContour;
                this.isCurve = isCurve;
            }

            public void Add(float x, float y)
            {
                this.x += x;
                this.y += y;
            }
        }
    }
}