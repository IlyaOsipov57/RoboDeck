using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    public enum ActionFail { DoNot = 0, Fail, FailFast };
    static class StateUpdater
    {
        public static bool TouchExit = true;

        public static void Update(Stage _stage, RoboState[] _states, Deck _deck, out RoboState[] _nextStates, out RoboAction[] _actions, out ActionFail[] _failed)
        {
            RoboState[] targetStates;
            FindTargetStates(_stage, _states, _deck, out targetStates, out _actions);
            _nextStates = new RoboState[_states.Length];
            _failed = new ActionFail[_states.Length];
            var resolved = new bool[_states.Length];
            var firstSpawn = _states.Length;
            var blocked = new HashSet<IntPoint>();
            #region First pass
            var inv = new Dictionary<IntPoint, int>();
            for (int i = 0; i < _states.Length; i++)
            {
                if(targetStates[i].Direction == RoboDirection.Despawned)
                {
                    _nextStates[i] = targetStates[i];
                    resolved[i] = true;
                    continue;
                }
                if (_states[i].Direction == RoboDirection.Spawning)
                {
                    blocked.Add(_states[i].Position);
                    firstSpawn = i;
                    break;
                }

                var nextPosition = targetStates[i].Position;

                if (nextPosition == _states[i].Position)
                {
                    _nextStates[i] = targetStates[i];
                    resolved[i] = true;
                    blocked.Add(_nextStates[i].Position);
                    continue;
                }

                if (_stage.Cells[nextPosition.X, nextPosition.Y])
                {
                    _nextStates[i] = _states[i];
                    resolved[i] = true;
                    _failed[i] = ActionFail.Fail;
                    blocked.Add(_nextStates[i].Position);
                    continue;
                }

                if(inv.ContainsKey(nextPosition))
                {
                    var j = inv[nextPosition];
                    if (targetStates[j].Position == _states[i].Position)
                    {
                        _nextStates[i] = _states[i];
                        resolved[i] = true;
                        _failed[i] = ActionFail.FailFast;
                        blocked.Add(_nextStates[i].Position);
                        _nextStates[j] = _states[j];
                        resolved[j] = true;
                        _failed[j] = ActionFail.FailFast;
                        blocked.Add(_nextStates[j].Position);
                        continue;
                    }
                }

                inv.Add(_states[i].Position, i);
            }
            #endregion
            #region Loop
            var hasChanged = false;
            do
            {
                hasChanged = false;
                var requested = new HashSet<IntPoint>();
                for (int i = 0; i < firstSpawn; i++)
                {
                    if (resolved[i])
                    {
                        continue;
                    }
                    var p = targetStates[i].Position;

                    if (blocked.Contains(p))
                    {
                        blocked.Add(_states[i].Position);
                        _nextStates[i] = _states[i];
                        resolved[i] = true;
                        _failed[i] = ActionFail.Fail;
                        hasChanged = true;
                    }
                    else
                    {
                        if (requested.Contains(p))
                        {
                            blocked.Add(p);
                            blocked.Add(_states[i].Position);
                            _nextStates[i] = _states[i];
                            resolved[i] = true;
                            _failed[i] = ActionFail.Fail;
                            hasChanged = true;
                        }
                        else
                        {
                            requested.Add(p);
                        }
                    }
                }
            } while (hasChanged);
            #endregion
            #region Final pass
            blocked = new HashSet<IntPoint>();
            for (int i = 0; i < firstSpawn; i++)
            {
                if (!resolved[i])
                {
                    _nextStates[i] = targetStates[i];
                }
            }
            
            if (firstSpawn != _states.Length)
            {
                if (firstSpawn == 0 || _nextStates[firstSpawn - 1].Position != _states[firstSpawn].Position || targetStates[firstSpawn - 1].Direction == RoboDirection.Despawned)
                {
                    _nextStates[firstSpawn] = targetStates[firstSpawn];
                    firstSpawn++;
                }
                for(int i = firstSpawn; i < _states.Length; i++)
                {
                    _nextStates[i] = _states[i];
                    _failed[i] = ActionFail.Fail;
                }
            }
            #endregion
        }

        private static void FindTargetStates(Stage _stage, RoboState[] _states, Deck _deck, out RoboState[] _targetStates, out RoboAction[] _actions)
        {
            var robots = GetPositionHashset(_states);
            _targetStates = new RoboState[_states.Length];
            _actions = new RoboAction[_states.Length];
            for (int i = 0; i < _states.Length; i++ )
            {
                var state = _states[i];
                var action = ChooseAction(_stage, robots, _deck, state);
                _actions[i] = action;
                _targetStates[i] = ApplyAction(state, action);
            }
        }

        public static HashSet<IntPoint> GetPositionHashset(RoboState[] _states)
        {
            return new HashSet<IntPoint>(_states.Where(s => s.Direction != RoboDirection.Despawned).Select(s => s.Position));
        }

        public static RoboState ApplyAction(RoboState _state, RoboAction action)
        {
            return new RoboState()
            {
                Direction = Direction[(int)_state.Direction, (int)action],
                Position = _state.Position + Movement[(int)_state.Direction, (int)action]
            };
        }

        private static IntPoint[,] Movement = new IntPoint[,]{
            {IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Up,    IntPoint.Right, IntPoint.Down,  IntPoint.Left,  IntPoint.Zero, IntPoint.Zero},
            {IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Right, IntPoint.Down,  IntPoint.Left,  IntPoint.Up,    IntPoint.Zero, IntPoint.Zero},
            {IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Down,  IntPoint.Left,  IntPoint.Up,    IntPoint.Right, IntPoint.Zero, IntPoint.Zero},
            {IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Left,  IntPoint.Up,    IntPoint.Right, IntPoint.Down,  IntPoint.Zero, IntPoint.Zero},
            {IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Zero,  IntPoint.Zero,  IntPoint.Zero,  IntPoint.Zero,  IntPoint.Zero, IntPoint.Zero},
            {IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Zero, IntPoint.Zero,  IntPoint.Zero,  IntPoint.Zero,  IntPoint.Zero,  IntPoint.Zero, IntPoint.Zero},
        };
        private static RoboDirection[,] Direction = new RoboDirection[,]{
            {RoboDirection.Up,        RoboDirection.Right,     RoboDirection.Down,      RoboDirection.Left,      RoboDirection.Up,        RoboDirection.Up,        RoboDirection.Up,        RoboDirection.Up,        RoboDirection.Despawned, RoboDirection.Up       },
            {RoboDirection.Right,     RoboDirection.Down,      RoboDirection.Left,      RoboDirection.Up,        RoboDirection.Right,     RoboDirection.Right,     RoboDirection.Right,     RoboDirection.Right,     RoboDirection.Despawned, RoboDirection.Right    },
            {RoboDirection.Down,      RoboDirection.Left,      RoboDirection.Up,        RoboDirection.Right,     RoboDirection.Down,      RoboDirection.Down,      RoboDirection.Down,      RoboDirection.Down,      RoboDirection.Despawned, RoboDirection.Down     },
            {RoboDirection.Left,      RoboDirection.Up,        RoboDirection.Right,     RoboDirection.Down,      RoboDirection.Left,      RoboDirection.Left,      RoboDirection.Left,      RoboDirection.Left,      RoboDirection.Despawned, RoboDirection.Left     },
            {RoboDirection.Up,        RoboDirection.Up,        RoboDirection.Up,        RoboDirection.Up,        RoboDirection.Up,        RoboDirection.Up,        RoboDirection.Up,        RoboDirection.Up,        RoboDirection.Despawned, RoboDirection.Up       },
            {RoboDirection.Despawned, RoboDirection.Despawned, RoboDirection.Despawned, RoboDirection.Despawned, RoboDirection.Despawned, RoboDirection.Despawned, RoboDirection.Despawned, RoboDirection.Despawned, RoboDirection.Despawned, RoboDirection.Despawned},
        };

        private static RoboAction ChooseAction(Stage _stage, HashSet<IntPoint> _robots, Deck _deck, RoboState _state)
        {
            if (_state.Direction == RoboDirection.Despawned)
                return RoboAction.Wait;
            if (_state.Direction == RoboDirection.Spawning)
                return RoboAction.Spawn;
            if (TouchExit && _state.Position == _stage.Exit)
            {
                return RoboAction.Terminate;
            }
            var cardIndex = ChooseCard(_stage, _robots, _deck, _state);
            if(cardIndex == -1)
            {
                return RoboAction.Wait;
            }
            return (RoboAction)_deck.Cards[cardIndex].Data[Card.ActionSquare.X, Card.ActionSquare.Y];
        }

        public static int ChooseCard(Stage _stage, HashSet<IntPoint> _robots, Deck _deck, RoboState _state)
        {
            var neighbourhood = Shift[(int)_state.Direction].Select(s =>
            {
                var p = _state.Position + s;
                if (_stage.Cells[p.X, p.Y])
                    return (byte)CellValue.Wall;
                if (_robots.Any(r => r == p))
                    return (byte)CellValue.Robo;
                return (byte)CellValue.Empty;
            }).ToArray();
            for (int i = 0; i < _deck.Cards.Count; i++)
            {
                var card = _deck.Cards[i];
                if ((card.Data[0, 0] & neighbourhood[0]) == 0)
                    continue;
                if ((card.Data[0, 1] & neighbourhood[1]) == 0)
                    continue;
                if ((card.Data[0, 2] & neighbourhood[2]) == 0)
                    continue;
                if ((card.Data[1, 0] & neighbourhood[3]) == 0)
                    continue;
                if ((card.Data[1, 2] & neighbourhood[4]) == 0)
                    continue;
                return i;
            }
            return -1;
        }
        private static IntPoint[][] Shift = new IntPoint[][]{
            new IntPoint[]{
            IntPoint.UpLeft,    IntPoint.Up,    IntPoint.UpRight,
            IntPoint.Left,                      IntPoint.Right},
            new IntPoint[]{
            IntPoint.UpRight,   IntPoint.Right, IntPoint.DownRight,
            IntPoint.Up,                        IntPoint.Down},
            new IntPoint[]{
            IntPoint.DownRight, IntPoint.Down,  IntPoint.DownLeft,
            IntPoint.Right,                     IntPoint.Left},
            new IntPoint[]{
            IntPoint.DownLeft,  IntPoint.Left,  IntPoint.UpLeft,
            IntPoint.Down,                      IntPoint.Up},
        };
    }
}
