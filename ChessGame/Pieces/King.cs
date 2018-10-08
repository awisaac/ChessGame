using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChessGame.Pieces
{
    public class King : Piece
    {
        public King(PieceColor color, Board b, int index) : base(color, b, index)
        {
            if (Color == PieceColor.Black)
            {
                Enum = PieceEnum.BlackKing;
            }
            else
            {
                Enum = PieceEnum.WhiteKing;
            }
        }

        internal override List<Move> GetPotentialMoves()
        {
            List<Move> moves = new List<Move>();

            int row = Position.Row;
            int col = Position.Col;

            if (row > 0 && col > 0 && GameBoard.GetPiece(row - 1, col - 1).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row - 1, col - 1), this, GameBoard.GetPiece(row - 1, col - 1), false));
            }

            if (row < 7 && col < 7 && GameBoard.GetPiece(row + 1, col + 1).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row + 1, col + 1), this, GameBoard.GetPiece(row + 1, col + 1), false));
            }

            if (row > 0 && col < 7 && GameBoard.GetPiece(row - 1, col + 1).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row - 1, col + 1), this, GameBoard.GetPiece(row - 1, col + 1), false));
            }

            if (col > 0 && row < 7 && GameBoard.GetPiece(row + 1, col - 1).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row + 1, col - 1), this, GameBoard.GetPiece(row + 1, col - 1), false));
            }

            if (col < 7 && GameBoard.GetPiece(row, col + 1).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row, col + 1), this, GameBoard.GetPiece(row, col + 1), false));
            }
            
            if (col > 0 && GameBoard.GetPiece(row, col - 1).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row, col - 1), this, GameBoard.GetPiece(row, col - 1), false));
            }

            if (row < 7 && GameBoard.GetPiece(row + 1, col).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row + 1, col), this, GameBoard.GetPiece(row + 1, col), false));
            }

            if (row > 0 && GameBoard.GetPiece(row - 1, col).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row - 1, col), this, GameBoard.GetPiece(row - 1, col), false));
            }

            if (MoveCount == 0 && GameBoard.GetPiece(row, 0).MoveCount == 0 && GameBoard.GetPiece(row, 1) is Empty
                && GameBoard.GetPiece(row, 2) is Empty && GameBoard.GetPiece(row, 3) is Empty)
            {
                Move kingMove = new Move(Position, new Position(row, 2), this, GameBoard.GetPiece(row, 2), false);
                kingMove.CastleMove = new Move(new Position(row, 0), new Position(row, 3), GameBoard.GetPiece(row, 0), GameBoard.GetPiece(row, 3), false);
                kingMove.Castle = true;
                moves.Add(kingMove);
            }

            if (MoveCount == 0 && GameBoard.GetPiece(row, 7).MoveCount == 0 && GameBoard.GetPiece(row, 5) is Empty
                && GameBoard.GetPiece(row, 6) is Empty)
            {
                Move kingMove = new Move(Position, new Position(row, 6), this, GameBoard.GetPiece(row, 6), false);
                kingMove.CastleMove = new Move(new Position(row, 7), new Position(row, 5), GameBoard.GetPiece(row, 7), GameBoard.GetPiece(row, 5), false);
                kingMove.Castle = true;
                moves.Add(kingMove);
            }

            return moves;
        }
    }
}
