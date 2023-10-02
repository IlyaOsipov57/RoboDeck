using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    class StageDrawer
    {
        private bool dragging;
        private EditorTile brush;
        private enum EditorTile { Entrance, Exit, Wall, Empty, OOB, Resize };

        public int GetHeight (DrawingParameters _parameters, Stage _stage)
        {
            return _stage.Cells.GetLength(1) * _parameters.BigGridStep;
        }

        public int GetWidth(DrawingParameters _parameters, Stage _stage)
        {
            return _stage.Cells.GetLength(0) * _parameters.BigGridStep;
        }

        public void DrawStage(Graphics _graphics, DrawingParameters _parameters, IntPoint _position, MetaState _meta)
        {
            var timeline = _meta.TheTimeline;
            var stateProgress = timeline.GetTime();
            var state = timeline.GetState();
            var stage = _meta.TheStage;
            var width = stage.Cells.GetLength(0);
            var height = stage.Cells.GetLength(1);
            var bigGridStep = _parameters.BigGridStep;
            for (int i = 0; i <= height; i++)
            {
                _graphics.DrawLine(Pens.Black, _position.X, _position.Y + i * bigGridStep, _position.X + width * bigGridStep, _position.Y + i * bigGridStep);
            }
            for (int i = 0; i <= width; i++)
            {
                _graphics.DrawLine(Pens.Black, _position.X + i * bigGridStep, _position.Y, _position.X + i * bigGridStep, _position.Y + height * bigGridStep);
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (stage.Cells[i, j])
                    {
                        var p = new IntPoint(i, j) * _parameters.BigGridStep;
                        Glyphs.DrawGlyph(_graphics, _position + p, CellValue.Wall, _parameters.BigGridMultiplier, GlyphColor.Bright);
                    }
                }
            }
            if (_meta.EditorMode == UserInterfaceMode.Editing || timeline.Time == 0)
            {
                Glyphs.DrawEntrance(_graphics, _position + stage.Entrance * _parameters.BigGridStep, _parameters.BigGridMultiplier, GlyphColor.Bright);
            }
            Glyphs.DrawExit(_graphics, _position + stage.Exit * _parameters.BigGridStep, _parameters.BigGridMultiplier, GlyphColor.Bright);
            if(_meta.BreakPoint.HasValue)
            {
                Glyphs.DrawBreakPoint(_graphics, _position + _meta.BreakPoint.Value * _parameters.BigGridStep, _parameters.BigGridMultiplier, GlyphColor.Bright);
            }
            var resizePosition = GetResizePosition(stage);
            Glyphs.DrawResize(_graphics, _position + resizePosition * _parameters.BigGridStep, _parameters.BigGridMultiplier, GlyphColor.Pale);
            for(int i = 0; i < state.Robots.Length; i++)
            {
                var highlighted = _meta.HighlightedRobo == i;
                Glyphs.DrawRobo(_graphics, _position, _parameters.BigGridMultiplier, _parameters.BigGridStep, GlyphColor.Bright, state.Robots[i], state.Actions[i], state.Failed[i], stateProgress, highlighted);
            }
        }

        private static IntPoint GetResizePosition(Stage _stage)
        {
            return new IntPoint(_stage.Cells.GetLength(0) - 1, _stage.Cells.GetLength(1) - 1);
        }

        public void Update(DrawingParameters _parameters, MetaState _meta, IntPoint _position)
        {
            if(_meta.EditorMode != UserInterfaceMode.Editing)
            {
                _meta.SelectTopCard(_parameters, _position);
                return;
            }
            if(dragging)
            {
                var stage = _meta.TheStage;
                var mouseTile = (_parameters.MousePosition - _position) / _parameters.BigGridStep;
                var tile = GetTile(stage, mouseTile);
                switch (brush)
                {
                    case EditorTile.Resize:
                        var newX = Math.Max(Math.Max(stage.Entrance.X, stage.Exit.X) + 1, mouseTile.X) + 1;
                        var newY = Math.Max(Math.Max(stage.Entrance.Y, stage.Exit.Y) + 1, mouseTile.Y) + 1;
                        if(newX == mouseTile.X && newY == mouseTile.Y)
                            return;
                        stage.Resize(newX, newY);
                        return;
                    case EditorTile.Entrance:
                        if (tile == EditorTile.Empty)
                        {
                            stage.Entrance = mouseTile;
                        }
                        return;
                    case EditorTile.Exit:
                        if (tile == EditorTile.Empty)
                        {
                            stage.Exit = mouseTile;
                        }
                        return;
                    case EditorTile.Wall:
                        if (tile == EditorTile.Empty)
                        {
                            stage.Cells[mouseTile.X, mouseTile.Y] = true;
                        }
                        return;
                    case EditorTile.Empty:
                        if (tile == EditorTile.Wall)
                        {
                            stage.Cells[mouseTile.X, mouseTile.Y] = false;
                        }
                        return;
                    default:
                        return;
                }
            }
        }

        public bool MouseDown(DrawingParameters _parameters, MetaState _meta, IntPoint _position)
        {
            var stage = _meta.TheStage;
            var mouseTile = (_parameters.MousePosition - _position) / _parameters.BigGridStep;
            var tile = GetTile(stage, mouseTile);
            if (_meta.EditorMode != UserInterfaceMode.Editing)
            {
                if(mouseTile == _meta.BreakPoint)
                {
                    _meta.BreakPoint = null;
                }
                else if (tile == EditorTile.Empty)
                {
                    _meta.BreakPoint = mouseTile;
                }
                return false;
            }
            switch(tile)
            {
                case EditorTile.Resize:
                    brush = EditorTile.Resize;
                    dragging = true;
                    return true;
                case EditorTile.Entrance:
                    brush = EditorTile.Entrance;
                    dragging = true;
                    return true;
                case EditorTile.Exit:
                    brush = EditorTile.Exit;
                    dragging = true;
                    return true;
                case EditorTile.Wall:
                    brush = EditorTile.Empty;
                    dragging = true;
                    stage.Cells[mouseTile.X, mouseTile.Y] = false;
                    return true;
                case EditorTile.Empty:
                    brush = EditorTile.Wall;
                    dragging = true;
                    stage.Cells[mouseTile.X, mouseTile.Y] = true;
                    return true;
                case EditorTile.OOB:
                default:
                    return false;
            }
        }

        private static EditorTile GetTile(Stage _stage, IntPoint mouseTile)
        {
            if(mouseTile == GetResizePosition(_stage))
            {
                return EditorTile.Resize;
            }
            if (mouseTile.X <= 0 || mouseTile.Y <= 0 ||
                mouseTile.X >= _stage.Cells.GetLength(0) - 1 ||
                mouseTile.Y >= _stage.Cells.GetLength(1) - 1)
            {
                return EditorTile.OOB;
            }
            if (_stage.Cells[mouseTile.X, mouseTile.Y])
            {
                return EditorTile.Wall;
            }
            if (_stage.Entrance == mouseTile)
            {
                return EditorTile.Entrance;
            }
            if (_stage.Exit == mouseTile)
            {
                return EditorTile.Exit;
            }
            return EditorTile.Empty;
        }

        public void MouseUp()
        {
            dragging = false;
        }
    }
}
