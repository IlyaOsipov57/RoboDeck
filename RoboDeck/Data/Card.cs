using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoboDeck
{
    public class Card
    {
        public static IntPoint ActionSquare = new IntPoint(1, 1);

        public byte[,] Data = new byte[2, 3] {
            {7,7,7},
            {7,4,7}
        };
    }
}
