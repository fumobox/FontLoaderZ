using System.Collections.Generic;
using FontLoaderZ;
using UnityEngine;

public sealed class ShowTextDemo : MonoBehaviour
{

    [SerializeField] private TextAsset _font;

    [SerializeField] private string _text;

    [SerializeField] private float _pointScale;

    [SerializeField] private Material _lineMaterial;

    [SerializeField] private Color _lineStartColor;

    [SerializeField] private Color _lineEndColor;

    private List<GlyphContourInfoV2> _contourPoints;

    private void Start()
    {
        var source = new FontSource(_font.bytes);

        var fontModel = FontModel.Load(source);

        var codePoints = UnicodeUtility.ToCodePoints(_text);
        _contourPoints = new List<GlyphContourInfoV2>(codePoints.Length);

        var offset = 0f;
        for (var i = 0; i < codePoints.Length; i++)
        {
            try
            {
                var codePoint = codePoints[i];
                var glyph = fontModel.GetGlyph(codePoint, source);
                var con = glyph.ToVector2(fontModel, source);
                ShowPoint(con, offset);
                offset += con.AdvancedWidth;
                _contourPoints.Add(con);
            }
            catch (FontException e)
            {
                Debug.LogWarning(e);

                // Replace with a tofu glyph.
                var glyph = fontModel.GetNotDefGlyph(source);
                var con = glyph.ToVector2(fontModel, source);
                ShowPoint(con, offset);
                offset += con.AdvancedWidth;
                _contourPoints.Add(con);
            }
        }
    }

    private void ShowPoint(GlyphContourInfoV2 con, float offset)
    {
        var scale = new Vector3(_pointScale, _pointScale, _pointScale);
        var glyphObject = new GameObject("Glyph");
        glyphObject.transform.SetParent(transform);
        glyphObject.transform.localPosition = new Vector3(offset, 0, 0);
        foreach (var points in con.Contours)
        {
            var contourObject = new GameObject("Contour");
            contourObject.transform.SetParent(glyphObject.transform, false);

            var lineRenderer = contourObject.AddComponent<LineRenderer> ();
            lineRenderer.positionCount = points.Length;
            lineRenderer.loop = true;
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.startColor = _lineStartColor;
            lineRenderer.endColor = _lineEndColor;
            lineRenderer.material = _lineMaterial;
            var cp = contourObject.transform.position;
            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                lineRenderer.SetPosition(i, new Vector3(cp.x + point.x, cp.y + point.y, 0));
            }

            foreach (var point in points)
            {
                var pointObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pointObject.transform.SetParent(contourObject.transform);
                pointObject.transform.localScale = scale;
                pointObject.transform.localPosition = new Vector3(point.x, point.y, 0);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_contourPoints == null) return;

        Gizmos.matrix = transform.localToWorldMatrix;
        var offset = 0f;
        foreach (var con in _contourPoints)
        {
            foreach (var points in con.Contours)
            {
                Gizmos.color = Color.yellow;
                for (var i = 0; i < points.Length - 1; i++)
                {
                    Gizmos.DrawLine(
                        new Vector3(points[i].x + offset, points[i].y, 0),
                        new Vector3(points[i+1].x + offset, points[i+1].y, 0));
                }
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(
                    new Vector3(points[points.Length - 1].x + offset, points[points.Length - 1].y, 0),
                    new Vector3(points[0].x + offset, points[0].y, 0));
            }
            offset += con.AdvancedWidth;
        }
    }

}
