using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ChessGame.Pieces
{
    class Queen : Piece
    {
        public Queen(PieceColor color, GameEngine engine, Board b, int index) : base(color, engine, b, index)
        {
            if (Color == PieceColor.Black)
            {
                Enum = PieceEnum.BlackQueen;
            }
            else
            {
                Enum = PieceEnum.WhiteQueen;
            }
        }

        internal override List<Move> GetPotentialMoves()
        {
            List<Move> moves = new List<Move>();

            int row = Position.Row - 1;
            int col = Position.Col - 1;

            while (row >= 0 && col >= 0 && GameBoard.GetPiece(row, col) is Empty)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
                row--;
                col--;
            }

            if (row >= 0 && col >= 0 && GameBoard.GetPiece(row, col).Color != Color)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
            }

            row = Position.Row - 1;
            col = Position.Col + 1;

            while (row >= 0 && col <= 7 && GameBoard.GetPiece(row, col) is Empty)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
                row--;
                col++;
            }

            if (row >= 0 && col <= 7 && GameBoard.GetPiece(row, col).Color != Color)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
            }

            row = Position.Row + 1;
            col = Position.Col - 1;

            while (row <= 7 && col >= 0 && GameBoard.GetPiece(row, col) is Empty)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
                row++;
                col--;
            }

            if (row <= 7 && col >= 0 && GameBoard.GetPiece(row, col).Color != Color)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
            }

            row = Position.Row + 1;
            col = Position.Col + 1;

            while (row <= 7 && col <= 7 && GameBoard.GetPiece(row, col) is Empty)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
                row++;
                col++;
            }

            if (row <= 7 && col <= 7 && GameBoard.GetPiece(row, col).Color != Color)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
            }

            row = Position.Row + 1;
            col = Position.Col;

            while (row <= 7 && GameBoard.GetPiece(row, col) is Empty)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
                row++;
            }

            if (row <= 7 && GameBoard.GetPiece(row, col).Color != Color)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
            }

            row = Position.Row - 1;

            while (row >= 0 && GameBoard.GetPiece(row, col) is Empty)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
                row--;
            }

            if (row >= 0 && GameBoard.GetPiece(row, col).Color != Color)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
            }

            row = Position.Row;
            col = Position.Col + 1;

            while (col <= 7 && GameBoard.GetPiece(row, col) is Empty)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
                col++;
            }

            if (col <= 7 && GameBoard.GetPiece(row, col).Color != Color)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
            }

            col = Position.Col - 1;

            while (col >= 0 && GameBoard.GetPiece(row, col) is Empty)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
                col--;
            }

            if (col >= 0 && GameBoard.GetPiece(row, col).Color != Color)
            {
                Move move = new Move(Position, new Position(row, col), this, GameBoard.GetPiece(row, col));
                if (!Engine.WillCauseCheck(move)) { moves.Add(move); }
            }

            return moves;
        }
    }
}
