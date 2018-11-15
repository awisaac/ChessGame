using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChessGame.Pieces
{
    class Knight : Piece
    {
        public Knight(PieceColor color, Board b, int index) : base(color, b, index)
        {
            Application.Current.Dispatcher.Invoke(new Action(AddImageSource));
        }

        private void AddImageSource()
        {
            if (Color == PieceColor.Black)
            {
                Enum = PieceEnum.BlackKnight;
            }
            else
            {
                Enum = PieceEnum.WhiteKnight;
            }
        }

        internal override List<Move> GetPotentialMoves()
        {
            List<Move> moves = new List<Move>();
            int row = Position.Row;
            int col = Position.Col;

            if (row < 6 && col > 0 && GameBoard.GetPiece(row + 2, col - 1).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row + 2, col - 1), this, GameBoard.GetPiece(row + 2, col - 1)));
            }

            if (row < 6 && col < 7 && GameBoard.GetPiece(row + 2, col + 1).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row + 2, col + 1), this, GameBoard.GetPiece(row + 2, col + 1)));
            }

            if (row > 1 && col > 0 && GameBoard.GetPiece(row - 2, col - 1).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row - 2, col - 1), this, GameBoard.GetPiece(row - 2, col - 1)));
            }

            if (row > 1 && col < 7 && GameBoard.GetPiece(row - 2, col + 1).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row - 2, col + 1), this, GameBoard.GetPiece(row - 2, col + 1)));
            }

            if (row < 7 && col > 1 && GameBoard.GetPiece(row + 1, col - 2).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row + 1, col - 2), this, GameBoard.GetPiece(row + 1, col - 2)));
            }

            if (row < 7 && col < 6 && GameBoard.GetPiece(row + 1, col + 2).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row + 1, col + 2), this, GameBoard.GetPiece(row + 1, col + 2)));
            }

            if (row > 0 && col > 1 && GameBoard.GetPiece(row - 1, col - 2).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row - 1, col - 2), this, GameBoard.GetPiece(row - 1, col - 2)));
            }

            if (row > 0 && col < 6 && GameBoard.GetPiece(row - 1, col + 2).Color != Color)
            {
                moves.Add(new Move(Position, new Position(row - 1, col + 2), this, GameBoard.GetPiece(row - 1, col + 2)));
            }

            return moves;
        }
    }
}
