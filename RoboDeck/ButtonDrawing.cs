using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    public enum ButtonState { Normal, Pressed, Disabled };

    static class ButtonDrawing
    {
        public static void DrawButton (Graphics _graphics, DrawingParameters _parameters, IntPoint _position, ButtonState _buttonState, ButtonGlyph _glyph)
        {
            if(_buttonState == ButtonState.Pressed)
            {
                _graphics.FillRectangle(Brushes.Gray, _position.X, _position.Y, _parameters.BigGridStep, _parameters.BigGridStep);
            }
            _graphics.DrawRectangle(Pens.Black, _position.X, _position.Y, _parameters.BigGridStep, _parameters.BigGridStep);

            var color = _buttonState == ButtonState.Disabled ? GlyphColor.Pale : GlyphColor.Bright;

            Glyphs.DrawButtonGlyph(_graphics, _position, _glyph, _parameters.BigGridMultiplier, color);
        }

        public static bool MouseDown (DrawingParameters _parameters, IntPoint _position)
        {
            var mousePosition = _parameters.MousePosition - _position;
            var bigGridStep = _parameters.BigGridStep;
            return 0 <= mousePosition.X && mousePosition.X < bigGridStep &&
                   0 <= mousePosition.Y && mousePosition.Y < bigGridStep;
        }
    }
}
