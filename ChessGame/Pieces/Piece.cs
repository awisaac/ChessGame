using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace ChessGame.Pieces
{
    internal abstract class Piece
    {
        protected Board Board { get; set; }
        public Image Image { get; set; }
        public int MoveCount { get; set; }
        public PieceColor Color { get; set; }
        public PieceEnum Enum { get; set; }
        public Piece PromotedFrom { get; set; }

        protected Piece(PieceColor c, Board board)
        {
            Color = c;
            Board = board;
            MoveCount = 0;
            Image = new Image();
            PromotedFrom = null;
        }
        
        internal Position GetPosition()
        {
            return Board.GetPosition(this);
        }

        internal abstract List<Move> GetPotentialMoves();        
    }

    internal enum PieceColor
    {
        White, Black, Empty
    }
}