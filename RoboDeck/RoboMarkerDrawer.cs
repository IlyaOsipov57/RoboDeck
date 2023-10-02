using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    static class RoboMarkerDrawer
    {
        public static void DrawRoboMarkers (Graphics _graphics, DrawingParameters _parameters, MetaState _meta, IntPoint _position)
        {
            var state = _meta.TheTimeline.GetState();
            var len = state.Robots.Length;
            var columnLength = (int)Math.Ceiling((float)len / Math.Ceiling((float)(len) / _meta.TheStage.Cells.GetLength(1)));
            for(int i = 0; i < len; i++)
            {
                var robot = state.Robots[i];
                var marker = RoboMarker.Active;
                switch(robot.Direction)
                {
                    case RoboDirection.Spawning:
                        marker = RoboMarker.Spawning;
                        break;
                    case RoboDirection.Despawned:
                        if(robot.Position == _meta.TheStage.Exit)
                        {
                            marker = RoboMarker.Exited;
                        }
                        else
                        {
                            marker = RoboMarker.Despawned;
                        }
                        break;
                }
                var position = _position + new IntPoint(i / columnLength, i % columnLength) * _parameters.BigGridStep;
                Glyphs.DrawRoboMarker(_graphics, position, _parameters.BigGridMultiplier, GlyphColor.Bright, marker, _meta.HighlightedRobo == i);
            }
        }

        public static void Update(DrawingParameters _parameters, MetaState _meta, IntPoint _position)
        {
            var mousePosition = _parameters.MousePosition - _position;
            var dist = _parameters.BigGridStep * _parameters.BigGridStep / 4;
            var state = _meta.TheTimeline.GetState();
            var len = state.Robots.Length;
            var columnLength = (int)Math.Ceiling((float)len / Math.Ceiling((float)(len) / _meta.TheStage.Cells.GetLength(1)));
            for(int i = 0; i < len; i++)
            {
                var position = new IntPoint(i / columnLength, i % columnLength) * _parameters.BigGridStep + new IntPoint(3,3) * _parameters.BigGridMultiplier;
                if ((mousePosition - position).SquaredLength < dist)
                {
                    _meta.HighlightedRobo = i;
                    return;
                }
            }
            _meta.HighlightedRobo = -1;
        }
    }
}
