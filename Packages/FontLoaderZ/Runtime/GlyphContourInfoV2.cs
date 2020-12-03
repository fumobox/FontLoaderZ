using UnityEngine;

namespace FontLoaderZ
{
    public readonly struct GlyphContourInfoV2
    {
        public float AdvancedWidth { get; }

        public float LeftSideBearing { get; }

        public Vector2[][] Contours { get; }

        public GlyphContourInfoV2(float advancedWidth, float leftSideBearing, Vector2[][] contours)
        {
            AdvancedWidth = advancedWidth;
            LeftSideBearing = leftSideBearing;
            Contours = contours;
        }
    }
}