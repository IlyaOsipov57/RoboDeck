using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    public class Timeline
    {
        public int MaxTime
        {
            get;
            private set;
        }
        public double Time = 0;
        private StageState[] States;
        public int LoopTime = -1;

        public StageState GetState ()
        {
            var t = Math.Min(MaxTime, Math.Max(0, (int)Time));
            return States[t];
        }

        public float GetTime ()
        {
            var time = Math.Min(MaxTime, Math.Max(0, (float)Time));
            return time - (int)time;
        }

        public Timeline(Stage _stage, Deck _deck, StageState _startState, int _maxTime)
        {
            MaxTime = _maxTime;
            States = new StageState[_maxTime + 1];
            States[0] = _startState;
            var h = new Dictionary<StageState, int>();
            _startState.PrepareForHashing();
            h.Add(_startState, 0);
            for(int i = 1; i <= MaxTime; i++)
            {
                var nextState = States[i - 1].Next(_stage, _deck);
                States[i] = nextState;
                if(nextState.Robots.All(r => r.Direction == RoboDirection.Despawned))
                {
                    MaxTime = i;
                    break;
                }
                nextState.PrepareForHashing();
                if(h.ContainsKey(nextState))
                {
                    MaxTime = i;
                    LoopTime = h[nextState];
                    break;
                }
                h.Add(nextState, i);
            }
            States[MaxTime].Next(_stage, _deck);
            if(States[MaxTime].Actions.All(a => a == RoboAction.Wait || a == RoboAction.Spawn))
            {
                LoopTime = -1;
            }
            foreach(var s in States)
            {
                s.ClearCache();
            }
        }

        private bool StatesAreSame(RoboState[] _oldRoboStates, RoboState[] _newRoboStates)
        {
            for (int i = 0; i < _oldRoboStates.Length; i++)
            {
                if (_oldRoboStates[i].Position != _newRoboStates[i].Position ||
                    _oldRoboStates[i].Direction != _newRoboStates[i].Direction)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckBreakPoint(int _time, IntPoint _point)
        {
            return States[_time].Robots.Any(r => r.Position == _point);
        }
    }
}
