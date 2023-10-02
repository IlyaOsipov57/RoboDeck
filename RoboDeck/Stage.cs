using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    public class Stage
    {
        public bool[,] Cells;
        public IntPoint Entrance;
        public IntPoint Exit;

        public void Resize(int newX, int newY)
        {
            var newCells = new bool[newX, newY];
            for (int x = 1; x < Math.Min(newX, Cells.GetLength(0)) - 1; x++)
            {
                for (int y = 1; y < Math.Min(newY, Cells.GetLength(1)) - 1; y++)
                {
                    newCells[x, y] = Cells[x, y];
                }
            }
            Cells = newCells;
            InitBorder();
        }

        public void InitBorder()
        {
            var width = Cells.GetLength(0);
            var height = Cells.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                Cells[x, 0] = true;
                Cells[x, height - 1] = true;
            }
            for (int y = 1; y < height - 1; y++)
            {
                Cells[0, y] = true;
                Cells[width - 1, y] = true;
            }
        }
    }

    public struct StageState
    {
        public RoboState[] Robots;
        public RoboAction[] Actions;
        public ActionFail[] Failed;

        public StageState Next(Stage _stage, Deck _deck)
        {
            var next = new StageState();
            StateUpdater.Update(_stage, Robots, _deck, out next.Robots, out Actions, out Failed);
            return next;
        }

        private RoboState[] Ordered;

        public void PrepareForHashing()
        {
            Ordered = Robots.OrderBy(r => r.Position.Y).ThenBy(r => r.Position.X).ThenBy(r => r.Direction).ToArray();
        }

        public void ClearCache()
        {
            Ordered = null;
        }

        public override int GetHashCode()
        {
            var h = 0;
            foreach (var r in Ordered)
            {
                h = new Tuple<int, int, int, RoboDirection>(h, r.Position.X, r.Position.Y, r.Direction).GetHashCode();
            }
            return h;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is StageState))
            {
                return false;
            }
            var other = (StageState)obj;
            if(Ordered.Length != other.Ordered.Length)
            {
                return false;
            }
            for(int i =0; i< Ordered.Length; i++)
            {
                var a = Ordered[i];
                var b = other.Ordered[i];
                if (a.Position.X != b.Position.X || a.Position.Y != b.Position.Y || a.Direction != b.Direction)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public struct RoboState
    {
        public IntPoint Position;
        public RoboDirection Direction;
    }
}
