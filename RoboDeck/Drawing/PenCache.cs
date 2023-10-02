using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    public enum GlyphColor { Bright = 0, Pale = 1};

    static class Cache
    {
        static Dictionary<Tuple<GlyphColor, int>, Pen> PenCache = new Dictionary<Tuple<GlyphColor, int>, Pen>();
        static Dictionary<GlyphColor, Brush> BrushCache = new Dictionary<GlyphColor, Brush>();

        public static Pen GetPen(GlyphColor _color, int _multiplier)
        {
            var key = new Tuple<GlyphColor, int>(_color, _multiplier);
            if(!PenCache.ContainsKey(key))
            {
                var pen = new Pen(GetColor(_color), (_multiplier + 1) / 2)
                {
                    LineJoin = System.Drawing.Drawing2D.LineJoin.Round,
                    EndCap = System.Drawing.Drawing2D.LineCap.Round,
                    StartCap = System.Drawing.Drawing2D.LineCap.Round,
                };
                PenCache.Add(key, pen);
            }
            return PenCache[key];
        }

        public static Brush GetBrush(GlyphColor _color)
        {
            var key = _color;
            if (!BrushCache.ContainsKey(key))
            {
                var brush = new SolidBrush(GetColor(_color));
                BrushCache.Add(key, brush);
            }
            return BrushCache[key];
        }

        private static Color GetColor (GlyphColor _color)
        {
            bool bright = (byte)_color % 2 == 0;
            return bright ? Color.Black : Color.Gray;
        }
    }
}
