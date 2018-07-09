using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame.Pieces
{
    internal class Empty : Piece
    {
        public Empty(Board b) : base(PieceColor.Empty, b)
        {
            Enum = PieceEnum.EmptyPosition;
        }

        internal override List<Move> GetPotentialMoves()
        {
            return new List<Move>();
        }
    }    
}
