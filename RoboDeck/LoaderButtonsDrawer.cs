using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RoboDeck
{
    public enum LoaderRequest { LoadStage = 0, SaveStage = 1, LoadDeck = 2, SaveDeck = 3, None = 4, Load = 0, Save = 1, Stage = 0, Deck = 2, }
    class LoaderButtonsDrawer
    {
        bool loadPressed;
        bool savePressed;
        public void DrawButtons(Graphics _graphics, DrawingParameters _parameters, IntPoint _position, bool _disabled)
        {
            var delta = _parameters.BigGridStep + _parameters.CardOffset;
            var firstButtonPosition = _position + IntPoint.Left * delta;
            var secondButtonPosition = _position + IntPoint.DownLeft * delta;
            var state = _disabled ? ButtonState.Disabled : ButtonState.Normal;
            ButtonDrawing.DrawButton(_graphics, _parameters, firstButtonPosition, loadPressed ? ButtonState.Pressed : state, ButtonGlyph.Load);
            ButtonDrawing.DrawButton(_graphics, _parameters, secondButtonPosition, savePressed ? ButtonState.Pressed : state, ButtonGlyph.Save);
        }

        public void Update()
        {
            loadPressed = false;
            savePressed = false;
        }

        public LoaderRequest MouseDown(DrawingParameters _parameters, IntPoint _position, LoaderRequest _request)
        {
            var delta = _parameters.BigGridStep + _parameters.CardOffset;
            var firstButtonPosition = _position + IntPoint.Left * delta;
            var secondButtonPosition = _position + IntPoint.DownLeft * delta;
            if (ButtonDrawing.MouseDown(_parameters, firstButtonPosition))
            {
                loadPressed = true;
                return LoaderRequest.Load | _request;
            }
            if (ButtonDrawing.MouseDown(_parameters, secondButtonPosition))
            {
                savePressed = true;
                return LoaderRequest.Save | _request;
            }
            return LoaderRequest.None;
        }
    }
}
