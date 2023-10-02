using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    public enum TimelineBehavior { Stopped, Idle, Play, Stepping, SteppingBack, Pausing }

    class TimelineDrawer
    {
        public int Width;
        private bool sliding = false;
        private bool speedSliding = false;
        public double SpeedMultiplier = 1;
        private int targetTime = 0;
        private TimelineBehavior Behavior = TimelineBehavior.Stopped;
        private bool SpeedModified = false;

        public TimelineDrawer (int _width)
        {
            Width = _width;
        }
        public void DrawTimeline (Graphics _graphics, DrawingParameters _parameters, MetaState _meta, IntPoint _position)
        {
            var buttonGlyphs = GetButtonGlyphs();
            var buttosStates = GetButtonStates(_meta);
            var buttonPositions = GetButtonPositions(_parameters, _position).ToArray();

            for (int i = 0; i < buttonGlyphs.Length; i++)
            {
                ButtonDrawing.DrawButton(_graphics, _parameters, buttonPositions[i], buttosStates[i], buttonGlyphs[i]);
            }
            var slider = GetSliderPosition(_parameters, _meta.TheTimeline, _position);
            _graphics.DrawLine(Pens.Black, _position.X, slider.Y, _position.X + Width, slider.Y);
            _graphics.DrawLine(new Pen(Color.Black, 2), slider.X, slider.Y - _parameters.CardOffset / 2, slider.X, slider.Y + _parameters.CardOffset / 2);
            if (SpeedModified)
            {
                var speedSlider = GetSpeedSliderPosition(_parameters, _position);
                _graphics.DrawLine(Pens.Black, _position.X + SpeedSliderLeft(_parameters), speedSlider.Y, _position.X + SpeedSliderLeft(_parameters) + SpeedSliderWidth(_parameters), speedSlider.Y);
                _graphics.DrawLine(new Pen(Color.Black, 2), speedSlider.X, speedSlider.Y - _parameters.CardOffset / 2, speedSlider.X, speedSlider.Y + _parameters.CardOffset / 2);
            }
        }

        private IntPoint GetSliderPosition(DrawingParameters _parameters, Timeline _timeline, IntPoint _position)
        {
            int y = _position.Y + _parameters.CardOffset * 2 + _parameters.BigGridStep;
            int x = _position.X + (int)(Width * _timeline.Time / _timeline.MaxTime);
            return new IntPoint(x, y);
        }
        private IntPoint GetSpeedSliderPosition(DrawingParameters _parameters, IntPoint _position)
        {
            int y = _position.Y + _parameters.BigGridStep / 2;
            var left = SpeedSliderLeft(_parameters);
            int x = _position.X + left + (int)(SpeedSliderWidth(_parameters) * (Math.Log(SpeedMultiplier, 2) + 1) / 5);
            return new IntPoint(x, y);
        }

        private int SpeedSliderLeft(DrawingParameters _parameters)
        {
            var step = _parameters.BigGridStep + _parameters.CardOffset;
            return step * 6 + _parameters.CardOffset;
        }

        private int SpeedSliderWidth(DrawingParameters _parameters)
        {
            return _parameters.BigGridStep * 6 - _parameters.CardOffset;
        }

        private static IntPoint[] GetButtonPositions(DrawingParameters _parameters, IntPoint _position)
        {
            var n = 6;
            var result = new IntPoint[n];
            var step = _parameters.BigGridStep + _parameters.CardOffset;
            for (int i = 0; i < n; i++)
            {
                result[i] = new IntPoint(_position.X + i * step, _position.Y);
            }
            return result;
        }

        private ButtonState[] GetButtonStates(MetaState _meta)
        {
            if (Behavior == TimelineBehavior.Stopped)
            {
                return new ButtonState[]{
                    ButtonState.Normal,
                    ButtonState.Disabled,
                    ButtonState.Disabled,
                    ButtonState.Disabled,
                    ButtonState.Disabled,
                    ButtonState.Disabled,
                };
            }
            else
            {
                var timeline = _meta.TheTimeline;
                return new ButtonState[]{
                    _meta.EditorMode == UserInterfaceMode.Debugging ? ButtonState.Disabled : ButtonState.Normal,
                    ButtonState.Normal,
                    Behavior == TimelineBehavior.SteppingBack ? ButtonState.Pressed : timeline.Time == 0 ? ButtonState.Disabled : ButtonState.Normal,
                    Behavior == TimelineBehavior.Play ? ButtonState.Pressed : Behavior == TimelineBehavior.Pausing ? ButtonState.Pressed : 
                    CanPlay(timeline) ? ButtonState.Normal : ButtonState.Disabled,
                    Behavior == TimelineBehavior.Stepping ? ButtonState.Pressed : CanPlay(timeline) ? ButtonState.Normal : ButtonState.Disabled,
                    SpeedModified ? ButtonState.Pressed : ButtonState.Normal,
                };
            }
        }

        private static bool CanPlay(Timeline timeline)
        {
            return timeline.Time != timeline.MaxTime || timeline.LoopTime != -1;
        }

        private ButtonGlyph[] GetButtonGlyphs()
        {
            return new ButtonGlyph[]{
                ButtonGlyph.Debug,
                ButtonGlyph.Stop,
                ButtonGlyph.StepBack,
                Behavior == TimelineBehavior.Pausing || Behavior == TimelineBehavior.Play ? ButtonGlyph.Pause : ButtonGlyph.Play,
                ButtonGlyph.Step,
                ButtonGlyph.FastForward,
            };
        }

        public void Update(DrawingParameters _parameters, MetaState _meta, IntPoint _position, double _deltaTime)
        {
            var timeline = _meta.TheTimeline;
            if (sliding)
            {
                timeline.Time = timeline.MaxTime * Math.Max(0, Math.Min(1, (double)(_parameters.MousePosition.X - _position.X) / Width));
                return;
            }
            if(speedSliding)
            {
                var m = Math.Max(0, Math.Min(1, (double)(_parameters.MousePosition.X - _position.X - SpeedSliderLeft(_parameters)) / SpeedSliderWidth(_parameters)));
                SpeedMultiplier = Math.Pow(2, m * 5 - 1);
            }
            if (SpeedModified)
            {
                _deltaTime *= SpeedMultiplier;
            }
            if (ApplyBehavior(_meta, _deltaTime))
            {
                Behavior = TimelineBehavior.Idle;
            }
        }

        private bool ApplyBehavior(MetaState _meta, double _deltaTime)
        {
            var timeline = _meta.TheTimeline;
            switch (Behavior)
            {
                case TimelineBehavior.Play:
                    return Debug(_meta, _deltaTime);
                case TimelineBehavior.Pausing:
                    return Pause(timeline, _deltaTime);
                case TimelineBehavior.Stepping:
                    return StepForward(timeline, _deltaTime);
                case TimelineBehavior.SteppingBack:
                    return StepBackward(timeline, _deltaTime);
                case TimelineBehavior.Stopped:
                    return false;
            }
            return false;
        }

        private bool StepForward(Timeline _timeline, double _deltaTime)
        {
            _deltaTime *= 2;
            if (_timeline.Time + _deltaTime >= targetTime)
            {
                _timeline.Time = targetTime;
                return true;
            }
            return PlayForward(_timeline, _deltaTime);
        }

        private bool StepBackward(Timeline _timeline, double _deltaTime)
        {
            _deltaTime *= 4;
            if (_timeline.Time - _deltaTime <= targetTime)
            {
                _timeline.Time = targetTime;
                return true;
            }
            return PlayBack(_timeline, _deltaTime);
        }

        private bool Pause(Timeline _timeline, double _deltaTime)
        {
            _deltaTime *= 2;
            if (_timeline.Time <= targetTime)
            {
                return StepForward(_timeline, _deltaTime);
            }
            else
            {
                return StepBackward(_timeline, _deltaTime);
            }
        }

        private bool Debug(MetaState _meta, double _deltaTime)
        {
            var timeline = _meta.TheTimeline;
            var breakPoint = _meta.BreakPoint;
            if (breakPoint.HasValue)
            {
                var next = (int)(timeline.Time + _deltaTime);
                for (int i = 1 + (int)timeline.Time; i <= next; i++)
                {
                    if (timeline.CheckBreakPoint(i, breakPoint.Value))
                    {
                        _meta.BreakPoint = null;
                        timeline.Time = i;
                        return true;
                    }
                }
            }
            return PlayForward(timeline, _deltaTime);
        }

        private bool PlayBack(Timeline _timeline, double _deltaTime)
        {
            if (_timeline.Time - _deltaTime < 0)
            {
                _timeline.Time = 0;
                return true;
            }
            else
            {
                _timeline.Time -= _deltaTime;
                return false;
            }
        }

        private bool PlayForward(Timeline _timeline, double _deltaTime)
        {
            if (_timeline.Time + _deltaTime > _timeline.MaxTime)
            {
                _timeline.Time = _timeline.MaxTime;
                return true;
            }
            else
            {
                _timeline.Time += _deltaTime;
                return false;
            }
        }

        public bool MouseDown(DrawingParameters _parameters, MetaState _meta, IntPoint _position)
        {
            var timeline = _meta.TheTimeline;

            if (SpeedModified)
            {
                var speedSliderPosition = GetSpeedSliderPosition(_parameters, _position);
                if (Math.Abs((_parameters.MousePosition - speedSliderPosition).Y) < _parameters.CardOffset &&
                _position.X + SpeedSliderLeft(_parameters) - _parameters.CardOffset <= _parameters.MousePosition.X &&
                _parameters.MousePosition.X <= _position.X + SpeedSliderLeft(_parameters) + SpeedSliderWidth(_parameters) + _parameters.CardOffset)
                {
                    speedSliding = true;
                    return true;
                }
            }

            var buttonPositions = GetButtonPositions(_parameters, _position).ToArray();
            if(ButtonDrawing.MouseDown(_parameters, buttonPositions[0]))
            {
                _meta.InitTimeline();
                Behavior = TimelineBehavior.Idle;
                timeline.Time = 0;
                return true;
            }
            switch(Behavior)
            {
                case TimelineBehavior.Stopped:
                        return false;
                case TimelineBehavior.SteppingBack:
                    if (ButtonDrawing.MouseDown(_parameters, buttonPositions[2]))
                    {
                        timeline.Time = targetTime;
                        targetTime--;
                        return true;
                    }
                    break;
                case TimelineBehavior.Play:
                    if (ButtonDrawing.MouseDown(_parameters, buttonPositions[3]))
                    {
                        Behavior = TimelineBehavior.Pausing;
                        targetTime = (int)Math.Round(timeline.Time);
                        return true;
                    }
                    break;
                case TimelineBehavior.Pausing:
                    if (ButtonDrawing.MouseDown(_parameters, buttonPositions[3]))
                    {
                        Behavior = TimelineBehavior.Idle;
                        timeline.Time = targetTime;
                        return true;
                    }
                    break;
                case TimelineBehavior.Stepping:
                    if (ButtonDrawing.MouseDown(_parameters, buttonPositions[4]))
                    {
                        timeline.Time = targetTime;
                        targetTime++;
                        return true;
                    }
                    break;
            }
            var sliderPosition = GetSliderPosition(_parameters, timeline, _position);
            //if (Math.Abs((_parameters.MousePosition - sliderPosition).Y) < _parameters.CardOffset &&
            //    _position.X - _parameters.CardOffset <= _parameters.MousePosition.X &&
            //    _parameters.MousePosition.X <= _position.X + Width + _parameters.CardOffset)
            if ((_parameters.MousePosition - sliderPosition).SquaredLength < _parameters.CardOffset * _parameters.CardOffset)
            {
                if (Behavior != TimelineBehavior.Stopped)
                {
                    Behavior = TimelineBehavior.Idle;
                }
                sliding = true;
                return true;
            }
            if(ButtonDrawing.MouseDown(_parameters, buttonPositions[1]))
            {
                _meta.EditorMode = UserInterfaceMode.Editing;
                Behavior = TimelineBehavior.Stopped;
                _meta.BreakPoint = null;
                timeline.Time = 0;
                return true;
            }
            if (ButtonDrawing.MouseDown(_parameters, buttonPositions[2]))
            {
                Behavior = TimelineBehavior.SteppingBack;
                targetTime = (int)Math.Round(timeline.Time);
                timeline.Time = targetTime;
                targetTime--;
                return true;
            }
            if (ButtonDrawing.MouseDown(_parameters, buttonPositions[3]) && CanPlay(timeline))
            {
                Behavior = TimelineBehavior.Play;
                if(timeline.Time == timeline.MaxTime && timeline.LoopTime != -1)
                {
                    timeline.Time = timeline.LoopTime;
                }
                targetTime = timeline.MaxTime;
                return true;
            }
            if (ButtonDrawing.MouseDown(_parameters, buttonPositions[4]) && CanPlay(timeline))
            {
                Behavior = TimelineBehavior.Stepping;
                targetTime = (int)Math.Round(timeline.Time);
                timeline.Time = targetTime;
                if (timeline.Time == timeline.MaxTime && timeline.LoopTime != -1)
                {
                    timeline.Time = timeline.LoopTime;
                }
                targetTime = (int)timeline.Time + 1;
                return true;
            }
            if (ButtonDrawing.MouseDown(_parameters, buttonPositions[5]))
            {
                SpeedModified = !SpeedModified;
                return true;
            }
            return false;
        }

        public void MouseUp(Timeline _timeline)
        {
            if(sliding)
            {
                _timeline.Time = Math.Round(_timeline.Time);
            }
            sliding = false;
            speedSliding = false;
        }
    }
}
