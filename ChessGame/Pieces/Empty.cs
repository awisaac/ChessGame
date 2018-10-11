using System.Collections.Generic;

namespace ChessGame.Pieces
{
    internal class Empty : Piece
    {
        public Empty(int row, int col, GameEngine engine, Board b) : base(PieceColor.Empty, engine, b, -1)
        {
            Position = new Position(row, col);
            Enum = PieceEnum.EmptyPosition;
        }

        internal override List<Move> GetPotentialMoves()
        {
            return new List<Move>();
        }
    }    
}
