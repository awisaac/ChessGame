using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChessGame.Pieces
{
    class Rook : Piece
    {
        public Rook(PieceColor color, Board b, int index) : base(color, b, index)
        {
            if (Color == PieceColor.Black)
            {
                Enum = PieceEnum.BlackRook;
            }
            else
            {
                Enum = PieceEnum.WhiteRook;
            }
        }
        
        internal override List<Move> GetPotentialMoves()
        {
            List<Move> moves = new List<Move>();
            int row = Position.Row + 1;
            int col = Position.Col;

            while (row <= 7 && GameBoard.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col)));
                row++;
            }

            if (row <= 7 && GameBoard.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col)));
            }

            row = Position.Row - 1;

            while (row >= 0 && GameBoard.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col)));
                row--;
            }

            if (row >= 0 && GameBoard.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col)));
            }

            row = Position.Row;
            col = Position.Col + 1;

            while (col <= 7 && GameBoard.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col)));
                col++;
            }

            if (col <= 7 && GameBoard.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col)));
            }

            col = Position.Col - 1;

            while (col >= 0 && GameBoard.GetPiece(row, col) is Empty)
            {
                moves.Add(new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col)));
                col--;
            }

            if (col >= 0 && GameBoard.GetPiece(row, col).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col)));
            }

            return moves;
        }
    }
}
