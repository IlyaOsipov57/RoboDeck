using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    public enum UserInterfaceMode { Editing, Playing, Debugging }

    public class MetaState
    {
        public Timeline TheTimeline;
        public Stage TheStage;
        public Deck TheDeck;
        public UserInterfaceMode EditorMode;
        public int HighlightedRobo;
        public int HighlightedCard;
        public IntPoint? BreakPoint = null;

        public void OnDeckEdited()
        {
            if (EditorMode == UserInterfaceMode.Debugging)
            {
                EditorMode = UserInterfaceMode.Playing;
            }
        }

        public void ResetHighlighting()
        {
            HighlightedRobo = -1;
            HighlightedCard = -1;
        }

        public void InitTimeline()
        {
            var total = 6;
            var state = new StageState()
            {
                Robots = Enumerable.Range(0, total).Select(_ => new RoboState()
                {
                    Position = TheStage.Entrance,
                    Direction = RoboDirection.Spawning,
                }).ToArray(),
            };
            TheTimeline = new Timeline(TheStage, TheDeck, state, 1000);
            EditorMode = UserInterfaceMode.Debugging;
        }

        public void SelectTopCard(DrawingParameters _parameters, IntPoint _position)
        {
            if (HighlightedRobo == -1)
            {
                TryHighlightRobo(_parameters, _position);
            }
            var state = TheTimeline.GetState();
            if (HighlightedRobo != -1)
            {
                var robot = state.Robots[HighlightedRobo];
                if (robot.Direction == RoboDirection.Spawning || robot.Direction == RoboDirection.Despawned)
                    return;
                if (StateUpdater.TouchExit && robot.Position == TheStage.Exit)
                    return;
                var card = StateUpdater.ChooseCard(TheStage, StateUpdater.GetPositionHashset(state.Robots), TheDeck, robot);
                if (card == -1)
                {
                    HighlightedCard = -1;
                }
                else
                {
                    HighlightedCard = card;
                }
            }
        }

        private void TryHighlightRobo(DrawingParameters _parameters, IntPoint _position)
        {
            var mousePosition = _parameters.MousePosition - _position;
            var best = _parameters.BigGridStep * _parameters.BigGridStep / 2;
            var Best = -1;
            var n = TheTimeline.GetState().Robots.Length;
            for (int i = 0; i < n; i++)
            {
                var test = GetSquaredDistanceToRobo(_parameters, mousePosition, i);
                if (test < best)
                {
                    best = test;
                    Best = i;
                }
            }
            HighlightedRobo = Best;
        }

        private int GetSquaredDistanceToRobo(DrawingParameters _parameters, IntPoint _mousePosition, int _index)
        {
            var state = TheTimeline.GetState();
            var robot = state.Robots[_index];
            if (robot.Direction == RoboDirection.Spawning || robot.Direction == RoboDirection.Despawned)
                return int.MaxValue;
            var roboPosition = robot.Position * _parameters.BigGridStep;
            if (roboPosition.X - _parameters.BigGridStep > _mousePosition.X || _mousePosition.X > roboPosition.X + 2 * _parameters.BigGridStep ||
                roboPosition.Y - _parameters.BigGridStep > _mousePosition.Y || _mousePosition.Y > roboPosition.Y + 2 * _parameters.BigGridStep)
            {
                return int.MaxValue;
            }
            var stateProgress = TheTimeline.GetTime();
            var center = Glyphs.GetRoboCenter(IntPoint.Zero, _parameters.BigGridMultiplier, _parameters.BigGridStep, robot, state.Actions[_index], state.Failed[_index], stateProgress);
            return (center - _mousePosition).SquaredLength;
        }
    }
}
