using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ChessGame.Pieces
{
    public abstract class Piece
    {
        internal Board GameBoard { get; set; }
        public int MoveCount { get; set; }
        public PieceColor Color { get; set; }
        public PieceEnum Enum { get; set; }
        public Position Position { get; set; }
        public int Index { get; set; }

        public Piece(PieceColor c, Board board, int index)
        {
            Color = c;
            GameBoard = board;
            MoveCount = 0;
            Index = index;
        }

        internal abstract List<Move> GetPotentialMoves();        
    }

    public enum PieceColor
    {
        White, Black, Empty
    }
}