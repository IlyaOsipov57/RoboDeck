using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboDeck
{
    public enum CellValue { Empty = 1, Wall = 2, NotRobo = 3, Robo = 4, NotWall = 5, NotEmpty = 6, Any = 7 };
    public enum RoboDirection { Up = 0, Right = 1, Down = 2, Left = 3, Spawning = 4, Despawned = 5};
    public enum RoboAction { Wait = 0, TurnRight = 1, TurnAround = 2, TurnLeft = 3, MoveForward = 4, MoveRight = 5, MoveBackwards = 6, MoveLeft = 7, Terminate = 8, Spawn = 9 };
    public enum ButtonGlyph { Play, Stop, Debug, Pause, Step, StepBack, FastForward, Save, Load};
    public enum RoboMarker { Spawning, Active, Despawned, Exited};

    class Glyphs
    {
        public static void DrawGlyph(Graphics _graphics, IntPoint _position, CellValue _value, int _multiplier, GlyphColor _color)
        {
            switch(_value)
            {
                case CellValue.Empty:
                    DrawEmpty(_graphics, _position, _multiplier, _color);
                    break;
                case CellValue.Wall:
                    DrawWall(_graphics, _position, _multiplier, _color);
                    break;
                case CellValue.NotRobo:
                    DrawRobo(_graphics, _position, _multiplier, _color);
                    DrawNot(_graphics, _position, _multiplier, _color);
                    break;
                case CellValue.Robo:
                    DrawRobo(_graphics, _position, _multiplier, _color);
                    break;
                case CellValue.NotWall:
                    DrawWall(_graphics, _position, _multiplier, _color);
                    DrawNot(_graphics, _position, _multiplier, _color);
                    break;
                case CellValue.NotEmpty:
                    DrawEmpty(_graphics, _position, _multiplier, _color);
                    DrawNot(_graphics, _position, _multiplier, _color);
                    break;
                case CellValue.Any:
                default:
                    return;
            }
        }

        public static void DrawGlyph(Graphics _graphics, IntPoint _position, RoboAction _action, int _multiplier, GlyphColor _color)
        {
            switch (_action)
            {
                case RoboAction.Wait:
                    DrawWait(_graphics, _position, _multiplier, _color);
                    break;
                case RoboAction.MoveForward:
                    DrawMoveForward(_graphics, _position, _multiplier, _color);
                    break;
                case RoboAction.Terminate:
                    DrawTerminate(_graphics, _position, _multiplier, _color);
                    break;
                case RoboAction.MoveLeft:
                    DrawMoveLeft(_graphics, _position, _multiplier, _color);
                    break;
                case RoboAction.MoveBackwards:
                    DrawMoveBackward(_graphics, _position, _multiplier, _color);
                    break;
                case RoboAction.MoveRight:
                    DrawMoveRight(_graphics, _position, _multiplier, _color);
                    break;
                case RoboAction.TurnLeft:
                    DrawTurnLeft(_graphics, _position, _multiplier, _color);
                    break;
                case RoboAction.TurnAround:
                    DrawTurnAround(_graphics, _position, _multiplier, _color);
                    break;
                case RoboAction.TurnRight:
                    DrawTurnRight(_graphics, _position, _multiplier, _color);
                    break;
                default:
                    return;
            }
        }

        private static void DrawWait(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            var r = new Rectangle((Point)(_position + IntPoint.DownRight * _multiplier), (Size)(IntPoint.DownRight * (4 * _multiplier)));
            _graphics.DrawEllipse(pen, r);
            _graphics.DrawLine(pen, (Point)(_position + new IntPoint(3, 3) * _multiplier), (Point)(_position + new IntPoint(3, 2) * _multiplier));
            _graphics.DrawLine(pen, (Point)(_position + new IntPoint(3, 3) * _multiplier), (Point)(_position + new IntPoint(5, 3) * _multiplier));
        }

        private static void DrawTerminate(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            _graphics.DrawLine(pen, (Point)(_position + IntPoint.DownRight * _multiplier), (Point)(_position + IntPoint.DownRight * (5 * _multiplier)));
            _graphics.DrawLine(pen, (Point)(_position + new IntPoint(5, 1) * _multiplier), (Point)(_position + new IntPoint(1, 5) * _multiplier));
        }


        private static void DrawMoveForward(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            _graphics.DrawLines(pen, new Point[]{
                (Point)(_position + new IntPoint(3, 5) * _multiplier),
                (Point)(_position + new IntPoint(3, 1) * _multiplier),
                (Point)(_position + new IntPoint(4, 2) * _multiplier),
                (Point)(_position + new IntPoint(3, 1) * _multiplier),
                (Point)(_position + new IntPoint(2, 2) * _multiplier)});
        }
        private static void DrawMoveLeft(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            _graphics.DrawLines(pen,  new Point[]{
                (Point)(_position + new IntPoint(5, 3) * _multiplier),
                (Point)(_position + new IntPoint(1, 3) * _multiplier),
                (Point)(_position + new IntPoint(2, 4) * _multiplier),
                (Point)(_position + new IntPoint(1, 3) * _multiplier),
                (Point)(_position + new IntPoint(2, 2) * _multiplier)});
        }

        private static void DrawMoveBackward(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            _graphics.DrawLines(pen, new Point[]{
                (Point)(_position + new IntPoint(3, 1) * _multiplier),
                (Point)(_position + new IntPoint(3, 5) * _multiplier),
                (Point)(_position + new IntPoint(4, 4) * _multiplier),
                (Point)(_position + new IntPoint(3, 5) * _multiplier),
                (Point)(_position + new IntPoint(2, 4) * _multiplier)});
        }

        private static void DrawMoveRight(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            _graphics.DrawLines(pen, new Point[]{
                (Point)(_position + new IntPoint(1, 3) * _multiplier),
                (Point)(_position + new IntPoint(5, 3) * _multiplier),
                (Point)(_position + new IntPoint(4, 4) * _multiplier),
                (Point)(_position + new IntPoint(5, 3) * _multiplier),
                (Point)(_position + new IntPoint(4, 2) * _multiplier)});
        }

        private static void DrawTurnLeft(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            _graphics.DrawLines(pen, new Point[]{
                (Point)(_position + new IntPoint(3, 5) * _multiplier),
                (Point)(_position + new IntPoint(3, 3) * _multiplier),
                (Point)(_position + new IntPoint(1, 3) * _multiplier),
                (Point)(_position + new IntPoint(2, 4) * _multiplier),
                (Point)(_position + new IntPoint(1, 3) * _multiplier),
                (Point)(_position + new IntPoint(2, 2) * _multiplier)});
        }

        private static void DrawTurnAround(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            _graphics.DrawLines(pen, new Point[]{
                (Point)(_position + new IntPoint(4, 5) * _multiplier),
                (Point)(_position + new IntPoint(4, 3) * _multiplier),
                (Point)(_position + new IntPoint(2, 3) * _multiplier),
                (Point)(_position + new IntPoint(2, 5) * _multiplier),
                (Point)(_position + new IntPoint(3, 4) * _multiplier),
                (Point)(_position + new IntPoint(2, 5) * _multiplier),
                (Point)(_position + new IntPoint(1, 4) * _multiplier)});
        }

        private static void DrawTurnRight(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            _graphics.DrawLines(pen, new Point[]{
                (Point)(_position + new IntPoint(3, 5) * _multiplier),
                (Point)(_position + new IntPoint(3, 3) * _multiplier),
                (Point)(_position + new IntPoint(5, 3) * _multiplier),
                (Point)(_position + new IntPoint(4, 4) * _multiplier),
                (Point)(_position + new IntPoint(5, 3) * _multiplier),
                (Point)(_position + new IntPoint(4, 2) * _multiplier)});
        }

        private static void DrawNot(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            _graphics.DrawLine(pen, (Point)(_position + IntPoint.DownRight * (int)(0.75 * _multiplier)), (Point)(_position + IntPoint.DownRight * (int)(5.25 * _multiplier)));
        }

        private static void DrawEmpty(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            var r = new Rectangle((Point)(_position + IntPoint.DownRight * _multiplier), (Size)(IntPoint.DownRight * (4 * _multiplier)));
            _graphics.DrawRectangle(pen, r);
        }

        private static void DrawWall(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var brush = Cache.GetBrush(_color);
            var r = new Rectangle((Point)(_position + IntPoint.DownRight * _multiplier), (Size)(IntPoint.DownRight * (4 * _multiplier)));
            _graphics.FillRectangle(brush, r);
        }

        private static void DrawRobo(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color, RoboDirection _direction = RoboDirection.Up)
        {
            var brush = Cache.GetBrush(_color);
            var mesh = GetRoboMesh(_position, _multiplier, _direction).Select(p => (Point)p).ToArray();
            _graphics.FillPolygon(brush, mesh);
        }

        public static void DrawEntrance(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color, RoboDirection _direction = RoboDirection.Up)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            var mesh = GetRoboMesh(_position, _multiplier, _direction).Select(p => (Point)p).ToArray();
            _graphics.DrawPolygon(pen, mesh);
        }

        public static void DrawExit(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            var r = new Rectangle((Point)(_position + new IntPoint(1, 1) * _multiplier), (Size)(new IntPoint(4, 4) * _multiplier));
            var r2 = new Rectangle((Point)(_position + new IntPoint(2, 2) * _multiplier), (Size)(new IntPoint(2, 2) * _multiplier));
            _graphics.DrawEllipse(pen, r);
            _graphics.DrawEllipse(pen, r2);
        }

        public static void DrawBreakPoint(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            var r = new Rectangle((Point)(_position + new IntPoint(1, 1) * _multiplier), (Size)(new IntPoint(4, 4) * _multiplier));
            _graphics.DrawEllipse(pen, r);
        }

        public static void DrawRobo(Graphics _graphics, IntPoint _position, int _multiplier, int _gridStep, GlyphColor _color, RoboState _state, RoboAction _action, ActionFail _failed, float _stateProgress, bool _highlight)
        {
            var mesh = GetMesh(_position, _multiplier, _gridStep, _state, _action, _failed, _stateProgress);
            if(mesh == null)
            {
                return;
            }
            var brush = Cache.GetBrush(_color);
            _graphics.FillPolygon(brush, mesh);
            if(_highlight)
            {
                var c = GetCenter(mesh);
                DrawHighlight(_graphics, _multiplier, c);
            }
        }

        public static IntPoint GetRoboCenter(IntPoint _position, int _multiplier, int _gridStep, RoboState _state, RoboAction _action, ActionFail _failed, float _stateProgress)
        {
            var mesh = GetMesh(_position, _multiplier, _gridStep, _state, _action, _failed, _stateProgress);
            if (mesh == null)
            {
                return _state.Position * _gridStep + new IntPoint(3, 3) * _multiplier;
            }
            return GetCenter(mesh);
        }

        private static IntPoint GetCenter(Point[] _mesh)
        {
            return new IntPoint((_mesh[0].X + _mesh[1].X + _mesh[1].X + _mesh[2].X) / 4, (_mesh[0].Y + _mesh[1].Y + _mesh[1].Y + _mesh[2].Y) / 4);
        }

        private static Point[] GetMesh(IntPoint _position, int _multiplier, int _gridStep, RoboState _state, RoboAction _action, ActionFail _failed, float _stateProgress)
        {
            switch (_action)
            {
                case RoboAction.Wait:
                    if (_state.Direction == RoboDirection.Despawned) return null;
                    break;
                case RoboAction.Spawn:
                    if (_failed != ActionFail.DoNot) return null;
                    break;
            }
            var nextState = StateUpdater.ApplyAction(_state, _action);
            if (_action == RoboAction.TurnAround)
            {
                var _intermediateState = new RoboState()
                {
                    Position = _state.Position,
                    Direction = _state.Direction ^ RoboDirection.Right
                };
                _stateProgress *= 2;
                if (_stateProgress < 1)
                {
                    nextState = _intermediateState;
                }
                else
                {
                    _state = _intermediateState;
                    _stateProgress -= 1;
                }
            }
            var zoom = 1f;
            switch (_action)
            {
                case RoboAction.MoveForward:
                case RoboAction.MoveRight:
                case RoboAction.MoveBackwards:
                case RoboAction.MoveLeft:
                    switch (_failed)
                    {
                        case ActionFail.Fail:
                            _stateProgress = Math.Min(1 - _stateProgress, _stateProgress);
                            break;
                        case ActionFail.FailFast:
                            _stateProgress = (1 - _stateProgress) * _stateProgress;
                            break;
                    }
                    break;
                case RoboAction.TurnRight:
                case RoboAction.TurnAround:
                case RoboAction.TurnLeft:
                    zoom = (1 - _stateProgress) * _stateProgress + 1;
                    break;
            }
            var center = _state.Position * _gridStep + new IntPoint(3, 3) * _multiplier;
            var nextMesh = GetRoboMesh(nextState.Position * _gridStep, _multiplier, nextState.Direction);
            var mesh = GetRoboMesh(_state.Position * _gridStep, _multiplier, _state.Direction)
                .Select((p, i) => (Point)(_position + (p + (nextMesh[i] - p) * _stateProgress - center) * zoom + center)).ToArray();
            return mesh;
        }

        public static IntPoint[] GetRoboMesh (IntPoint _position, int _multiplier, RoboDirection _direction)
        {
            switch (_direction)
            {
                case RoboDirection.Up:
                    return new IntPoint[]{
                        _position + new IntPoint(1, 5) * _multiplier,
                        _position + new IntPoint(3, 1) * _multiplier,
                        _position + new IntPoint(5, 5) * _multiplier,
                    };
                case RoboDirection.Right:
                    return new IntPoint[]{
                        _position + new IntPoint(1, 1) * _multiplier,
                        _position + new IntPoint(5, 3) * _multiplier,
                        _position + new IntPoint(1, 5) * _multiplier
                    };
                case RoboDirection.Down:
                    return new IntPoint[]{
                        _position + new IntPoint(5, 1) * _multiplier,
                        _position + new IntPoint(3, 5) * _multiplier,
                        _position + new IntPoint(1, 1) * _multiplier
                    };
                case RoboDirection.Left:
                    return new IntPoint[]{
                        _position + new IntPoint(5, 5) * _multiplier,
                        _position + new IntPoint(1, 3) * _multiplier,
                        _position + new IntPoint(5, 1) * _multiplier
                    };
                case RoboDirection.Spawning:
                case RoboDirection.Despawned:
                default:
                    return new IntPoint[]{
                        _position + new IntPoint(3, 3) * _multiplier,
                        _position + new IntPoint(3, 3) * _multiplier,
                        _position + new IntPoint(3, 3) * _multiplier
                    };
            }
        }

        private static Point Transform(IntPoint _start, IntPoint _end, float _stateProgress)
        {
            return (Point)(_start + (_end - _start) * _stateProgress);
        }

        public static void DrawButtonGlyph(Graphics _graphics, IntPoint _position, ButtonGlyph _button, int _multiplier, GlyphColor _color)
        {
            switch(_button)
            {
                case ButtonGlyph.Play:
                    DrawRobo(_graphics, _position, _multiplier, _color, RoboDirection.Right);
                    break;
                case ButtonGlyph.FastForward:
                    DrawFastForward(_graphics, _position, _multiplier, _color);
                    break;
                case ButtonGlyph.Stop:
                    DrawWall(_graphics, _position, _multiplier, _color);
                    break;
                case ButtonGlyph.Debug:
                    DrawDebug(_graphics, _position, _multiplier, _color);
                    break;
                case ButtonGlyph.Pause:
                    DrawPause(_graphics, _position, _multiplier, _color);
                    break;
                case ButtonGlyph.Step:
                    DrawStepButton(_graphics, _position, _multiplier, _color);
                    break;
                case ButtonGlyph.StepBack:
                    DrawStepBackButton(_graphics, _position, _multiplier, _color);
                    break;
                case ButtonGlyph.Save:
                    DrawLoad(_graphics, _position, _multiplier, _color);
                    break;
                case ButtonGlyph.Load:
                    DrawMoveForward(_graphics, _position, _multiplier, _color);
                    DrawSave(_graphics, _position, _multiplier, _color);
                    break;
            }
        }

        private static void DrawLoad(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            DrawMoveBackward(_graphics, _position, _multiplier, _color);
            var brush = Cache.GetBrush(_color);
            var r = new Rectangle((Point)(_position + new IntPoint(3, 1) * _multiplier / 2), (Size)(new IntPoint(3, 2) * _multiplier));
            _graphics.FillRectangle(brush, r);
        }

        private static void DrawSave(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            DrawMoveForward(_graphics, _position, _multiplier, _color);
            var brush = Cache.GetBrush(_color);
            var r = new Rectangle((Point)(_position + new IntPoint(3, 7) * _multiplier / 2), (Size)(new IntPoint(3, 2) * _multiplier));
            _graphics.FillRectangle(brush, r);
        }

        public static void DrawPause(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var brush = Cache.GetBrush(_color);
            var r = new Rectangle((Point)(_position + IntPoint.DownRight * _multiplier), (Size)(new IntPoint(1, 4) * _multiplier));
            _graphics.FillRectangle(brush, r);
            r = new Rectangle((Point)(_position + new IntPoint(4,1) * _multiplier), (Size)(new IntPoint(1, 4) * _multiplier));
            _graphics.FillRectangle(brush, r);
        }

        public static void DrawStepButton(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var brush = Cache.GetBrush(_color);
            _graphics.FillPolygon(brush, new Point[]{
                (Point)(_position + new IntPoint(1, 1) * _multiplier),
                (Point)(_position + new IntPoint(3, 3) * _multiplier),
                (Point)(_position + new IntPoint(1, 5) * _multiplier)});
            var r = new Rectangle((Point)(_position + new IntPoint(4, 1) * _multiplier), (Size)(new IntPoint(1, 4) * _multiplier));
            _graphics.FillRectangle(brush, r);
        }

        public static void DrawFastForward(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            var pen2 = Cache.GetPen(_color, _multiplier);
            var r = new Rectangle((Point)(_position + new IntPoint(1, 2) * _multiplier), (Size)(new IntPoint(4, 4) * _multiplier));
            _graphics.DrawArc(pen, r, 0, -180);
            _graphics.DrawLine(pen2, (Point)(_position + new IntPoint(3, 4) * _multiplier), (Point)(_position + new IntPoint(4, 2) * _multiplier));
        }

        public static void DrawStepBackButton(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var brush = Cache.GetBrush(_color);
            var r = new Rectangle((Point)(_position + IntPoint.DownRight * _multiplier), (Size)(new IntPoint(1, 4) * _multiplier));
            _graphics.FillRectangle(brush, r);
            _graphics.FillPolygon(brush, new Point[]{
                (Point)(_position + new IntPoint(5, 1) * _multiplier),
                (Point)(_position + new IntPoint(3, 3) * _multiplier),
                (Point)(_position + new IntPoint(5, 5) * _multiplier)});
        }

        private static void DrawDebug(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var brush = Cache.GetBrush(_color);
            var r = new Rectangle((Point)(_position + IntPoint.DownRight * _multiplier), (Size)(IntPoint.DownRight * (4 * _multiplier)));
            _graphics.FillEllipse(brush, r);
        }

        public static void DrawResize(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color)
        {
            var pen = Cache.GetPen(_color, _multiplier);
            _graphics.DrawLines(pen, new Point[]{
                (Point)(_position + new IntPoint(2, 3) * _multiplier),
                (Point)(_position + new IntPoint(2, 2) * _multiplier),
                (Point)(_position + new IntPoint(3, 2) * _multiplier),
                (Point)(_position + new IntPoint(2, 2) * _multiplier),
                (Point)(_position + new IntPoint(4, 4) * _multiplier),
                (Point)(_position + new IntPoint(4, 3) * _multiplier),
                (Point)(_position + new IntPoint(4, 4) * _multiplier),
                (Point)(_position + new IntPoint(3, 4) * _multiplier)});
        }

        public static void DrawRoboMarker(Graphics _graphics, IntPoint _position, int _multiplier, GlyphColor _color, RoboMarker _marker, bool _highlight)
        {
            if (_highlight)
            {
                var c = _position + _multiplier * new IntPoint(3, 3);
                c = DrawHighlight(_graphics, _multiplier, c);
            }
            switch(_marker)
            {
                case RoboMarker.Spawning:
                    DrawEntrance(_graphics, _position, _multiplier, _color);
                    return;
                case RoboMarker.Active:
                    DrawRobo(_graphics, _position, _multiplier, _color);
                    return;
                case RoboMarker.Despawned:
                    DrawTerminate(_graphics, _position, _multiplier, _color);
                    return;
                case RoboMarker.Exited:
                    DrawExit(_graphics, _position, _multiplier, _color);
                    return;
            }
        }

        private static IntPoint DrawHighlight(Graphics _graphics, int _multiplier, IntPoint _center)
        {
            var pen = Cache.GetPen(GlyphColor.Pale, _multiplier);
            _graphics.DrawRectangle(pen, _center.X - _multiplier * 3, _center.Y - _multiplier * 3, _multiplier * 6, _multiplier * 6);
            return _center;
        }
    }
}
