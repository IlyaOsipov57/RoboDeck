using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    static class Loader
    {
        public static void SaveDeck(Deck _deck, String _fileName)
        {
            var data = SaveDeck(_deck);
            File.WriteAllLines(_fileName, data);
        }

        private static IEnumerable<String> SaveDeck(Deck _deck)
        {
            foreach(var card in _deck.Cards)
            {
                yield return String.Empty;
                yield return String.Format("{0},{1},{2}", Codes.CellCode(card.Data[0, 0]), Codes.CellCode(card.Data[0, 1]), Codes.CellCode(card.Data[0, 2]));
                yield return String.Format("{0},{1},{2}", Codes.CellCode(card.Data[1, 0]), Codes.ActionCode(card.Data[1, 1]), Codes.CellCode(card.Data[1, 2]));
            }
        }

        public static Deck LoadDeck(String _fileName)
        {
            return LoadDeck(File.ReadLines(_fileName));
        }

        private static Deck LoadDeck (IEnumerable<String> _data)
        {
            var result = new Deck();
            var enumerator = _data.GetEnumerator();
            var card = (Card) null;
            while (enumerator.MoveNext())
            {
                var split = enumerator.Current.Split(',');
                if(split.Length < 3)
                {
                    continue;
                }
                if(card == null)
                {
                    card = new Card();
                    card.Data[0, 0] = Codes.CellValue(split[0]);
                    card.Data[0, 1] = Codes.CellValue(split[1]);
                    card.Data[0, 2] = Codes.CellValue(split[2]);
                }
                else
                {
                    card.Data[1, 0] = Codes.CellValue(split[0]);
                    card.Data[1, 1] = Codes.ActionValue(split[1]);
                    card.Data[1, 2] = Codes.CellValue(split[2]);
                    result.Cards.Add(card);
                    card = null;
                }
            }
            return result;
        }

        public static void SaveStage(Stage _stage, String _fileName)
        {
            var data = SaveStage(_stage);
            File.WriteAllLines(_fileName, data);
        }

        private static IEnumerable<String> SaveStage(Stage _stage)
        {
            for (int y = 1; y < _stage.Cells.GetLength(1) - 1; y++)
            {
                var line = String.Empty;
                for (int x = 1; x < _stage.Cells.GetLength(0) - 1; x++)
                {
                    if(_stage.Cells[x,y])
                    {
                        line += Codes.Wall;
                        continue;
                    }
                    if(_stage.Entrance.X == x && _stage.Entrance.Y == y)
                    {
                        line += Codes.Start;
                        continue;
                    }
                    if(_stage.Exit.X == x && _stage.Exit.Y == y)
                    {
                        line += Codes.Finish;
                        continue;
                    }
                    line += Codes.Empty;
                }
                yield return line;
            }
        }

        public static Stage LoadStage(String _fileName)
        {
            return LoadStage(File.ReadLines(_fileName));
        }

        private static Stage LoadStage(IEnumerable<String> _data)
        {
            var s = IntPoint.Zero;
            var f = IntPoint.Zero;
            var field = new List<bool[]>();
            var enumerator = _data.GetEnumerator();
            {
                var y = 0;
                while (enumerator.MoveNext())
                {
                    y++;
                    var line = enumerator.Current.Trim();
                    field.Add(line.Select(c => c == Codes.Wall).ToArray());
                    var x = line.IndexOf('S');
                    if (x >= 0)
                    {
                        s = new IntPoint(x + 1, y);
                    }
                    x = line.IndexOf('F');
                    if (x >= 0)
                    {
                        f = new IntPoint(x + 1, y);
                    }
                }
            }

            if (s == IntPoint.Zero)
            {
                s = new IntPoint(1, 1);
                if (f == s)
                {
                    s = new IntPoint(2, 1);
                }
            }
            if (f == IntPoint.Zero)
            {
                f = new IntPoint(1, 1);
                if (s == f)
                {
                    f = new IntPoint(2, 1);
                }
            }

            var width = field.Max(r => r.Length) + 2;
            var height = field.Count + 2;

            var result = new Stage()
            {
                Entrance = s,
                Exit = f,
                Cells = new bool[width, height]
            };
            for (int y = 0; y < field.Count; y++)
            {
                for (int x = 0; x < field[y].Length; x++)
                {
                    result.Cells[x + 1, y + 1] = field[y][x];
                }
            }
            var newWidth = Math.Max(f.X, s.X) + 1;
            var newHeight = Math.Max(f.Y, s.Y) + 1;
            if (newHeight > result.Cells.GetLength(1) || newWidth > result.Cells.GetLength(0))
            {
                result.Resize(newWidth, newHeight);
            }
            else
            {
                result.InitBorder();
            }
            return result;
        }

        static class Codes
        {
            public static String CellCode(byte _value)
            {
                if (_value >= 7)
                {
                    return CellCodesReadable[0];
                }
                return CellCodesReadable[_value];
            }

            public static String ActionCode(byte _value)
            {
                if (_value >= 9)
                {
                    return ActionCodesReadable[0];
                }
                return ActionCodesReadable[_value];
            }

            public static byte CellValue(String _code)
            {
                byte result;
                if (InvCellCodes.TryGetValue(_code.Trim(), out result))
                {
                    return result;
                }
                return 7;
            }

            
            public static byte ActionValue(String _code)
            {
                byte result;
                if (InvActionCodes.TryGetValue(_code.Trim(), out result))
                {
                    return result;
                }
                return 4;
            }

            public static char Wall = 'W';
            public static char Empty = 'E';
            public static char Start = 'S';
            public static char Finish = 'F';

            private static String[] CellCodes = new String[]
        {
            "",
            "E",
            "W",
            "-R",
            "R",
            "-W",
            "-E",
            "",
        };

            private static String[] ActionCodes = new String[]
        {
            "T",
            "R",
            "U",
            "L",
            "W",
            "A",
            "S",
            "D",
            "X",
        };

            static Codes()
            {
                CellCodesReadable = CellCodes.Select(c => AddSpaces(c)).ToArray();
                ActionCodesReadable = ActionCodes.Select(c => AddSpaces(c)).ToArray();

                InvCellCodes = new Dictionary<String, byte>();
                for(int i = 1; i< CellCodes.Length; i++)
                {
                    InvCellCodes.Add(CellCodes[i], (byte)i);
                }
                InvActionCodes = new Dictionary<String, byte>();
                for (int i = 0; i < ActionCodes.Length; i++)
                {
                    InvActionCodes.Add(ActionCodes[i], (byte)i);
                }
            }

            private static String AddSpaces (String s)
            {
                if(s.Length == 0)
                {
                    return "  ";
                }
                if(s.Length == 1)
                {
                    return " " + s;
                }
                return s;
            }

            private static String[] CellCodesReadable;

            private static String[] ActionCodesReadable;

            private static Dictionary<String, byte> InvCellCodes;

            private static Dictionary<String, byte> InvActionCodes;
        }
    }
}
