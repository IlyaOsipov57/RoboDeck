using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    public class DrawingParameters
    {
        public DrawingParameters(IntPoint _mousePosition, int _multiplier)
        {
            this.MousePosition = _mousePosition;
            this.Multiplier = _multiplier;
        }

        public IntPoint MousePosition;

        private int multiplier;

        public int Multiplier
        {
            get
            {
                return multiplier;
            }
            set
            {
                multiplier = value;
                BigGridMultiplier = multiplier * 3;
                GridStep = 6 * multiplier;
                BigGridStep = GridStep * 3;
                CardOffset = BigGridStep / 2;
                DeckHeight = BigGridStep * 5 / 2;
                CardWidth = BigGridStep * 3;
                ExtraSpace = GridStep;
            }
        }
        public int ExtraSpace
        {
            get;
            private set;
        }
        public int DeckHeight
        {
            get;
            private set;
        }
        public int CardOffset
        {
            get;
            private set;
        }
        public int CardWidth
        {
            get;
            private set;
        }
        public int BigGridMultiplier
        {
            get;
            private set;
        }
        public int GridStep
        {
            get;
            private set;
        }
        public int BigGridStep
        {
            get;
            private set;
        }
    }
}
