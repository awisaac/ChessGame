using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame
{
    public class PieceLocation
    {
        int Row { get; }
        int Col { get; }
        PieceEnum Enum { get; }

        public PieceLocation(int row, int col, PieceEnum p)
        {
            Row = row;
            Col = col;
            Enum = p;
        }
    }
}
