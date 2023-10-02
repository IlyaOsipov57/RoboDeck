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
    class DeckDrawer
    {
        class DragState
        {
            public int DraggedCardClickX
            {
                get;
                private set;
            }
            public int DraggedCardIndex
            {
                get;
                private set;
            }
            public bool IsDragging
            {
                get;
                private set;
            }
            public void StartDrag(DrawingParameters _parameters, int _cardPosition, int _index)
            {
                DraggedCardIndex = _index;
                DraggedCardClickX = _parameters.MousePosition.X - _cardPosition;
                IsDragging = true;
            }

            public int GetDraggedPosition(DrawingParameters _parameters)
            {
                return _parameters.MousePosition.X - DraggedCardClickX;
            }

            public void StopDrag ()
            {
                IsDragging = false;
            }

            public int GetDraggedCardIndex()
            {
                if(IsDragging)
                {
                    return DraggedCardIndex;
                }
                return -1;
            }
        }

        private DragState dragState = new DragState();
        private List<float> CardPositions = new List<float>();
        private int TopCard = 0;
        public int Width;

        private float LeftOver = 0;
        private double TimeOut = 0;

        public DeckDrawer (int _width)
        {
            this.Width = _width;
        }

        public void DrawDeck (Graphics _graphics, DrawingParameters _parameters, MetaState _meta, IntPoint _position)
        {
            _graphics.FillRectangle(Brushes.White, _position.X, _position.Y, Width, _parameters.DeckHeight);

            foreach (var index in EnumerateCardsTopDown(CardPositions.Count).Reverse())
            {
                var cardPositionX = (int)Math.Round(CardPositions[index]);
                var cardBorderPosition = new IntPoint(_position.X + cardPositionX, _position.Y);
                var showTopCard = index == TopCard && ShowTopCard(_parameters, _position);
                CardDrawing.DrawCardBorder(_graphics, _parameters, cardBorderPosition, showTopCard || index == _meta.HighlightedCard, showTopCard);
                var cardPosition = new IntPoint(_position.X + cardPositionX, _position.Y + _parameters.CardOffset);
                CardDrawing.DrawCard(_graphics, _parameters, _meta.TheDeck.Cards[index], cardPosition, DrawButtons(_parameters, _position, index));
            }
            CardDrawing.DrawPlusCard(_graphics, _parameters, _position);
            CardDrawing.DrawPlusCard(_graphics, _parameters, new IntPoint(_position.X + Width - _parameters.BigGridStep, _position.Y));
        }

        private bool DrawButtons(DrawingParameters _parameters, IntPoint _position, int index)
        {
            if (_parameters.MousePosition.X < _position.X + _parameters.BigGridStep || _position.X + Width - _parameters.BigGridStep < _parameters.MousePosition.X)
            {
                return false;
            }
            return index == TopCard && !dragState.IsDragging;
        }

        public void Update(DrawingParameters _parameters, MetaState _meta, double _deltaTime, IntPoint _position)
        {
            var deck = _meta.TheDeck;
            UpdateLength(deck.Cards.Count);
            if(_meta.HighlightedRobo != -1)
            {
                if (_meta.HighlightedCard == -1)
                {
                    TopCard = deck.Cards.Count;
                }
                else
                {
                    TopCard = _meta.HighlightedCard;
                }
            }
            if(dragState.IsDragging)
            {
                UpdateDraggedCard(_parameters, deck, _position);
                TopCard = dragState.DraggedCardIndex;
            }
            else
            {
                if (ShowTopCard(_parameters, _position))
                {
                    SelectTopCard(_parameters, _position);
                }
            }

            var tickMultiplier = GetTickMultiplier(_deltaTime);
            var targets = GetTargetPositions(_parameters, TopCard);
            for(int i = 0; i< CardPositions.Count; i++)
            {
                if (dragState.GetDraggedCardIndex() == i)
                {
                    continue;
                }
                var shift = (targets[i] - CardPositions[i]) * tickMultiplier;
                if (TimeOut > 0)
                {
                    shift *= 2;
                }
                if (Math.Abs(shift) < _deltaTime * 100)
                {
                    var dist = targets[i] - CardPositions[i];
                    shift = Math.Sign(dist) * Math.Min((float)_deltaTime * 100, Math.Abs(dist));
                }
                CardPositions[i] += shift;
                if (TopCard == i)
                {
                    if (ShowTopCard(_parameters, _position) &&
                        TimeOut <= 0 &&
                        _parameters.MousePosition.X > _position.X + CardPositions[i] &&
                        _position.X + CardPositions[i] + _parameters.CardWidth > _parameters.MousePosition.X &&
                        _parameters.MousePosition.X > _position.X + _parameters.BigGridStep &&
                        _parameters.MousePosition.X < _position.X + Width - _parameters.BigGridStep)
                    {
                        var cursorShift = shift + LeftOver;
                        var applicable = (int)cursorShift;
                        LeftOver = cursorShift - applicable;
                        Cursor.Position = new Point(Cursor.Position.X + applicable, Cursor.Position.Y);
                    }
                }
            }
            TimeOut = Math.Max(0, TimeOut - _deltaTime);
        }

        private float GetTickMultiplier(double _deltaTime)
        {
            var ticks = _deltaTime * 30;
            return (float)(1 - Math.Pow(0.9F, ticks));
        }

        private bool ShowTopCard (DrawingParameters _parameters, IntPoint _position)
        {
            if (_parameters.MousePosition.Y < _position.Y || _position.Y + _parameters.DeckHeight < _parameters.MousePosition.Y)
            {
                return false;
            }
            if (_parameters.MousePosition.X < _position.X || _position.X + Width < _parameters.MousePosition.X)
            {
                return false;
            }
            return GameForm.AllowCursorControl;
        }

        private void SelectTopCard(DrawingParameters _parameters, IntPoint _position)
        {
            if (CardPositions.Count == 0)
            {
                return;
            }
            if (_parameters.MousePosition.Y < _position.Y || _position.Y + _parameters.DeckHeight < _parameters.MousePosition.Y)
            {
                return;
            }
            if (_parameters.MousePosition.X < _position.X || _position.X + Width < _parameters.MousePosition.X)
            {
                return;
            }
            foreach (var index in EnumerateCardsTopDown(CardPositions.Count))
            {
                var cardPositionX = _position.X + (int)CardPositions[index];
                if (cardPositionX <= _parameters.MousePosition.X && _parameters.MousePosition.X <= cardPositionX + _parameters.CardWidth)
                {
                    TopCard = index;
                    return;
                }
            }
            var minX = _position.X + CardPositions[0];
            if(_parameters.MousePosition.X < minX)
            {
                TopCard = 0;
                return;
            }
            var maxX = _position.X + CardPositions[CardPositions.Count - 1] + _parameters.CardWidth;
            if(_parameters.MousePosition.X > maxX)
            {
                TopCard = CardPositions.Count - 1;
                return;
            }
        }

        private void UpdateDraggedCard(DrawingParameters _parameters, Deck _deck, IntPoint _position)
        {
            var draggedCardPosition = dragState.GetDraggedPosition(_parameters);
            if (CardPositions.Count > 1)
            {
                var index = GetBestIndex(_parameters, draggedCardPosition - _position.X);
                
                if (index != dragState.DraggedCardIndex)
                {
                    var card = _deck.Cards[dragState.DraggedCardIndex];
                    _deck.Cards.RemoveAt(dragState.DraggedCardIndex);
                    _deck.Cards.Insert(index, card);

                    CardPositions.RemoveAt(dragState.DraggedCardIndex);
                    CardPositions.Insert(index, draggedCardPosition - _position.X);
                    dragState.StartDrag(_parameters, draggedCardPosition, index);
                    return;
                }
            }
            CardPositions[dragState.DraggedCardIndex] = draggedCardPosition - _position.X;
        }

        private int GetBestIndex(DrawingParameters _parameters, int draggedCardPosition)
        {
            //var step = GetStep(_parameters);
            //var index = (int)Math.Round((draggedCardPosition - _position.X - _parameters.BigGridStep) / step);
            //index = Math.Max(0, index);
            //index = Math.Min(CardPositions.Count - 1, index);

            var targetPositions = GetTopCardPositions(_parameters);
            var bestIndex = 0;
            var best = Width;
            for (int i = 0; i < targetPositions.Length; i++)
            {
                var test = Math.Abs(draggedCardPosition - targetPositions[i]);
                if (test < best)
                {
                    best = test;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }
        
        private int[] GetTopCardPositions(DrawingParameters _parameters)
        {
            int[] result = new int[CardPositions.Count];
            for(int i = 0; i < CardPositions.Count; i++)
            {
                result[i] = GetTargetPositions(_parameters, i)[i];
            }
            return result;
        }
        private int[] GetTargetPositions(DrawingParameters _parameters, int topCard)
        {
            if (topCard < 0)
                topCard = 0;
            if (topCard >= CardPositions.Count)
                topCard = CardPositions.Count - 1;
            var cardWidthWithExtraSpace = _parameters.CardWidth + _parameters.ExtraSpace;
            if (Width > cardWidthWithExtraSpace * CardPositions.Count + _parameters.ExtraSpace + 2 * _parameters.BigGridStep || CardPositions.Count == 0)
            {
                return CardPositions.Select((_, i) => (i * cardWidthWithExtraSpace + _parameters.BigGridStep) + _parameters.ExtraSpace).ToArray();
            }
            var maxFullWidthCards = (Width - 2 * _parameters.BigGridStep - cardWidthWithExtraSpace - _parameters.ExtraSpace) / cardWidthWithExtraSpace + 1;
            var buffer = Math.Max(0, maxFullWidthCards / 2 - 1);
            buffer = Math.Min(buffer, 3);
            var firstCard = Math.Min(Math.Max(topCard - buffer, 0), CardPositions.Count - 2 * buffer - 1);
            var smallStep = (float)(Width - cardWidthWithExtraSpace * 2 * buffer - _parameters.ExtraSpace - _parameters.CardWidth - 2 * _parameters.BigGridStep) / (CardPositions.Count - 2 * buffer - 1);


            int[] result = new int[CardPositions.Count];
            int index = 0;
            float position = _parameters.BigGridStep + _parameters.ExtraSpace /2;
            while(index < firstCard)
            {
                result[index] = (int)position;
                position += smallStep;
                index++;
            }
            while(index < firstCard + 2*buffer)
            {
                result[index] = (int)position;
                position += cardWidthWithExtraSpace;
                index++;
            }
            while (index < CardPositions.Count)
            {
                result[index] = (int)position;
                position += smallStep;
                index++;
            }
            return result;
        }

        //private int[] GetTargetPositions(DrawingParameters _parameters, int topCard)
        //{
        //    var step = GetStep(_parameters);
        //    return CardPositions.Select((_, i) => (int)(i * step + _parameters.BigGridStep)).ToArray();
        //}

        //private float GetStep(DrawingParameters _parameters)
        //{
        //    if (Width < _parameters.CardWidth * CardPositions.Count + 2 * _parameters.BigGridStep && CardPositions.Count > 0)
        //    {
        //        return (float)(Width - _parameters.CardWidth - 2 * _parameters.BigGridStep) / (CardPositions.Count - 1);
        //    }
        //    else
        //    {
        //        return _parameters.CardWidth;
        //    }
        //}

        private void UpdateLength(int n)
        {
            if (CardPositions.Count > n)
            {
                CardPositions.RemoveRange(n, CardPositions.Count - n);
                if (TopCard > n)
                {
                    TopCard = n - 1;
                }
                if (dragState.GetDraggedCardIndex() > n)
                {
                    dragState.StopDrag();
                }
            }
            else
            {
                while (CardPositions.Count < n)
                {
                    CardPositions.Add(0);
                }
            }
        }

        public bool MouseDown(DrawingParameters _parameters, MetaState _meta, IntPoint _position)
        {
            var r = DoMouseDown(_parameters, _meta.TheDeck, _position);
            if(r)
            {
                _meta.OnDeckEdited();
            }
            return r;
        }

        private bool DoMouseDown(DrawingParameters _parameters, Deck _deck, IntPoint _position)
        {
            if(_parameters.MousePosition.Y < _position.Y || _position.Y + _parameters.DeckHeight < _parameters.MousePosition.Y)
            {
                return false;
            }
            if (_parameters.MousePosition.X < _position.X || _position.X + Width < _parameters.MousePosition.X)
            {
                return false;
            }

            if (_position.X + _parameters.BigGridStep > _parameters.MousePosition.X)
            {
                dragState.StartDrag(_parameters, _position.X, 0);
                _deck.Cards.Insert(0, new Card());
                CardPositions.Insert(0, 0);
                return true;
            }
            if (_position.X + Width - _parameters.BigGridStep < _parameters.MousePosition.X)
            {
                dragState.StartDrag(_parameters, _position.X + Width - _parameters.CardWidth, _deck.Cards.Count);
                _deck.Cards.Add(new Card());
                CardPositions.Add(Width - _parameters.CardWidth);
                return true;
            }

            var length = _deck.Cards.Count;
            UpdateLength(length);
            foreach(var index in EnumerateCardsTopDown(length))
            {
                var cardPosition = new IntPoint(_position.X + (int)CardPositions[index], _position.Y + _parameters.CardOffset);
                if (cardPosition.X <= _parameters.MousePosition.X && _parameters.MousePosition.X <= cardPosition.X + _parameters.CardWidth)
                {
                    if (CardDrawing.MouseDown(_parameters, _deck.Cards[index], cardPosition))
                    {
                        return true;
                    }
                    if(cardPosition.X + _parameters.CardWidth - _parameters.CardOffset <= _parameters.MousePosition.X)
                    {
                        _deck.Cards.RemoveAt(index);
                        CardPositions.RemoveAt(index);
                        SelectTopCard(_parameters, _position);
                        TimeOut = 1;
                        return true;
                    }
                    dragState.StartDrag(_parameters, cardPosition.X, index);
                    return true;
                }
            }

            return false;
        }

        private IEnumerable<int> EnumerateCardsTopDown (int _length)
        {
            if(_length == 0)
            {
                yield break;
            }
            var index = Math.Min(_length - 1, Math.Max(0, TopCard));
            index = Math.Min(index, _length - 1);
            var a = index * 2 - 1;
            var b = index * 2;
            while(true)
            {
                yield return index;
                index = a - index;
                if (index < 0)
                {
                    for (int i = b - index; i < _length; i++)
                    {
                        yield return i;
                    }
                    yield break;
                }
                yield return index;
                index = b - index;
                if (index >= _length)
                {
                    for (int i = a - index; i >= 0; i--)
                    {
                        yield return i;
                    }
                    yield break;
                }
            }
        }

        public void MouseUp()
        {
            dragState.StopDrag();
            TimeOut = 0.5;
        }
    }
}
