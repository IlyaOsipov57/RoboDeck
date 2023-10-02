using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoboDeck
{
    class CardDrawing
    {
        private static RoboAction[,] ActionMap = new RoboAction[3, 3] {
            {RoboAction.Wait, RoboAction.MoveForward, RoboAction.Terminate},
            {RoboAction.MoveLeft, RoboAction.MoveBackwards, RoboAction.MoveRight},
            {RoboAction.TurnLeft, RoboAction.TurnAround, RoboAction.TurnRight}
        };

        private static CellValue[,] CellValueMap = new CellValue[3, 3] {
            {CellValue.Empty, CellValue.Wall, CellValue.Robo},
            {CellValue.Any, CellValue.Any, CellValue.Any},
            {CellValue.NotEmpty, CellValue.NotWall, CellValue.NotRobo}
        };

        public static void DrawPlusCard(Graphics _graphics, DrawingParameters _parameters, IntPoint _position)
        {
            _graphics.FillRectangle(Brushes.LightGray, _position.X, _position.Y, _parameters.BigGridStep, _parameters.DeckHeight);
            _graphics.DrawRectangle(Pens.Black, _position.X, _position.Y, _parameters.BigGridStep, _parameters.DeckHeight);
            var xPlus = _position.X + _parameters.BigGridStep / 2;
            var yPlus = _position.Y + _parameters.DeckHeight / 2;
            var plusHalfLength = _parameters.CardOffset;
            _graphics.DrawLine(Pens.Black, xPlus, yPlus - plusHalfLength, xPlus, yPlus + plusHalfLength);
            _graphics.DrawLine(Pens.Black, xPlus - plusHalfLength, yPlus, xPlus + plusHalfLength, yPlus);
        }

        public static void DrawCardBorder(Graphics _graphics, DrawingParameters _parameters, IntPoint _position, bool _isActiveCard, bool _drawCross)
        {
            _graphics.FillRectangle(Brushes.LightGray, _position.X, _position.Y, _parameters.CardWidth, _parameters.DeckHeight);
            var pen = _isActiveCard ? new Pen(Color.Black, 4) : new Pen(Color.Black, 2);
            _graphics.DrawRectangle(pen, _position.X, _position.Y, _parameters.CardWidth, _parameters.DeckHeight);
            if (_drawCross)
            {
                var xCross = _position.X + _parameters.CardWidth - _parameters.CardOffset / 2;
                var yCross = _position.Y + _parameters.CardOffset / 2;
                var crossHalfLength = _parameters.CardOffset / 2;
                _graphics.DrawLine(Pens.Black, xCross - crossHalfLength, yCross - crossHalfLength, xCross + crossHalfLength, yCross + crossHalfLength);
                _graphics.DrawLine(Pens.Black, xCross - crossHalfLength, yCross + crossHalfLength, xCross + crossHalfLength, yCross - crossHalfLength);
            }
        }

        public static void DrawCard (Graphics _graphics, DrawingParameters _parameters, Card _card, IntPoint _position, bool _isActive)
        {
            var bigGridStep = _parameters.BigGridStep;
            for (int i = 0; i < 3; i++)
            {
                _graphics.DrawLine(Pens.Black, _position.X, _position.Y + i * bigGridStep, _position.X + 3 * bigGridStep, _position.Y + i * bigGridStep);
            }
            for (int i = 0; i < 4; i++)
            {
                _graphics.DrawLine(Pens.Black, _position.X + i * bigGridStep, _position.Y, _position.X + i * bigGridStep, _position.Y + 2 * bigGridStep);
            }

            var activeSquare = new IntPoint(-1, -1);

            if (_isActive)
            {
                var mousePosition = _parameters.MousePosition - _position;

                if (0 <= mousePosition.X && mousePosition.X < 3 * bigGridStep &&
                    0 <= mousePosition.Y && mousePosition.Y < 2 * bigGridStep)
                {
                    activeSquare = mousePosition / bigGridStep;
                }
            }


            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    var square = new IntPoint(i, j);
                    if (square == activeSquare)
                    {
                        if (square == Card.ActionSquare)
                        {
                            DrawActionMap(_graphics, _position + square * bigGridStep, _parameters, (RoboAction)_card.Data[j, i]);
                        }
                        else
                        {
                            DrawCellValueMap(_graphics, _position + square * bigGridStep, _parameters, (CellValue)_card.Data[j, i]);
                        }
                    }
                    else
                    {
                        if (square == Card.ActionSquare)
                        {
                            Glyphs.DrawGlyph(_graphics, _position + square * bigGridStep, (RoboAction)_card.Data[j, i], _parameters.BigGridMultiplier, GlyphColor.Bright);
                        }
                        else
                        {
                            Glyphs.DrawGlyph(_graphics, _position + square * bigGridStep, (CellValue)_card.Data[j, i], _parameters.BigGridMultiplier, GlyphColor.Bright);
                        }
                    }
                }
            }
        }

        private static void DrawActionMap(Graphics _graphics, IntPoint _position, DrawingParameters _parameters, RoboAction _value)
        {
            int multiplier = _parameters.Multiplier;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Glyphs.DrawGlyph(_graphics, _position + new IntPoint(i * 6 * multiplier, j * 6 * multiplier), ActionMap[j, i], multiplier,
                        ActionMap[j, i] == _value ? GlyphColor.Bright : GlyphColor.Pale);
                }
            }
        }

        private static void DrawCellValueMap(Graphics _graphics, IntPoint _position, DrawingParameters _parameters, CellValue _value)
        {
            var multiplier = _parameters.Multiplier;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Glyphs.DrawGlyph(_graphics, _position + new IntPoint(i * 6 * multiplier, j * 6 * multiplier), CellValueMap[j, i], multiplier,
                        CellValueMap[j, i] == _value ? GlyphColor.Bright : GlyphColor.Pale);
                }
            }
        }

        public static bool MouseDown(DrawingParameters _parameters, Card _card, IntPoint _position)
        {
            var mousePosition = _parameters.MousePosition - _position;
            var bigGridStep = _parameters.BigGridStep;
            if (0 > mousePosition.X || mousePosition.X > 3 * bigGridStep ||
                0 > mousePosition.Y || mousePosition.Y > 2 * bigGridStep)
            {
                return false;
            }

            var activeSquare = mousePosition / bigGridStep;
            var activeSubSquare = (mousePosition - activeSquare * bigGridStep) / _parameters.GridStep;

            if (activeSquare == Card.ActionSquare)
            {
                var action = (byte)ActionMap[activeSubSquare.Y, activeSubSquare.X];
                _card.Data[activeSquare.Y, activeSquare.X] = action;
            }
            else
            {
                var action = (byte)CellValueMap[activeSubSquare.Y, activeSubSquare.X];
                _card.Data[activeSquare.Y, activeSquare.X] = action;
            }
            return true;
        }
    }
}
